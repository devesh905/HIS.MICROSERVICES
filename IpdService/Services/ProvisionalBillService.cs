using IpdService.Data;
using IpdService.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IpdService.Services;

public class ProvisionalBillService
{
    private readonly HmsDbContext _hisDb;
    private readonly LisDbContext _lisDb;
    private readonly ViphaDbContext _viphaDb;

    public ProvisionalBillService(HmsDbContext hisDb, LisDbContext lisDb, ViphaDbContext viphaDb)
    {
        _hisDb = hisDb;
        _lisDb = lisDb;
        _viphaDb = viphaDb;
    }

    public async Task<ProvisionalBillDto?> GetProvisionalBillAsync(string admNo)
    {
        // 1. Verify admission exists
        var admission = await _hisDb.IpdRegistrations
            .AsNoTracking()
            .Where(i => i.Adm_No == admNo)
            .Select(i => new {
                i.UhidNo,
                i.Adm_Date,
                i.DocId,
                i.Sponsor_Id,
                i.Cur_WardId,
                i.Cur_RoomId,
                i.Cur_Bed,
                i.Pro_Diagnos,
                i.Adm_Type,      // (for AdmType field)
                i.Patient_Type   // (for PatientType field)
            })
            .FirstOrDefaultAsync();

        if (admission == null) return null;

        var patient = await _hisDb.PatientRegistrations
            .AsNoTracking()
            .Where(p => p.UhidNo == admission.UhidNo)
            .Select(p => new { p.PatientName, p.Gender, p.AgeInYears })
            .FirstOrDefaultAsync();

        var doctorName = admission.DocId.HasValue
            ? await _hisDb.DoctorMasters
                .Where(d => d.Id == admission.DocId.Value)
                .Select(d => d.Doc_Name).FirstOrDefaultAsync()
            : null;

        var sponsorName = admission.Sponsor_Id.HasValue
            ? await _hisDb.SponsorMasters
                .Where(s => s.Id == admission.Sponsor_Id.Value)
                .Select(s => s.Sponsor_Name).FirstOrDefaultAsync()
            : null;

        var dto = new ProvisionalBillDto
        {
            AdmNo = admNo,
            UhidNo = admission.UhidNo,
            AdmissionDate = admission.Adm_Date,
            PatientName = patient?.PatientName,
            Gender = patient?.Gender,
            AgeInYears = patient?.AgeInYears,
            WardBed = $"Ward {admission.Cur_WardId} / Room {admission.Cur_RoomId} / Bed {admission.Cur_Bed}",
            DoctorName = doctorName,
            Sponsor = sponsorName,
        };

        var dischargeReq = await _hisDb.DischargeReqs
            .AsNoTracking()
            .Where(d => d.Adm_No == admNo && d.IsCancel != true)
            .OrderByDescending(d => d.DocDate)
            .Select(d => new {
                d.DocNo,
                d.DocDate,
                d.DocTime,
                d.Discharge_Status,
                d.P_Condition,
                d.Discharge_Mode
            })
            .FirstOrDefaultAsync();

        dto.IsDischarge = dischargeReq != null;
        dto.DischargeDocNo = dischargeReq?.DocNo;
        dto.DischargeDate = dischargeReq?.DocDate;
        dto.DischargeTime = dischargeReq?.DocTime;
        dto.DischargeStatus = dischargeReq?.Discharge_Status;
        dto.PatientCondition = dischargeReq?.P_Condition;
        dto.Diagnosis = admission.Pro_Diagnos;
        dto.AdmType = admission.Adm_Type;
        dto.PatientType = admission.Patient_Type;
        dto.DischargeMode = dischargeReq?.Discharge_Mode switch
        {
            1 => "Relieved",
            2 => "LAMA",
            3 => "Referred",
            4 => "Absconded",
            5 => "Expired",
            9 => "Normal",
            _ => dischargeReq?.Discharge_Mode?.ToString()
        };

        // Sequential — EF Core DbContext is not thread-safe, cannot use Task.WhenAll
        dto.AccommodationCharges = await GetAccommodationAsync(admNo);

        var (services, lab, radiology) = await GetHisChargesAsync(admNo);
        dto.ServiceCharges = services;
        dto.LaboratoryCharges = lab;
        dto.RadiologyCharges = radiology;

        dto.PharmacyCharges = await GetPharmacyChargesAsync(admNo);

        var receipts = await GetReceiptsAsync(admNo);
        var refunds = await GetRefundsAsync(admNo);
        dto.Receipts = receipts;

        // Totals
        decimal totalCharges =
              dto.AccommodationCharges.Sum(l => l.NetAmount)
            + dto.ServiceCharges.Sum(l => l.NetAmount)
            + dto.LaboratoryCharges.Sum(l => l.NetAmount)
            + dto.RadiologyCharges.Sum(l => l.NetAmount)
            + dto.PharmacyCharges.Sum(l => l.NetAmount);

        decimal totalDeposits = receipts.Sum(r => r.Amount);
        decimal netAdvance = totalDeposits - refunds;

        dto.TotalCharges = totalCharges;
        dto.AmountReceived = netAdvance;
        dto.NetBilledAmount = totalCharges;
        dto.Balance = totalCharges - netAdvance;

        return dto;
    }

    // Accommodation
    // Now reads from Ipd_Room_Charges_Auto_TimeWise (the table with actual data)
    private async Task<List<ProvisionalBillLineDto>> GetAccommodationAsync(string admNo)
    {
        try
        {
            return await _hisDb.IpdRoomChargesAutoTimeWise
                .AsNoTracking()
                .Where(r => r.Adm_No == admNo
                         && r.Sib == "Y"                          // sib=Y means billable
                         && (r.IncludeInBill == null || r.IncludeInBill != "N"))
                .Select(r => new ProvisionalBillLineDto
                {
                    ChargeHead = "Accommodation",
                    Description = $"Ward {r.WardId} / Room {r.RoomId} / Bed {r.Bed}",
                    ChargeType = "Accommodation",
                    Qty = r.Qty ?? 1,
                    Rate = r.Rate ?? 0,
                    Amount = r.Amount ?? ((r.Qty ?? 1) * (r.Rate ?? 0)),
                    DisPer = r.Dp ?? 0,
                    DisAmt = r.Discount ?? 0,
                    NetAmount = r.NetAmount ?? (((r.Qty ?? 1) * (r.Rate ?? 0)) - (r.Discount ?? 0)),
                    Date = r.DocDate
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            LogSectionError("Accommodation", admNo, ex);
            return [];
        }
    }

    // Services + Lab + Radiology (all from HIS IPD_Service_Bill_Det)
    // Key fix: removed IncludeInBill filter — Vipha sets it 'N' on all Det rows
    // Key fix: lab/radiology amounts come from here, NOT from LIS Invest_Booking_Det
    private async Task<(List<ProvisionalBillLineDto> services,
                        List<ProvisionalBillLineDto> lab,
                        List<ProvisionalBillLineDto> radiology)> GetHisChargesAsync(string admNo)
    {
        try
        {
            var billNos = await _hisDb.IpdServiceBillMas
                .AsNoTracking()
                .Where(m => m.Adm_No == admNo && m.IsCancel != true)
                .Select(m => m.BillNo)
                .ToListAsync();

            var valid = billNos.Where(b => !string.IsNullOrEmpty(b)).ToList();
            if (valid.Count == 0) return ([], [], []);

            // Fetch ALL non-cancelled lines — IncludeInBill filter removed intentionally
            var rawLines = await _hisDb.IpdServiceBillDet
                .AsNoTracking()
                .Where(d => valid.Contains(d.BillNo) && d.Can_Status != "Y")
                .Select(d => new {
                    d.SerId,
                    d.Typ,
                    d.Ser_Type,
                    d.Qty,
                    d.Rate,
                    d.Amount,
                    d.DisPer,
                    d.DisAmt,
                    d.NetAmount,
                    d.Bill_Date
                })
                .ToListAsync();

            if (rawLines.Count == 0) return ([], [], []);

            // Lab (Typ=L) → LIS Test_Mas
            // Radiology (Typ=R) + others → HIS Service_Master
            var labIds = rawLines
                .Where(d => d.SerId.HasValue && d.Typ == "L")
                .Select(d => d.SerId!.Value)
                .Distinct().ToList();

            var serviceIds = rawLines
                .Where(d => d.SerId.HasValue && d.Typ != "L")
                .Select(d => d.SerId!.Value)
                .Distinct().ToList();

            // Lab names from LIS Test_Mas
            var testNames = labIds.Count > 0
                ? await _lisDb.TestMas
                    .Where(t => labIds.Contains(t.Id))
                    .Select(t => new { t.Id, t.Test_Name })
                    .ToDictionaryAsync(t => t.Id, t => t.Test_Name)
                : new Dictionary<int, string?>();

            // Radiology + Service names from HIS Service_Master
            // Cast SerId to long to match Service_Master.Id type
            var serviceIdsAsLong = serviceIds.Select(id => (long)id).ToList();
            var serviceNames = serviceIdsAsLong.Count > 0
                ? await _hisDb.ServiceMasters
                    .Where(s => serviceIdsAsLong.Contains(s.Id))
                    .Select(s => new { s.Id, s.Ser_Name })
                    .ToDictionaryAsync(s => (int)s.Id, s => s.Ser_Name)
                : new Dictionary<int, string?>();

            var services = new List<ProvisionalBillLineDto>();
            var lab = new List<ProvisionalBillLineDto>();
            var radiology = new List<ProvisionalBillLineDto>();

            foreach (var d in rawLines)
            {
                string? name = null;

                if (d.SerId.HasValue)
                {
                    if (d.Typ == "L")
                        testNames.TryGetValue(d.SerId.Value, out name);
                    else
                        serviceNames.TryGetValue(d.SerId.Value, out name);
                }

                name ??= $"Service ID: {d.SerId}";

                var line = new ProvisionalBillLineDto
                {
                    Description = name,
                    ChargeType = d.Typ,
                    Qty = d.Qty ?? 1,
                    Rate = d.Rate ?? 0,
                    Amount = d.Amount ?? 0,
                    DisPer = d.DisPer ?? 0,
                    DisAmt = d.DisAmt ?? 0,
                    NetAmount = d.NetAmount ?? 0,
                    Date = d.Bill_Date
                };

                switch (d.Typ?.ToUpperInvariant())
                {
                    case "L":
                        line.ChargeHead = "Laboratory";
                        lab.Add(line);
                        break;
                    case "R":
                        line.ChargeHead = "Radiology";
                        radiology.Add(line);
                        break;
                    default:
                        line.ChargeHead = d.Ser_Type ?? "Service";
                        services.Add(line);
                        break;
                }
            }

            return (services, lab, radiology);
        }
        catch (Exception ex)
        {
            LogSectionError("HisCharges", admNo, ex);
            return ([], [], []);
        }
    }

    // Pharmacy
    private async Task<List<ProvisionalBillLineDto>> GetPharmacyChargesAsync(string admNo)
    {
        try
        {
            var sales = await _viphaDb.RetailMas
                .AsNoTracking()
                .Where(r => r.IPDNo == admNo)
                .Select(r => new ProvisionalBillLineDto
                {
                    ChargeHead = "Pharmacy",
                    Description = "Invoice: " + r.Retail_No,
                    ChargeType = "Pharmacy",
                    Qty = 1,
                    Rate = r.RetailNetAmount ?? 0,
                    Amount = r.RetailNetAmount ?? 0,
                    DisAmt = r.Discount ?? 0,
                    NetAmount = r.RetailNetAmount ?? 0,
                    Date = r.Retail_Date
                })
                .ToListAsync();

            var returns = await _viphaDb.RetailReturnMas
                .AsNoTracking()
                .Where(r => r.IPDNo == admNo && r.T_Status != "C")
                .Select(r => new ProvisionalBillLineDto
                {
                    ChargeHead = "Pharmacy Return",
                    Description = "Return: " + r.Return_No,
                    ChargeType = "PharmacyReturn",
                    Qty = 1,
                    Rate = -(r.ReturnNetAmount ?? 0),
                    Amount = -(r.ReturnNetAmount ?? 0),
                    NetAmount = -(r.ReturnNetAmount ?? 0),
                    Date = r.Return_Date
                })
                .ToListAsync();

            return [.. sales, .. returns];
        }
        catch (Exception ex)
        {
            LogSectionError("Pharmacy", admNo, ex);
            return [];
        }
    }

    // Receipts
    private async Task<List<ReceiptDetailDto>> GetReceiptsAsync(string admNo)
    {
        try
        {
            return await _hisDb.ReceiptMas
                .AsNoTracking()
                .Where(r => r.Adm_No == admNo && r.T_Status != "C")
                .Where(r => r.Adm_No == admNo && r.T_Status != "C")
                .OrderBy(r => r.DocDate)
                .Select(r => new ReceiptDetailDto
                {
                    ReceiptNo = r.DocNo,
                    ReceiptDate = r.DocDate,
                    Amount = r.Amount ?? 0,
                    Type = r.DocType ?? "IPD"
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            LogSectionError("Receipts", admNo, ex);
            return [];
        }
    }

    // Refunds
    private async Task<decimal> GetRefundsAsync(string admNo)
    {
        try
        {
            return await _hisDb.PaymentMas
                .AsNoTracking()
                .Where(p => p.Adm_No == admNo && p.T_Status != "C")
                .Select(p => p.Amount ?? 0)
                .SumAsync();
        }
        catch (Exception ex)
        {
            LogSectionError("Refunds", admNo, ex);
            return 0;
        }
    }

    private static void LogSectionError(string section, string admNo, Exception ex) =>
        Console.Error.WriteLine(
            $"[ProvisionalBill/{section}] admNo={admNo} | {ex.GetType().Name}: {ex.Message} | Inner: {ex.InnerException?.Message} | {ex.InnerException?.InnerException?.Message}");
}