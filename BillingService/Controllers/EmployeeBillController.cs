using BillingService.Data;
using BillingService.Models.DTOs;
using BillingService.Models.Entities.ViphaHms;
using BillingService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BillingService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "employee")]
public class EmployeeBillController : ControllerBase
{
    private readonly IHospitalQueryService _hospitals;

    public EmployeeBillController(IHospitalQueryService hospitals)
    {
        _hospitals = hospitals;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string hospitalKey,
        [FromQuery] string? uhid,
        [FromQuery] string? billNo,
        [FromQuery] string? mobile)
    {
        if (string.IsNullOrWhiteSpace(hospitalKey))
            return BadRequest(new { message = "hospitalKey is required." });

        // At least one search param must be provided
        if (string.IsNullOrWhiteSpace(uhid) &&
            string.IsNullOrWhiteSpace(billNo) &&
            string.IsNullOrWhiteSpace(mobile))
            return BadRequest(new { message = "Provide at least one of: uhid, billNo, mobile." });

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        await using var db = _hospitals.CreateHmsContext(entry.HisCs);
        await using var lis = _hospitals.CreateLisContext(entry.LisCs);

        // 1. Resolve UHID(s) from search param
        List<string> uhidList;

        if (!string.IsNullOrWhiteSpace(billNo))
        {
            // Search by exact bill number → get its UHID, then pull all bills for that patient
            var billNoTrim = billNo.Trim();
            var masFromBill = await db.OpdBillingMas
                .Where(b => b.BillNo == billNoTrim)
                .FirstOrDefaultAsync();

            if (masFromBill == null)
                return NotFound(new { message = $"No bill found with Bill No: {billNoTrim}" });

            uhidList = new List<string> { masFromBill.UhidNo! };
        }
        else if (!string.IsNullOrWhiteSpace(mobile))
        {
            // Search by mobile → find all UHIDs linked to this mobile
            var mobileTrim = mobile.Trim().TrimStart('+').Replace(" ", "");
            // Normalise: strip leading country code if present
            if (mobileTrim.StartsWith("91") && mobileTrim.Length == 12)
                mobileTrim = mobileTrim[2..];

            var matchedUhids = await db.PatientRegistrations
                .Where(r => r.MobileNo != null && r.MobileNo.EndsWith(mobileTrim))
                .Select(r => r.UhidNo ?? r.OpNo)   // adjust to your PK field
                .Distinct()
                .ToListAsync();

            if (!matchedUhids.Any())
                return NotFound(new { message = $"No patients found with mobile: {mobile}" });

            uhidList = matchedUhids.Where(u => u != null).Cast<string>().ToList();
        }
        else
        {
            // Direct UHID search
            uhidList = new List<string> { uhid!.Trim() };
        }

        // 2. Pull billing masters
        var masters = await db.OpdBillingMas
            .Where(b => uhidList.Contains(b.UhidNo!))
            .OrderByDescending(b => b.Bill_Date)
            .ToListAsync();

        if (!masters.Any())
            return NotFound(new { message = "No bills found for the given criteria." });

        var billNos = masters.Select(m => m.BillNo).Distinct().ToList();
        var details = await db.OpdBillingDet
            .Where(d => uhidList.Contains(d.UhidNo!) && billNos.Contains(d.BillNo))
            .ToListAsync();

        // 3. Name lookups (same helpers as BillController)
        var (serviceDict, testDict) = await BuildNameDicts(db, lis, details);
        var (patientDict, doctorDict, sponsDict) = await BuildLookupDicts(db, masters);

        return Ok(BuildResult(masters, details, serviceDict, testDict,
                              patientDict, doctorDict, sponsDict));
    }


    [HttpGet("ipd-search")]
    public async Task<IActionResult> IpdSearch(
        [FromQuery] string hospitalKey,
        [FromQuery] string? uhid,
        [FromQuery] string? admNo,
        [FromQuery] string? mobile)
    {
        if (string.IsNullOrWhiteSpace(hospitalKey))
            return BadRequest(new { message = "hospitalKey is required." });

        if (string.IsNullOrWhiteSpace(uhid) &&
            string.IsNullOrWhiteSpace(admNo) &&
            string.IsNullOrWhiteSpace(mobile))
            return BadRequest(new { message = "Provide at least one of: uhid, admNo, mobile." });

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        await using var db = _hospitals.CreateHmsContext(entry.HisCs);

        // Resolve admission number(s) from the search param
        List<string> admNos;

        if (!string.IsNullOrWhiteSpace(admNo))
        {
            // Direct admission number search
            admNos = new List<string> { admNo.Trim() };
        }
        else if (!string.IsNullOrWhiteSpace(mobile))
        {
            // mobile - UHIDs - admNos
            var mobileTrim = mobile.Trim().TrimStart('+').Replace(" ", "");
            if (mobileTrim.StartsWith("91") && mobileTrim.Length == 12)
                mobileTrim = mobileTrim[2..];

            var uhidList = await db.PatientRegistrations
                .Where(r => r.MobileNo != null && r.MobileNo.EndsWith(mobileTrim))
                .Select(r => r.UhidNo)
                .Where(u => u != null)
                .Distinct()
                .ToListAsync();

            if (!uhidList.Any())
                return NotFound(new { message = $"No patients found with mobile: {mobile}" });

            admNos = await db.IpdRegistrations
                .Where(i => uhidList.Contains(i.UhidNo))
                .Select(i => i.Adm_No)
                .Distinct()
                .ToListAsync();
        }
        else
        {
            // UHID → admNos
            var uhidTrim = uhid!.Trim();
            admNos = await db.IpdRegistrations
                .Where(i => i.UhidNo == uhidTrim)
                .Select(i => i.Adm_No)
                .Distinct()
                .ToListAsync();
        }

        if (!admNos.Any())
            return NotFound(new { message = "No IPD admissions found for the given criteria." });

        // Pull all IPD bills for these admissions
        var bills = await db.IpdBillMas
            .Where(b => admNos.Contains(b.Adm_No))
            .OrderByDescending(b => b.Bill_Date)
            .ToListAsync();

        if (!bills.Any())
            return NotFound(new { message = "No IPD bills found." });

        // Pull line items for all those bills
        var billNos = bills.Select(b => b.BillNo).Distinct().ToList();
        var lines = await db.IpdBillDet
            .Where(d => billNos.Contains(d.BillNo))
            .OrderBy(d => d.Ser_Type)
            .ThenBy(d => d.Ser_name)
            .ToListAsync();

        var linesByBill = lines
            .GroupBy(l => l.BillNo)
            .ToDictionary(g => g.Key ?? "", g => g.ToList());

        // Build result
        var result = bills.Select(b => new
        {
            b.Adm_No,
            b.BillNo,
            b.PatientName,
            b.Bill_Date,
            b.BillTime,
            b.NetAmount,
            b.DisAmt,
            b.ReceiptAmount,
            b.TotalNetAmount,
            b.GstAmount,
            b.UnderPackageAmt,
            b.T_status,
            b.Adm_Date,
            b.DisDate,
            b.Remarks,
            b.Remarks1,
            b.Can_Remarks,
            b.CancelAt,
            b.IsImplantService,
            b.ReOpenForReturn,
            b.AccountPosting,
            b.UPAmt,
            HospitalKey = hospitalKey,
            HospitalName = entry.Name,

            LineItems = linesByBill.TryGetValue(b.BillNo ?? "", out var items)
    ? items.Select(l => (object)new
    {
        l.Ser_Type,
        l.Ser_name,
        l.Qty,
        l.Rate,
        l.Amount,
        l.DisPer,
        l.DisAmt,
        l.NetAmount,
        l.GstAmt,
        l.UPAmt
    }).ToList()
    : new List<object>()
        }).ToList();

        return Ok(result);
    }

    private sealed record PatientInfo(
        string? OpNo, string? PatientName,
        string? MobileNo, short? AgeInYears, string? Gender);

    private static async Task<(Dictionary<string, PatientInfo>, Dictionary<int, string?>, Dictionary<int, string?>)>
        BuildLookupDicts(HmsDbContext db, List<OpdBillingMas> masters)
    {
        var opNos = masters.Select(m => m.OpNo).Distinct().ToList();
        var docIds = masters.Where(m => m.DocId.HasValue).Select(m => m.DocId!.Value).Distinct().ToList();
        var sponsIds = masters.Where(m => m.SponsId.HasValue).Select(m => m.SponsId!.Value).Distinct().ToList();

        var patientDict = await db.PatientRegistrations
            .Where(r => opNos.Contains(r.OpNo))
            .Select(r => new PatientInfo(r.OpNo, r.PatientName, r.MobileNo, r.AgeInYears, r.Gender))
            .ToDictionaryAsync(r => r.OpNo ?? "", r => r);

        var doctorDict = docIds.Any()
            ? await db.DoctorMasters.Where(d => docIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.Doc_Name)
            : new Dictionary<int, string?>();

        var sponsDict = sponsIds.Any()
            ? await db.SponsorMasters.Where(s => sponsIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Sponsor_Name)
            : new Dictionary<int, string?>();

        return (patientDict, doctorDict, sponsDict);
    }

    private static async Task<(Dictionary<int, string?>, Dictionary<int, string?>)>
        BuildNameDicts(HmsDbContext db, LisDbContext lis, List<OpdBillingDet> details)
    {
        var labIds = details.Where(d => d.Typ == "L" && d.SerId.HasValue)
                            .Select(d => d.SerId!.Value).Distinct().ToList();
        var hisIds = details.Where(d => d.Typ != "L" && d.SerId.HasValue)
                            .Select(d => d.SerId!.Value).Distinct().ToList();

        var testDict = labIds.Any()
            ? await lis.TestMas.Where(t => labIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Test_Name)
            : new Dictionary<int, string?>();

        var serviceDict = hisIds.Any()
            ? await db.ServiceMasters.Where(s => hisIds.Contains((int)s.Id))
                .ToDictionaryAsync(s => (int)s.Id, s => s.Ser_Name)
            : new Dictionary<int, string?>();

        return (serviceDict, testDict);
    }

    private static List<BillDto> BuildResult(
        List<OpdBillingMas> masters, List<OpdBillingDet> details,
        Dictionary<int, string?> serviceDict, Dictionary<int, string?> testDict,
        Dictionary<string, PatientInfo> patientDict,
        Dictionary<int, string?> doctorDict, Dictionary<int, string?> sponsDict)
    {
        var detByBill = details.GroupBy(d => d.BillNo)
                               .ToDictionary(g => g.Key ?? "", g => g.ToList());

        return masters.Select(m =>
        {
            patientDict.TryGetValue(m.OpNo ?? "", out var pat);
            return new BillDto
            {
                Id = m.Id,
                BillNo = m.BillNo,
                Bill_Date = m.Bill_Date,
                Timein = m.Timein,
                UhidNo = m.UhidNo,
                OpNo = m.OpNo1,
                VisitNo = m.VisitNo,
                PayMode = m.PayMode,
                PayMode2 = m.PayMode2,
                TotalAmt = m.TotalAmt,
                NetAmount = m.NetAmount,
                Receipt_Amount = m.Receipt_Amount,
                Balance = m.Balance,
                GSTAmount = m.GSTAmount,
                Round_off = m.Round_off,
                IsCancel = m.IsCancel,
                CancelRemarks = m.CancelRemarks,
                Remarks = m.Remarks,
                PatientName = pat?.PatientName,
                AgeSex = pat != null ? $"{pat.AgeInYears} Y / {pat.Gender}" : null,
                Mobile = pat?.MobileNo,
                ConsultantName = m.DocId.HasValue && doctorDict.TryGetValue(m.DocId.Value, out var doc) ? doc : null,
                SponsorName = m.SponsId.HasValue && sponsDict.TryGetValue(m.SponsId.Value, out var sp) ? sp : null,
                Items = detByBill.TryGetValue(m.BillNo ?? "", out var items)
                    ? items.OrderBy(i => i.Sno).Select(i => new BillDetailItemDto
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
                    : new List<BillDetailItemDto>()
            };
        }).ToList();

        string ResolveName(OpdBillingDet i)
        {
            if (!i.SerId.HasValue) return "Unknown";
            if (i.Typ == "L")
                return testDict.TryGetValue(i.SerId.Value, out var tn) && tn != null ? tn : $"Lab Test {i.SerId}";
            return serviceDict.TryGetValue(i.SerId.Value, out var sn) && sn != null ? sn : $"Service {i.SerId}";
        }
    }
}