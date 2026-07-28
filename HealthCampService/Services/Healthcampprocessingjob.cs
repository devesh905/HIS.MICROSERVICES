using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using HealthCampMicroservice.Data;
using HealthCampMicroservice.Models.Entities.Portal;
using HealthCampMicroservice.Services;

namespace HealthCampService.BackgroundJobs;

public class HealthCampProcessingJob
{
    private readonly PortalDbContext _portalDb;
    private readonly IHospitalQueryService _hospitals;
    private readonly ILogger<HealthCampProcessingJob> _logger;
    private readonly IDoctorScheduleService _doctorSchedule;

    private const int TmtServiceId = 2347;
    private const string TmtServiceTyp = "C";        /// "create new OpNo + Patient_Consultancy" branch
    private const string TmtServiceSerType = "S";        
    private const int HapSponsorId = 33;                /// Health Assurance Policy
    private const int VerticalId = 9;                  /// STAFF
    private const int OrgId = 2;                   
    private const string OpdType = "GENERAL";        

    public HealthCampProcessingJob(
        PortalDbContext portalDb,
        IHospitalQueryService hospitals,
        ILogger<HealthCampProcessingJob> logger,
        IDoctorScheduleService doctorSchedule)
    {
        _portalDb = portalDb;
        _hospitals = hospitals;
        _logger = logger;
        _doctorSchedule = doctorSchedule;
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        var today = DateTime.Today;

        var duePendingBookings = await _portalDb.HealthCampBookings
            .Include(b => b.Slot)
            .Where(b =>
                b.Status == HealthCampBookingStatus.Booked &&
                b.ProcessingStatus == HealthCampProcessingStatus.Pending &&
                b.Slot!.SlotDate.Date == today &&
                !b.IsNewRegistration) // existing-UHID only, per current scope
            .ToListAsync(ct);

        _logger.LogInformation("HealthCampProcessingJob: {Count} bookings due for {Date}",
            duePendingBookings.Count, today.ToShortDateString());

        foreach (var booking in duePendingBookings)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                await ProcessSingleBookingAsync(booking, ct);
            }
            catch (Exception ex)
            {
                // Never let one bad booking kill the whole batch.
                _logger.LogError(ex, "Unhandled error processing booking {RefNo}", booking.BookingRefNo);
                booking.ProcessingStatus = HealthCampProcessingStatus.Failed;
                booking.ProcessingError = Truncate(ex.Message, 500);
                booking.ModifiedOn = DateTime.Now;
                await _portalDb.SaveChangesAsync(ct);
            }
        }
    }

    private async Task ProcessSingleBookingAsync(HealthCampBooking booking, CancellationToken ct)
    {
        if (booking.HospitalKey != "MRT") /// Meerut
        {
            _logger.LogWarning("Booking {RefNo} is for hospital {Key} — HAP camp billing is currently MRT-only.",
                booking.BookingRefNo, booking.HospitalKey);
            booking.ProcessingStatus = HealthCampProcessingStatus.Failed;
            booking.ProcessingError = "HAP camp processing is currently enabled for Meerut (MRT) only.";
            booking.ModifiedOn = DateTime.Now;
            await _portalDb.SaveChangesAsync(ct);
            return;
        }

        if (string.IsNullOrWhiteSpace(booking.UHID))
        {
            _logger.LogWarning("Booking {RefNo} has no UHID — cannot bill. Marking Failed.", booking.BookingRefNo);
            booking.ProcessingStatus = HealthCampProcessingStatus.Failed;
            booking.ProcessingError = "Missing UHID at processing time.";
            booking.ModifiedOn = DateTime.Now;
            await _portalDb.SaveChangesAsync(ct);
            return;
        }

        var hospital = HospitalRegistry.Find(booking.HospitalKey);
        if (hospital == null)
        {
            _logger.LogError("Unknown hospitalKey {HospitalKey} for booking {RefNo}", booking.HospitalKey, booking.BookingRefNo);
            booking.ProcessingStatus = HealthCampProcessingStatus.Failed;
            booking.ProcessingError = $"Unknown hospitalKey: {booking.HospitalKey}";
            booking.ModifiedOn = DateTime.Now;
            await _portalDb.SaveChangesAsync(ct);
            return;
        }

        await using var ctx = _hospitals.CreateHisContext(hospital.HisCs);

        // Confirm the patient actually exists and pull latest VisitNo so we pass VisitNo > 1
        // (this is what makes HMS_Opd_Billing_Create reuse-and-increment correctly rather
        // than treating it as a brand new patient's first visit).
        var patient = await ctx.PatientRegistrations
            .Where(p => p.UhidNo == booking.UHID && p.OrgId == OrgId)
            .OrderByDescending(p => p.RegistrationDate)
            .FirstOrDefaultAsync(ct);

        if (patient == null)
        {
            _logger.LogWarning("UHID {Uhid} not found in {Hospital} HIS DB for booking {RefNo}",
                booking.UHID, booking.HospitalKey, booking.BookingRefNo);
            booking.ProcessingStatus = HealthCampProcessingStatus.Failed;
            booking.ProcessingError = $"UHID {booking.UHID} not found in HIS DB.";
            booking.ModifiedOn = DateTime.Now;
            await _portalDb.SaveChangesAsync(ct);
            return;
        }

        var visitNo = (short)(patient.VisitNo + 1);
        var rate = await ctx.Database.SqlQuery<decimal>(
                    $"SELECT Opd_Charges AS Value FROM Service_Master WHERE Id = {TmtServiceId}").FirstOrDefaultAsync(ct);

        if (rate <= 0)
        {
            _logger.LogError("TMT service rate lookup failed or returned 0 for booking {RefNo}", booking.BookingRefNo);
            booking.ProcessingStatus = HealthCampProcessingStatus.Failed;
            booking.ProcessingError = "Could not resolve TMT service rate.";
            booking.ModifiedOn = DateTime.Now;
            await _portalDb.SaveChangesAsync(ct);
            return;
        }

        const int CardiologyDeptId = 2;

        var availableDoctors = await _doctorSchedule.GetAvailableDoctorsAsync(
            booking.HospitalKey, CardiologyDeptId, booking.Slot!.SlotDate);

        if (availableDoctors.Count == 0)
        {
            _logger.LogWarning("No scheduled Cardiology doctor for {Hospital} on {Date} — booking {RefNo}",
                booking.HospitalKey, booking.Slot.SlotDate, booking.BookingRefNo);
            booking.ProcessingStatus = HealthCampProcessingStatus.Failed;
            booking.ProcessingError = "No doctor scheduled for TMT on this date. Contact admin to assign a schedule.";
            booking.ModifiedOn = DateTime.Now;
            await _portalDb.SaveChangesAsync(ct);
            return;
        }

        var doctorId = availableDoctors.Any(d => d.DoctorId == booking.DoctorId)
            ? booking.DoctorId!.Value
            : availableDoctors[0].DoctorId;

        var alreadyBilled = await ctx.Database.SqlQuery<int>(
             $"SELECT COUNT(*) AS Value FROM Opd_Billing_Mas WHERE UhidNo = {booking.UHID} AND OrgId = {OrgId} AND AppNo = {booking.BookingRefNo}")
             .FirstOrDefaultAsync(ct);

        if (alreadyBilled > 0)
        {
            _logger.LogWarning("Booking {RefNo} already has a bill in Vipha (AppNo match) — marking Processed without re-billing.",
                booking.BookingRefNo);
            booking.ProcessingStatus = HealthCampProcessingStatus.Processed;
            booking.ProcessingError = "Recovered: bill already existed (AppNo match), skipped re-billing.";
            booking.ModifiedOn = DateTime.Now;
            await _portalDb.SaveChangesAsync(ct);
            return;
        }


        // Build the billing detail TVP: one line for TMT
        var billingDet = new DataTable();
        billingDet.Columns.Add("Sno", typeof(int));
        billingDet.Columns.Add("OpDocId", typeof(int));
        billingDet.Columns.Add("Typ", typeof(string));
        billingDet.Columns.Add("SerId", typeof(int));
        billingDet.Columns.Add("Qty", typeof(int));
        billingDet.Columns.Add("Rate", typeof(decimal));
        billingDet.Columns.Add("Amount", typeof(decimal));
        billingDet.Columns.Add("DisAuthId", typeof(int));
        billingDet.Columns.Add("DisId", typeof(int));
        billingDet.Columns.Add("LimitPer", typeof(decimal));
        billingDet.Columns.Add("LimitAmount", typeof(decimal));
        billingDet.Columns.Add("DisPer", typeof(decimal));
        billingDet.Columns.Add("DisAmt", typeof(decimal));
        billingDet.Columns.Add("GstPer", typeof(decimal));
        billingDet.Columns.Add("GstAmt", typeof(decimal));
        billingDet.Columns.Add("NetAmount", typeof(decimal));
        billingDet.Columns.Add("Ser_type", typeof(string));
        billingDet.Columns.Add("UnderServiceId", typeof(int));
        billingDet.Columns.Add("ActualRate", typeof(decimal));
        billingDet.Columns.Add("Bill_Discount", typeof(decimal));
        billingDet.Columns.Add("Can_Status", typeof(string));
        billingDet.Columns.Add("IsPartial_Discount", typeof(bool));

        billingDet.Rows.Add(
            1,                          
            doctorId,                   /// OpDocId — resolved scheduled doctor
            "S",                        
            TmtServiceId,               
            1,                          
            rate,                       
            rate,                  
            0, 0,                       
            0m, 0m,                     
            0m, 0m,                     
            0m, 0m,                    
            rate,                       
            TmtServiceSerType,          
            0,                        
            0m,                        
            0m,                      
            "N",                        
            false                      
        );

        var emptyDiscountDet = new DataTable();
        emptyDiscountDet.Columns.Add("Sno", typeof(int));
        emptyDiscountDet.Columns.Add("DisAuthId", typeof(int));
        emptyDiscountDet.Columns.Add("DisId", typeof(int));
        emptyDiscountDet.Columns.Add("DisPer", typeof(decimal));
        emptyDiscountDet.Columns.Add("DisAmt", typeof(decimal));
        // no rows — no manual discount for HAP camp bookings

        var pId = new SqlParameter("@Id", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
        var pBillNo = new SqlParameter("@BillNo", SqlDbType.VarChar, 30) { Direction = ParameterDirection.Output };
        var pOpNo = new SqlParameter("@OpNo", SqlDbType.NVarChar, 18) { Direction = ParameterDirection.Output };
        var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
        var pMessage = new SqlParameter("@Message", SqlDbType.VarChar, -1) { Direction = ParameterDirection.Output };

        var parameters = new[]
        {
            pId,
            new SqlParameter("@AppNo", SqlDbType.VarChar, 30) { Value = booking.BookingRefNo },
            pBillNo,
            pOpNo,
            new SqlParameter("@Typ", SqlDbType.NVarChar, 3) { Value = TmtServiceTyp },
            new SqlParameter("@Bill_Date", SqlDbType.DateTime) { Value = DateTime.Now },
            new SqlParameter("@Timein", SqlDbType.VarChar, 20) { Value = DateTime.Now.ToString("HH:mm") },
            new SqlParameter("@UhidNo", SqlDbType.VarChar, 30) { Value = booking.UHID },
            new SqlParameter("@VisitNo", SqlDbType.Int) { Value = (int)visitNo },
            new SqlParameter("@QueueNo", SqlDbType.Int) { Value = 0 },
            new SqlParameter("@DocId", SqlDbType.Int) { Value = doctorId },
            new SqlParameter("@SponsId", SqlDbType.Int) { Value = HapSponsorId },
            new SqlParameter("@OpdType", SqlDbType.VarChar, 20) { Value = OpdType },
            new SqlParameter("@DepId", SqlDbType.Int) { Value = CardiologyDeptId },
            new SqlParameter("@Card_no", SqlDbType.VarChar, 20) { Value = "" },
            new SqlParameter("@ClaimId", SqlDbType.VarChar, 20) { Value = "" },
            new SqlParameter("@RefId", SqlDbType.Int) { Value = 1 },    /// SUBHARTI HOSPITAL
            new SqlParameter("@Cash", SqlDbType.Bit) { Value = true },
            new SqlParameter("@Credit", SqlDbType.Bit) { Value = true },   
            new SqlParameter("@Free", SqlDbType.Bit) { Value = false  },
            new SqlParameter("@PayMode", SqlDbType.VarChar, 20) { Value = "Credit" },
            new SqlParameter("@AccId", SqlDbType.Int) { Value = 0 },
            new SqlParameter("@ChequeNo", SqlDbType.VarChar, 20) { Value = "" },
            new SqlParameter("@Auth_code", SqlDbType.VarChar, 30) { Value = "" },
            new SqlParameter("@Qty", SqlDbType.Int) { Value = 1 },
            new SqlParameter("@TotalAmt", SqlDbType.Decimal) { Value = rate },
            new SqlParameter("@Ser_Cons", SqlDbType.Decimal) { Value = 0m, Precision = 18, Scale = 2 },
            new SqlParameter("@NetAmount", SqlDbType.Decimal) { Value = rate  },
            new SqlParameter("@Con_Amount", SqlDbType.Decimal) {Value = 0m},
            new SqlParameter("@Receipt_Amount1", SqlDbType.Decimal) { Value = 0m },
            new SqlParameter("@PayMode2", SqlDbType.VarChar, 20) { Value = "" },
            new SqlParameter("@AccountId2", SqlDbType.Int) { Value = 0 },
            new SqlParameter("@ChequeNo2", SqlDbType.VarChar, 20) { Value = "" },
            new SqlParameter("@Auth_Code2", SqlDbType.VarChar, 30) { Value = "" },
            new SqlParameter("@Receipt_Amount2", SqlDbType.Decimal) { Value = 0m },
            new SqlParameter("@Receipt_Amount", SqlDbType.Decimal) {Value = 0m},
            new SqlParameter("@Balance", SqlDbType.Decimal) { Value = rate  },
            new SqlParameter("@Round_off", SqlDbType.Decimal) { Value = 0m, Precision = 18, Scale = 2 },
            new SqlParameter("@Remarks", SqlDbType.VarChar, 255) { Value = $"HAP Health Camp - {booking.BookingRefNo}" },
            new SqlParameter("@OpdBillingDet", SqlDbType.Structured) { TypeName = "dbo.OpdBillingDetails", Value = billingDet },
            new SqlParameter("@OpdBillingDiscountDet", SqlDbType.Structured) { TypeName = "dbo.OpdBillingDiscountDetails", Value = emptyDiscountDet },
            new SqlParameter("@OrgId", SqlDbType.SmallInt) { Value = (short)OrgId },
            new SqlParameter("@FinYr", SqlDbType.SmallInt) { Value = (short)0 },
            new SqlParameter("@UserId", SqlDbType.Int) { Value = 1993 }, 
            new SqlParameter("@Ac_Jr_Id", SqlDbType.Int) { Value = 0 },
            new SqlParameter("@VerticalId", SqlDbType.Int) { Value = VerticalId },
            new SqlParameter("@SystemName", SqlDbType.NVarChar, 50) { Value = $"{Environment.MachineName}#HIS.HEALTHCAMP" },
            new SqlParameter("@OpNo1", SqlDbType.NVarChar, 20) { Value = "" },
            new SqlParameter("@OpdReqNo", SqlDbType.NVarChar, 20) { Value = "" },
            new SqlParameter("@Indent_No", SqlDbType.NVarChar, 25) { Value = "" },
            new SqlParameter("@OutSourced", SqlDbType.Bit) { Value = false },
            new SqlParameter("@OSAccountId", SqlDbType.Int) { Value = 0 },
            new SqlParameter("@TDSAmount", SqlDbType.Decimal) { Value = 0m, Precision = 18, Scale = 2 },
            new SqlParameter("@GSTAmount", SqlDbType.Decimal) { Value = 0m, Precision = 18, Scale = 2 },
            new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = 1993 }, 
            pSuccess,
            pMessage,
        };

        const string paramString =
            "@Id OUTPUT, @AppNo, @BillNo OUTPUT, @OpNo OUTPUT, @Typ, @Bill_Date, @Timein, @UhidNo, " +
            "@VisitNo, @QueueNo, @DocId, @SponsId, @OpdType, @DepId, @Card_no, @ClaimId, @RefId, " +
            "@Cash, @Credit, @Free, @PayMode, @AccId, @ChequeNo, @Auth_code, @Qty, @TotalAmt, " +
            "@Ser_Cons, @NetAmount, @Con_Amount, @Receipt_Amount1, @PayMode2, @AccountId2, " +
            "@ChequeNo2, @Auth_Code2, @Receipt_Amount2, @Receipt_Amount, @Balance, @Round_off, " +
            "@Remarks, @OpdBillingDet, @OpdBillingDiscountDet, @OrgId, @FinYr, @UserId, @Ac_Jr_Id, " +
            "@VerticalId, @SystemName, @OpNo1, @OpdReqNo, @Indent_No, @OutSourced, @OSAccountId, " +
            "@TDSAmount, @GSTAmount, @CreatedBy, @Success OUTPUT, @Message OUTPUT";

        try
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "EXEC HMS_Opd_Billing_Create " + paramString, parameters, ct);

            var success = pSuccess.Value != DBNull.Value && (bool)pSuccess.Value;
            var message = pMessage.Value?.ToString() ?? "";

            if (success)
            {
                booking.ProcessingStatus = HealthCampProcessingStatus.Processed;
                booking.ResultUhidNo = booking.UHID;
                booking.ResultOpNo = pOpNo.Value?.ToString();
                booking.ResultBillNo = pBillNo.Value?.ToString();
                booking.ProcessedOn = DateTime.Now;
                booking.ProcessingError = null;

                _logger.LogInformation(
                    "Booking {RefNo} processed OK. UHID={Uhid} OpNo={OpNo} BillNo={BillNo}",
                    booking.BookingRefNo, booking.UHID, pOpNo.Value, pBillNo.Value);
            }
            else if (message.Contains("Duplicate Entry Not Allowed Before 20 Seconds", StringComparison.OrdinalIgnoreCase))
            {
                // SP-level dedup window tripped, most likely from a job retry.
                // Don't mark Failed — leave Pending so the NEXT run (after the 20s window) picks it up
                // instead of us looping tight retries against the same window.
                _logger.LogWarning(
                    "Booking {RefNo} hit SP 20s duplicate window. Will retry on next run.",
                    booking.BookingRefNo);
                booking.ProcessingError = "Retrying: SP duplicate-window (20s) triggered.";
                booking.ModifiedOn = DateTime.Now;
            }
            else
            {
                booking.ProcessingStatus = HealthCampProcessingStatus.Failed;
                booking.ProcessingError = Truncate(message, 500);
                booking.ModifiedOn = DateTime.Now;

                _logger.LogWarning("Booking {RefNo} SP call failed: {Message}", booking.BookingRefNo, message);
            }

            await _portalDb.SaveChangesAsync(ct);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL error processing booking {RefNo}", booking.BookingRefNo);
            booking.ProcessingStatus = HealthCampProcessingStatus.Failed;
            booking.ProcessingError = Truncate(sqlEx.Message, 500);
            booking.ModifiedOn = DateTime.Now;
            await _portalDb.SaveChangesAsync(ct);
        }
    }

    private static string Truncate(string s, int max) =>
        string.IsNullOrEmpty(s) || s.Length <= max ? s : s[..max];
}