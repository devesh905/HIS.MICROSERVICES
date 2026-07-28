using HIS.API.Data;
using HIS.API.Models.DTOs;
using HIS.API.Models.DTOs.Portal;
using HIS.API.Models.Entities.Hms;
using HIS.API.Models.Entities.Portal;
using HIS.API.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HIS.API.Services;

public class HealthCampService : IHealthCampService
{
    private readonly PortalDbContext _db;
    private readonly ILogger<HealthCampService> _logger;
    private readonly IHospitalQueryService _hospitals;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public HealthCampService(PortalDbContext db, ILogger<HealthCampService> logger, IHospitalQueryService hospitals)
    {
        _db = db;
        _logger = logger;
        _hospitals = hospitals;
    }

    public async Task<List<HealthCampTypeDto>> GetActiveCampTypesAsync(string hospitalKey)
    {
        return await _db.HealthCampTypes
            .Where(x => x.HospitalKey == hospitalKey && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new HealthCampTypeDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsEmployeeOnly = x.IsEmployeeOnly,
                DefaultDoctorId = x.DefaultDoctorId,
                DefaultDepartmentId = x.DefaultDepartmentId
            })
            .ToListAsync();
    }

    public async Task<List<SlotAvailabilityDto>> GetSlotAvailabilityAsync(
        string hospitalKey, int campTypeId, DateTime fromDate, DateTime toDate)
    {
        var now = DateTime.Now;

        var slots = await _db.HealthCampSlots
            .Where(x => x.HospitalKey == hospitalKey
                     && x.CampTypeId == campTypeId
                     && x.IsActive
                     && x.SlotDate >= fromDate.Date
                     && x.SlotDate <= toDate.Date)
            .OrderBy(x => x.SlotDate).ThenBy(x => x.StartTime)
            .ToListAsync();

        // Sunday filter applied client-side after materialization
        slots = slots.Where(x => x.SlotDate.DayOfWeek != DayOfWeek.Sunday).ToList();

        return slots.Select(x => new SlotAvailabilityDto
        {
            SlotId = x.Id,
            SlotDate = x.SlotDate,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            Capacity = x.Capacity,
            BookedCount = x.BookedCount,
            AvailableCount = x.Capacity - x.BookedCount,
            IsFull = x.BookedCount >= x.Capacity,
            /// Past-time = slot is today AND its end time has already passed.
            IsPastTime = x.SlotDate.Date == now.Date && x.EndTime <= now.TimeOfDay
        }).ToList();
    }

    /// Books a seat in a slot. Concurrency-safe via UPDLOCK/ROWLOCK on the slot row.
    /// Supports two identity paths:
    ///   - Existing UHID: request.UHID is set, IsNewRegistration = false.
    ///   - New registration: request.NewRegistration is serialized to JSON and
    ///     stored for the visit-date job to consume via HMS_PatientRegistration_Create.
    /// No Vipha writes happen here - PortalDb only.
    public async Task<BookingResultDto> CreateBookingAsync(CreateBookingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PatientName) || string.IsNullOrWhiteSpace(request.MobileNo))
        {
            return Fail("Patient name and mobile number are required.");
        }

        if (!request.IsNewRegistration && string.IsNullOrWhiteSpace(request.UHID))
        {
            return Fail("Please select an existing profile or choose new registration.");
        }

        if (request.IsNewRegistration && request.NewRegistration == null)
        {
            return Fail("Registration details are required for a new patient.");
        }

        // Resolve doctor/department: explicit request wins, else fall back to camp type defaults
        var (doctorId, departmentId) = await ResolveDoctorDepartmentAsync(request);

        var strategy = _db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted);

            try
            {
                var slot = await _db.HealthCampSlots
                    .FromSqlRaw(@"SELECT * FROM HealthCampSlot WITH (UPDLOCK, ROWLOCK)
                                  WHERE Id = {0}", request.SlotId)
                    .FirstOrDefaultAsync();

                if (slot == null)
                {
                    await transaction.RollbackAsync();
                    return Fail("Slot not found.");
                }

                if (!slot.IsActive)
                {
                    await transaction.RollbackAsync();
                    return Fail("This slot is no longer available.");
                }

                if (slot.SlotDate.Date < DateTime.Today)
                {
                    await transaction.RollbackAsync();
                    return Fail("Cannot book a slot for a past date.");
                }

                /// block Sunday slots outright (OPD closed, no doctor available)
                if (slot.SlotDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    await transaction.RollbackAsync();
                    return Fail("Bookings are not available on Sundays. OPD is closed.");
                }

                /// block today's slot once its time window has passed
                if (slot.SlotDate.Date == DateTime.Today && slot.EndTime <= DateTime.Now.TimeOfDay)
                {
                    await transaction.RollbackAsync();
                    return Fail("This slot's time has already passed. Please choose another slot.");
                }

                if (slot.BookedCount >= slot.Capacity)
                {
                    await transaction.RollbackAsync();
                    return Fail("This slot is fully booked. Please choose another slot.");
                }

                var alreadyBooked = await _db.HealthCampBookings.AnyAsync(x =>
                    x.SlotId == request.SlotId &&
                    x.MobileNo == request.MobileNo &&
                    x.Status == HealthCampBookingStatus.Booked);

                if (alreadyBooked)
                {
                    await transaction.RollbackAsync();
                    return Fail("A booking already exists for this mobile number in this slot.");
                }

                var refNo = await GenerateBookingRefNoAsync(request.HospitalKey, request.CampTypeId);

                var booking = new HealthCampBooking
                {
                    HospitalKey = request.HospitalKey,
                    SlotId = request.SlotId,
                    CampTypeId = request.CampTypeId,
                    BookingRefNo = refNo,

                    EmployeeId = request.EmployeeId,
                    UHID = request.IsNewRegistration ? null : request.UHID,

                    IsNewRegistration = request.IsNewRegistration,
                    RegistrationPayloadJson = request.IsNewRegistration
                        ? JsonSerializer.Serialize(request.NewRegistration, JsonOpts)
                        : null,

                    DoctorId = doctorId,
                    DepartmentId = departmentId,

                    PatientName = request.PatientName.Trim(),
                    MobileNo = request.MobileNo.Trim(),
                    Email = request.Email,
                    Gender = request.Gender,
                    Age = request.Age,
                    Department = request.Department,

                    Status = HealthCampBookingStatus.Booked,
                    ProcessingStatus = HealthCampProcessingStatus.Pending,
                    CreatedOn = DateTime.Now
                };

                _db.HealthCampBookings.Add(booking);

                slot.BookedCount += 1;
                slot.ModifiedOn = DateTime.Now;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BookingResultDto
                {
                    Success = true,
                    BookingId = booking.Id,
                    BookingRefNo = booking.BookingRefNo,
                    SlotDate = slot.SlotDate,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime
                };
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Duplicate booking attempt for slot {SlotId}", request.SlotId);
                return Fail("A booking already exists for this mobile number in this slot.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Booking failed for slot {SlotId}", request.SlotId);
                return Fail("Booking failed due to a server error. Please try again.");
            }
        });
    }

    public async Task<bool> CancelBookingAsync(string hospitalKey, string bookingRefNo, string? reason, int? cancelledBy)
    {
        var strategy = _db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var booking = await _db.HealthCampBookings.FirstOrDefaultAsync(x =>
                    x.HospitalKey == hospitalKey &&
                    x.BookingRefNo == bookingRefNo &&
                    x.Status == HealthCampBookingStatus.Booked);

                if (booking == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                if (booking.ProcessingStatus == HealthCampProcessingStatus.Processed)
                {
                    // Already has a real OpNo/BillNo in Vipha - don't silently orphan that.
                    // Caller should handle cancellation in Vipha billing separately.
                    await transaction.RollbackAsync();
                    return false;
                }

                var slot = await _db.HealthCampSlots
                    .FromSqlRaw(@"SELECT * FROM HealthCampSlot WITH (UPDLOCK, ROWLOCK)
                                  WHERE Id = {0}", booking.SlotId)
                    .FirstOrDefaultAsync();

                booking.Status = HealthCampBookingStatus.Cancelled;
                booking.CancelledOn = DateTime.Now;
                booking.CancelReason = reason;
                booking.ModifiedBy = cancelledBy;
                booking.ModifiedOn = DateTime.Now;

                if (slot != null && slot.BookedCount > 0)
                {
                    slot.BookedCount -= 1;
                    slot.ModifiedOn = DateTime.Now;
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Cancel booking failed for {RefNo}", bookingRefNo);
                return false;
            }
        });
    }

    public async Task<BookingListItemDto?> GetBookingByRefNoAsync(string hospitalKey, string bookingRefNo)
    {
        return await _db.HealthCampBookings
            .Include(x => x.Slot)
            .Where(x => x.HospitalKey == hospitalKey && x.BookingRefNo == bookingRefNo)
            .Select(x => MapToListItem(x))
            .FirstOrDefaultAsync();
    }

    public async Task<List<BookingListItemDto>> GetBookingsBySlotAsync(int slotId)
    {
        return await _db.HealthCampBookings
            .Include(x => x.Slot)
            .Where(x => x.SlotId == slotId)
            .OrderBy(x => x.CreatedOn)
            .Select(x => MapToListItem(x))
            .ToListAsync();
    }

    public async Task<List<BookingListItemDto>> GetBookingsByMobileAsync(string hospitalKey, string mobileNo)
    {
        return await _db.HealthCampBookings
            .Include(x => x.Slot)
            .Where(x => x.HospitalKey == hospitalKey && x.MobileNo == mobileNo)
            .OrderByDescending(x => x.CreatedOn)
            .Select(x => MapToListItem(x))
            .ToListAsync();
    }


    public async Task<int> CreateSlotAsync(CreateSlotRequest request, int? createdBy)
    {
        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("EndTime must be after StartTime.");

        if (request.Capacity < 0)
            throw new ArgumentException("Capacity cannot be negative.");

        var existing = await GetActiveSlotsForDateAsync(request.HospitalKey, request.CampTypeId, request.SlotDate);
        var conflict = existing.FirstOrDefault(x => TimesOverlap(x.StartTime, x.EndTime, request.StartTime, request.EndTime));
        if (conflict != null)
            throw new ArgumentException($"Overlaps an existing slot ({conflict.StartTime:hh\\:mm}-{conflict.EndTime:hh\\:mm}).");


        var slot = new HealthCampSlot
        {
            HospitalKey = request.HospitalKey,
            CampTypeId = request.CampTypeId,
            SlotDate = request.SlotDate.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Capacity = request.Capacity,
            BookedCount = 0,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedOn = DateTime.Now
        };

        _db.HealthCampSlots.Add(slot);
        await _db.SaveChangesAsync();
        return slot.Id;
    }

    public async Task<bool> MarkCardGeneratedAsync(string bookingRefNo, string sentVia)
    {
        var booking = await _db.HealthCampBookings
            .FirstOrDefaultAsync(x => x.BookingRefNo == bookingRefNo);

        if (booking == null) return false;

        booking.CardGenerated = true;
        booking.CardSentOn = DateTime.Now;
        booking.CardSentVia = sentVia;
        booking.ModifiedOn = DateTime.Now;

        await _db.SaveChangesAsync();
        return true;
    }

    /// scoped to exactly one BillNo so the frontend never has to disambiguate a list.
    public async Task<BillDto?> GetReceiptByBillNoAsync(string hospitalKey, string billNo)
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null) return null;

        await using var db = _hospitals.CreateHisContext(entry.HisCs);
        await using var lis = _hospitals.CreateLisContext(entry.LisCs);

        var mas = await db.OpdBillingMas.FirstOrDefaultAsync(b => b.BillNo == billNo);
        if (mas == null) return null;

        var det = await db.OpdBillingDet
            .Where(d => d.BillNo == billNo)
            .OrderBy(d => d.Sno)
            .ToListAsync();

        // Name lookups
        var labSerIds = det.Where(d => d.Typ == "L" && d.SerId.HasValue).Select(d => d.SerId!.Value).Distinct().ToList();
        var hisSerIds = det.Where(d => d.Typ != "L" && d.SerId.HasValue).Select(d => d.SerId!.Value).Distinct().ToList();

        var testDict = labSerIds.Any()
            ? await lis.TestMas.Where(t => labSerIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id, t => t.Test_Name)
            : new Dictionary<int, string?>();

        var serviceDict = hisSerIds.Any()
            ? await db.ServiceMasters.Where(s => hisSerIds.Contains((int)s.Id)).ToDictionaryAsync(s => (int)s.Id, s => s.Ser_Name)
            : new Dictionary<int, string?>();

        /// Patient lookup by UHID (not OpNo — repeat-visit OpNo isn't written back to PatientRegistration)
        var patient = !string.IsNullOrWhiteSpace(mas.UhidNo)
            ? await db.PatientRegistrations
                .Where(r => r.UhidNo == mas.UhidNo)
                .OrderByDescending(r => r.RegistrationDate)
                .FirstOrDefaultAsync()
            : null;

        var doctorName = mas.DocId.HasValue
            ? await db.DoctorMasters.Where(d => d.Id == mas.DocId.Value).Select(d => d.Doc_Name).FirstOrDefaultAsync()
            : null;

        var deptName = mas.DepId.HasValue
            ? await db.DepartmentMas.Where(d => d.Id == mas.DepId.Value).Select(d => d.Dept_Name).FirstOrDefaultAsync()
            : null;

        var sponsorName = mas.SponsId.HasValue
            ? await db.SponsorMasters.Where(s => s.Id == mas.SponsId.Value).Select(s => s.Sponsor_Name).FirstOrDefaultAsync()
            : null;


        string? receivedByName = null;
        if (mas.UserId.HasValue)
        {
            await using var viphaCtx = _hospitals.CreateViphaContext(entry.ViphaCs);
            receivedByName = await viphaCtx.Users
                .Where(u => u.Id == mas.UserId.Value)
                .Select(u => u.UserName ?? (u.FirstName + " " + u.LastName))
                .FirstOrDefaultAsync();
        }

        string ResolveName(OpdBillingDet i)
        {
            if (!i.SerId.HasValue) return "Unknown";
            if (i.Typ == "L")
                return testDict.TryGetValue(i.SerId.Value, out var tn) && tn != null ? tn : $"Lab Test {i.SerId}";
            return serviceDict.TryGetValue(i.SerId.Value, out var sn) && sn != null ? sn : $"Service {i.SerId}";
        }

        return new BillDto
        {
            Id = mas.Id,
            BillNo = mas.BillNo,
            Bill_Date = mas.Bill_Date,
            Timein = mas.Timein,
            UhidNo = mas.UhidNo,
            OpNo = mas.OpNo,
            VisitNo = mas.VisitNo,
            PayMode = mas.PayMode,
            PayMode2 = mas.PayMode2,
            TotalAmt = mas.TotalAmt,
            NetAmount = mas.NetAmount,
            Receipt_Amount = mas.Receipt_Amount,
            Balance = mas.Balance,
            GSTAmount = mas.GSTAmount,
            Round_off = mas.Round_off,
            IsCancel = mas.IsCancel,
            CancelRemarks = mas.CancelRemarks,
            Remarks = mas.Remarks,
            BillSource = "OPD",
            PatientName = patient?.PatientName,
            AgeSex = patient != null ? $"{patient.AgeInYears} Y / {patient.Gender}" : null,
            Mobile = patient?.MobileNo,
            Address = patient?.Address,                 
            DepartmentName = deptName,                        
            ReceivedByName = receivedByName,            
            ConsultantName = doctorName,
            SponsorName = sponsorName,
            Items = det.Select(i => new BillDetailItemDto
            {
                Sno = i.Sno,
                Typ = i.Typ,
                SerName = ResolveName(i),
                Qty = i.Qty,
                Rate = i.Rate,
                Amount = i.Amount,
                DisPer = i.DisPer,
                DisAmt = i.DisAmt,
                NetAmount = i.NetAmount,
                GstPer = i.GstPer,
                GstAmt = i.GstAmt,
                Can_Status = i.Can_Status
            }).ToList()
        };
    }

    public async Task<List<SlotAdminListDto>> GetSlotsForAdminAsync(
string hospitalKey, int? campTypeId, DateTime fromDate, DateTime toDate)
    {
        var query = _db.HealthCampSlots
            .Include(x => x.CampType)
            .Where(x => x.HospitalKey == hospitalKey
                     && x.SlotDate >= fromDate.Date
                     && x.SlotDate <= toDate.Date);

        if (campTypeId.HasValue)
            query = query.Where(x => x.CampTypeId == campTypeId.Value);

        return await query
            .OrderBy(x => x.SlotDate).ThenBy(x => x.StartTime)
            .Select(x => new SlotAdminListDto
            {
                SlotId = x.Id,
                HospitalKey = x.HospitalKey,
                CampTypeId = x.CampTypeId,
                CampTypeName = x.CampType!.Name,
                SlotDate = x.SlotDate,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                Capacity = x.Capacity,
                BookedCount = x.BookedCount,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    /// Edits slot timing/capacity. Capacity cannot drop below BookedCount -
    /// that would silently invalidate existing bookings.
    public async Task<bool> UpdateSlotAsync(int slotId, UpdateSlotRequest request)
    {
        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("EndTime must be after StartTime.");

        var slot = await _db.HealthCampSlots.FirstOrDefaultAsync(x => x.Id == slotId);
        if (slot == null) return false;

        if (request.Capacity < slot.BookedCount)
            throw new ArgumentException(
                $"Capacity cannot be less than the current booked count ({slot.BookedCount}).");

        var existing = await GetActiveSlotsForDateAsync(slot.HospitalKey, slot.CampTypeId, request.SlotDate, excludeSlotId: slotId);
        var conflict = existing.FirstOrDefault(x => TimesOverlap(x.StartTime, x.EndTime, request.StartTime, request.EndTime));
        if (conflict != null)
            throw new ArgumentException($"Overlaps an existing slot ({conflict.StartTime:hh\\:mm}-{conflict.EndTime:hh\\:mm}).");

        slot.SlotDate = request.SlotDate.Date;
        slot.StartTime = request.StartTime;
        slot.EndTime = request.EndTime;
        slot.Capacity = request.Capacity;
        slot.ModifiedBy = request.ModifiedBy;
        slot.ModifiedOn = DateTime.Now;

        await _db.SaveChangesAsync();
        return true;
    }

    /// Soft delete - flips IsActive off. Blocked if there are still active
    /// (non-cancelled) bookings against the slot, so history/receipts stay intact
    /// and reception isn't left with orphaned bookings on a hidden slot.
    public async Task<SlotDeactivateResultDto> DeactivateSlotAsync(int slotId, int? modifiedBy)
    {
        var slot = await _db.HealthCampSlots.FirstOrDefaultAsync(x => x.Id == slotId);
        if (slot == null)
            return new SlotDeactivateResultDto { Success = false, ErrorMessage = "Slot not found." };

        var activeBookingCount = await _db.HealthCampBookings.CountAsync(x =>
            x.SlotId == slotId && x.Status == HealthCampBookingStatus.Booked);

        if (activeBookingCount > 0)
            return new SlotDeactivateResultDto
            {
                Success = false,
                ErrorMessage = $"Cannot remove: {activeBookingCount} active booking(s) exist for this slot. Cancel them first."
            };

        slot.IsActive = false;
        slot.ModifiedBy = modifiedBy;
        slot.ModifiedOn = DateTime.Now;

        await _db.SaveChangesAsync();
        return new SlotDeactivateResultDto { Success = true };
    }

    public async Task<BulkCreateSlotsResultDto> BulkCreateSlotsAsync(BulkCreateSlotsRequest request)
    {
        if (request.ToDate.Date < request.FromDate.Date)
            throw new ArgumentException("ToDate must be on or after FromDate.");

        if (request.TimeSlots == null || !request.TimeSlots.Any())
            throw new ArgumentException("At least one time slot is required.");

        if (request.TimeSlots.Count > 7)
            throw new ArgumentException("A maximum of 7 time slots per day is allowed.");

        foreach (var ts in request.TimeSlots)
        {
            if (ts.EndTime <= ts.StartTime)
                throw new ArgumentException($"EndTime must be after StartTime ({ts.StartTime} - {ts.EndTime}).");
            if (ts.Capacity < 0)
                throw new ArgumentException("Capacity cannot be negative.");
        }

        // reject duplicate time ranges within the submitted set itself
        for (int i = 0; i < request.TimeSlots.Count; i++)
            for (int j = i + 1; j < request.TimeSlots.Count; j++)
                if (TimesOverlap(request.TimeSlots[i].StartTime, request.TimeSlots[i].EndTime,
                                  request.TimeSlots[j].StartTime, request.TimeSlots[j].EndTime))
                    throw new ArgumentException("Two of the submitted time slots overlap with each other.");

        var result = new BulkCreateSlotsResultDto();
        var toInsert = new List<HealthCampSlot>();

        // Pre-load all existing active slots for this campType across the whole date range once,
        // instead of querying per-day - fewer round trips for a 7+ day bulk create.
        var existingSlots = await _db.HealthCampSlots
            .Where(x => x.HospitalKey == request.HospitalKey
                     && x.CampTypeId == request.CampTypeId
                     && x.IsActive
                     && x.SlotDate >= request.FromDate.Date
                     && x.SlotDate <= request.ToDate.Date)
            .ToListAsync();

        for (var date = request.FromDate.Date; date <= request.ToDate.Date; date = date.AddDays(1))
        {
            if (request.SkipSundays && date.DayOfWeek == DayOfWeek.Sunday)
            {
                result.SkippedSundayCount++;
                continue;
            }

            var dayExisting = existingSlots.Where(x => x.SlotDate == date).ToList();

            foreach (var ts in request.TimeSlots)
            {
                var conflict = dayExisting.FirstOrDefault(x =>
                    TimesOverlap(x.StartTime, x.EndTime, ts.StartTime, ts.EndTime));

                if (conflict != null)
                {
                    result.SkippedOverlapCount++;
                    result.OverlapDetails.Add(
                        $"{date:dd MMM}: {ts.StartTime:hh\\:mm}-{ts.EndTime:hh\\:mm} overlaps existing {conflict.StartTime:hh\\:mm}-{conflict.EndTime:hh\\:mm}");
                    continue;
                }

                toInsert.Add(new HealthCampSlot
                {
                    HospitalKey = request.HospitalKey,
                    CampTypeId = request.CampTypeId,
                    SlotDate = date,
                    StartTime = ts.StartTime,
                    EndTime = ts.EndTime,
                    Capacity = ts.Capacity,
                    BookedCount = 0,
                    IsActive = true,
                    CreatedBy = request.CreatedBy,
                    CreatedOn = DateTime.Now
                });
            }
        }

        if (toInsert.Any())
        {
            _db.HealthCampSlots.AddRange(toInsert);
            await _db.SaveChangesAsync();
        }

        result.CreatedCount = toInsert.Count;
        return result;
    }

    // helpers

    private async Task<(int? doctorId, int? departmentId)> ResolveDoctorDepartmentAsync(CreateBookingRequest request)
    {
        if (request.DoctorId.HasValue || request.DepartmentId.HasValue)
            return (request.DoctorId, request.DepartmentId);

        var campType = await _db.HealthCampTypes.FirstOrDefaultAsync(x => x.Id == request.CampTypeId);
        return (campType?.DefaultDoctorId, campType?.DefaultDepartmentId);
    }

    private async Task<string> GenerateBookingRefNoAsync(string hospitalKey, int campTypeId)
    {
        var campType = await _db.HealthCampTypes.FirstOrDefaultAsync(x => x.Id == campTypeId);
        var prefix = campType?.Code ?? "CMP";
        var datePart = DateTime.Now.ToString("yyMMdd");

        var countToday = await _db.HealthCampBookings.CountAsync(x =>
            x.HospitalKey == hospitalKey &&
            x.CampTypeId == campTypeId &&
            x.CreatedOn.Date == DateTime.Today);

        var seq = (countToday + 1).ToString("D4");
        return $"{prefix}{datePart}{seq}";
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sqlEx &&
               (sqlEx.Number == 2601 || sqlEx.Number == 2627);
    }

    private static BookingListItemDto MapToListItem(HealthCampBooking x) => new()
    {
        BookingId = x.Id,
        BookingRefNo = x.BookingRefNo,
        PatientName = x.PatientName,
        MobileNo = x.MobileNo,
        Department = x.Department,
        SlotDate = x.Slot!.SlotDate,
        StartTime = x.Slot.StartTime,
        EndTime = x.Slot.EndTime,
        Status = x.Status,
        CardGenerated = x.CardGenerated,
        ProcessingStatus = x.ProcessingStatus,
        ResultUhidNo = x.ResultUhidNo,
        ResultOpNo = x.ResultOpNo,
        ResultBillNo = x.ResultBillNo
    };

    private static BookingResultDto Fail(string message) => new()
    {
        Success = false,
        ErrorMessage = message
    };

    /// Two time ranges overlap if start1 < end2 AND start2 < end1.
    /// Checked against active slots only, same hospital+campType+date.
    private static bool TimesOverlap(TimeSpan s1, TimeSpan e1, TimeSpan s2, TimeSpan e2)
        => s1 < e2 && s2 < e1;

    private async Task<List<HealthCampSlot>> GetActiveSlotsForDateAsync(
        string hospitalKey, int campTypeId, DateTime date, int? excludeSlotId = null)
    {
        var query = _db.HealthCampSlots.Where(x =>
            x.HospitalKey == hospitalKey &&
            x.CampTypeId == campTypeId &&
            x.SlotDate == date.Date &&
            x.IsActive);

        if (excludeSlotId.HasValue)
            query = query.Where(x => x.Id != excludeSlotId.Value);

        return await query.ToListAsync();
    }
}