using BillingService.Data;
using BillingService.Models.DTOs;
using BillingService.Models.Entities.ViphaHms;
using BillingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillController : ControllerBase
{
    private readonly IHospitalQueryService _hospitals;

    public BillController(IHospitalQueryService hospitals)  
    {
        _hospitals = hospitals;
    }

    private sealed record PatientInfo(
        string? OpNo, string? PatientName, string? MobileNo,
        short? AgeInYears, string? Gender);

    private sealed record BillRow(
        long? Id, string? BillNo, DateTime? Bill_Date, string? Timein,
        string? UhidNo, string? OpNo, int? VisitNo,
        string? PayMode, string? PayMode2,
        decimal? TotalAmt, decimal? NetAmount, decimal? Receipt_Amount,
        decimal? Balance, decimal? GSTAmount, decimal? Round_off,
        bool? IsCancel, string? CancelRemarks, string? Remarks,
        int? DocId, int? SponsId, string Source);

    private sealed record DetailRow(
        string? BillNo, int? Sno, string? Typ, int? SerId,
        decimal? Qty, decimal? Rate, decimal? Amount,
        decimal? DisPer, decimal? DisAmt, decimal? NetAmount,
        decimal? GstPer, decimal? GstAmt, string? Can_Status);

    [HttpGet("by-uhid/{uhid}")]
    public async Task<IActionResult> GetByUhid(string uhid, [FromQuery] string hospitalKey)
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        await using var db = _hospitals.CreateHmsContext(entry.HisCs);
        await using var lis = _hospitals.CreateLisContext(entry.LisCs);

        var (bills, details) = await FetchAllBills(db, uhid: uhid);

        if (!bills.Any())
            return NotFound(new { message = "No bills found for this UHID." });

        return Ok(await BuildResult(db, lis, bills, details));
    }

    [HttpGet("by-uhids")]
    public async Task<IActionResult> GetByUhids(
        [FromQuery] string uhids, [FromQuery] string hospitalKey)
    {
        if (string.IsNullOrWhiteSpace(uhids))
            return BadRequest(new { message = "No UHIDs provided." });

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        var uhidList = uhids.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

        await using var db = _hospitals.CreateHmsContext(entry.HisCs);
        await using var lis = _hospitals.CreateLisContext(entry.LisCs);

        var (bills, details) = await FetchAllBills(db, uhidList: uhidList);

        if (!bills.Any())
            return NotFound(new { message = "No bills found." });

        return Ok(await BuildResult(db, lis, bills, details));
    }

    // Fetch OPD + IPD bills 
    private static async Task<(List<BillRow> bills, List<DetailRow> details)>
        FetchAllBills(HmsDbContext db, string? uhid = null, List<string>? uhidList = null)
    {
        var opdQ = db.OpdBillingMas.AsQueryable();
        opdQ = uhid != null ? opdQ.Where(b => b.UhidNo == uhid)
                            : opdQ.Where(b => uhidList!.Contains(b.UhidNo));
        var opdMas = await opdQ.OrderByDescending(b => b.Bill_Date).ToListAsync();

        var ipdQ = db.IpdServiceBillMas.AsQueryable();
        ipdQ = uhid != null ? ipdQ.Where(b => b.UhidNo == uhid)
                            : ipdQ.Where(b => uhidList!.Contains(b.UhidNo));
        var ipdMas = await ipdQ.OrderByDescending(b => b.Bill_Date).ToListAsync();

        var opdBillNos = opdMas.Select(m => m.BillNo).Distinct().ToList();
        List<OpdBillingDet> opdDet = opdBillNos.Any()
            ? await db.OpdBillingDet
                .Where(d => opdBillNos.Contains(d.BillNo) &&
                            (uhid == null || d.UhidNo == uhid) &&
                            (uhidList == null || uhidList.Contains(d.UhidNo!)))
                .ToListAsync()
            : new();

        var ipdBillNos = ipdMas.Select(m => m.BillNo).Distinct().ToList();
        List<IpdServiceBillDet> ipdDet = ipdBillNos.Any()
            ? await db.IpdServiceBillDet
                .Where(d => ipdBillNos.Contains(d.BillNo))
                .ToListAsync()
            : new();

        // Map to unified rows
        var bills = opdMas.Select(m => new BillRow(
                m.Id, m.BillNo, m.Bill_Date, m.Timein,
                m.UhidNo, m.OpNo, m.VisitNo,
                m.PayMode, m.PayMode2,
                m.TotalAmt, m.NetAmount, m.Receipt_Amount,
                m.Balance, m.GSTAmount, m.Round_off,
                m.IsCancel, m.CancelRemarks, m.Remarks,
                m.DocId, m.SponsId, "OPD"))
            .Concat(ipdMas.Select(m => new BillRow(
                m.Id, m.BillNo, m.Bill_Date, m.Timein,
                m.UhidNo, null, null,         
                m.Paymode, m.Paymode2,
                m.TotalAmt, m.NetAmount, m.Receipt_Amount,
                m.Balance, m.GSTAmount, m.Round_off,
                m.IsCancel, m.CancelRemarks, m.Remarks,
                m.DocId, m.SponsId, "IPD")))
            .OrderByDescending(b => b.Bill_Date)
            .ToList();

        var details = opdDet.Select(d => new DetailRow(
                d.BillNo, d.Sno, d.Typ, d.SerId,
                d.Qty, d.Rate, d.Amount,
                d.DisPer, d.DisAmt, d.NetAmount,
                d.GstPer, d.GstAmt, d.Can_Status))
            .Concat(ipdDet.Select(d => new DetailRow(
    d.BillNo, d.Sno, d.Typ, d.SerId,
    d.Qty,
    d.SerRate ?? d.Rate,                    // SerRate has actual rate
    d.SerAmount ?? d.Amount,                // SerAmount has actual amount  
    d.DisPer, d.DisAmt,
    d.NetAmount != 0 ? d.NetAmount          // Row 5 (Pc type) has NetAmount
        : d.SerAmount ?? d.NetAmount,       // Service rows: use SerAmount as net
    d.GstPer, d.GstAmt, d.Can_Status)))
            .ToList();

        return (bills, details);
    }

    private static async Task<List<BillDto>> BuildResult(
        HmsDbContext db, LisDbContext lis,
        List<BillRow> bills, List<DetailRow> details)
    {
        // Name dicts
        var labSerIds = details.Where(d => d.Typ == "L" && d.SerId.HasValue)
                               .Select(d => d.SerId!.Value).Distinct().ToList();
        var hisSerIds = details.Where(d => d.Typ != "L" && d.SerId.HasValue)
                               .Select(d => d.SerId!.Value).Distinct().ToList();

        var testDict = labSerIds.Any()
            ? await lis.TestMas
                .Where(t => labSerIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Test_Name)
            : new Dictionary<int, string?>();

        var serviceDict = hisSerIds.Any()
            ? await db.ServiceMasters
                .Where(s => hisSerIds.Contains((int)s.Id))
                .ToDictionaryAsync(s => (int)s.Id, s => s.Ser_Name)
            : new Dictionary<int, string?>();

        // Patient / doctor / sponsor dicts
        var opNos = bills.Where(b => b.OpNo != null).Select(b => b.OpNo).Distinct().ToList();
        var ipdUhids = bills.Where(b => b.OpNo == null && b.UhidNo != null)
                    .Select(b => b.UhidNo).Distinct().ToList();
        var docIds = bills.Where(b => b.DocId.HasValue).Select(b => b.DocId!.Value).Distinct().ToList();
        var spIds = bills.Where(b => b.SponsId.HasValue).Select(b => b.SponsId!.Value).Distinct().ToList();

        var patientDict = opNos.Any()
      ? await db.PatientRegistrations
          .Where(r => opNos.Contains(r.OpNo))
          .ToDictionaryAsync(r => r.OpNo ?? "",
              r => new PatientInfo(r.OpNo, r.PatientName, r.MobileNo, r.AgeInYears, r.Gender))
      : new Dictionary<string, PatientInfo>();

        // Build UHID→PatientInfo dict for IPD bills
        // REPLACE the patientByUhid block with this:
        var patientByUhid = ipdUhids.Any()
            ? (await db.PatientRegistrations
                .Where(r => ipdUhids.Contains(r.UhidNo))
                .OrderByDescending(r => r.Id)
                .ToListAsync())                          // fetch to memory first
                .GroupBy(r => r.UhidNo ?? "")
                .ToDictionary(
                    g => g.Key,
                    g => g.First())                      // now GroupBy runs in C#
                .ToDictionary(
                    kv => kv.Key,
                    kv => new PatientInfo(
                        kv.Value.OpNo, kv.Value.PatientName,
                        kv.Value.MobileNo, kv.Value.AgeInYears, kv.Value.Gender))
            : new Dictionary<string, PatientInfo>();

        var doctorDict = docIds.Any()
            ? await db.DoctorMasters
                .Where(d => docIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.Doc_Name)
            : new Dictionary<int, string?>();

        var sponsDict = spIds.Any()
            ? await db.SponsorMasters
                .Where(s => spIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Sponsor_Name)
            : new Dictionary<int, string?>();

        var detailsByBill = details
            .GroupBy(d => d.BillNo ?? "")
            .ToDictionary(g => g.Key, g => g.ToList());

        return bills.Select(b =>
        {
            var pat = b.OpNo != null && patientDict.TryGetValue(b.OpNo, out var p1) ? p1
        : b.UhidNo != null && patientByUhid.TryGetValue(b.UhidNo, out var p2) ? p2
        : null;
            return new BillDto
            {
                Id = b.Id,
                BillNo = b.BillNo,
                Bill_Date = b.Bill_Date,
                Timein = b.Timein,
                UhidNo = b.UhidNo,
                OpNo = b.OpNo,
                VisitNo = b.VisitNo,
                PayMode = b.PayMode,
                PayMode2 = b.PayMode2,
                TotalAmt = b.TotalAmt,
                NetAmount = b.NetAmount,
                Receipt_Amount = b.Receipt_Amount,
                Balance = b.Balance,
                GSTAmount = b.GSTAmount,
                Round_off = b.Round_off,
                IsCancel = b.IsCancel,
                CancelRemarks = b.CancelRemarks,
                Remarks = b.Remarks,
                BillSource = b.Source,           
                PatientName = pat?.PatientName,
                AgeSex = pat != null ? $"{pat.AgeInYears} Y / {pat.Gender}" : null,
                Mobile = pat?.MobileNo,
                ConsultantName = b.DocId.HasValue && doctorDict.TryGetValue(b.DocId.Value, out var doc) ? doc : null,
                SponsorName = b.SponsId.HasValue && sponsDict.TryGetValue(b.SponsId.Value, out var sp) ? sp : null,
                Items = detailsByBill.TryGetValue(b.BillNo ?? "", out var items)
                    ? items.OrderBy(i => i.Sno).Select(i => new BillDetailItemDto
                    {
                        Sno = i.Sno,
                        Typ = i.Typ,
                        SerName = ResolveName(i, serviceDict, testDict),
                        Qty = (int?)i.Qty,
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
    }

    private static string ResolveName(
        DetailRow i,
        Dictionary<int, string?> serviceDict,
        Dictionary<int, string?> testDict)
    {
        if (!i.SerId.HasValue) return "Unknown";
        if (i.Typ == "L")
            return testDict.TryGetValue(i.SerId.Value, out var tn) && tn != null ? tn : $"Lab Test {i.SerId}";
        return serviceDict.TryGetValue(i.SerId.Value, out var sn) && sn != null ? sn : $"Service {i.SerId}";
    }
}