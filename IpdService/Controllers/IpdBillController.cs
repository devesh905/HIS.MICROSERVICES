using IpdService.Data;
using IpdService.Models.DTOs;
using IpdService.Models.Entities.ViphaHms;
using IpdService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IpdService.Controllers;

[ApiController]
[Route("api/ipd-bills")]
[Authorize]
public class IpdBillController : ControllerBase
{
    private readonly IHospitalQueryService _hospitals;

    public IpdBillController(IHospitalQueryService hospitals) => _hospitals = hospitals;

    // GET api/ipd-bills/by-adm/{admNo}?hospitalKey=MRT
    // All bills for one admission
    [HttpGet("by-adm/{admNo}")]
    public async Task<IActionResult> GetByAdmission(string admNo, [FromQuery] string hospitalKey)
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

        var bills = await ctx.IpdBillMas
            .Where(b => b.Adm_No == admNo)
            .OrderByDescending(b => b.Bill_Date)
            .ToListAsync();

        if (!bills.Any())
            return NotFound(new { message = "No bills found for this admission." });

        var result = bills.Select(b => MapToSummary(b, hospitalKey, entry.Name)).ToList();
        return Ok(result);
    }

    // GET api/ipd-bills/by-uhids?uhids=X,Y&hospitalKey=MRT
    // All bills for a patient (all their UHIDs → all their admissions → all bills)
    [HttpGet("by-uhids")]
    public async Task<IActionResult> GetByUhids(
        [FromQuery] string uhids,
        [FromQuery] string hospitalKey)
    {
        if (string.IsNullOrWhiteSpace(uhids))
            return BadRequest(new { message = "No UHIDs provided." });

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        var uhidList = uhids.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

        await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

        // Resolve admission numbers for these UHIDs first
        var admNos = await ctx.IpdRegistrations
            .Where(i => uhidList.Contains(i.UhidNo))
            .Select(i => i.Adm_No)
            .Distinct()
            .ToListAsync();

        if (!admNos.Any())
            return NotFound(new { message = "No admissions found for these UHIDs." });

        var bills = await ctx.IpdBillMas
            .Where(b => admNos.Contains(b.Adm_No))
            .OrderByDescending(b => b.Bill_Date)
            .ToListAsync();

        if (!bills.Any())
            return NotFound(new { message = "No IPD bills found." });

        var result = bills.Select(b => MapToSummary(b, hospitalKey, entry.Name)).ToList();
        return Ok(result);
    }

    // GET api/ipd-bills/detail/{billNo}?hospitalKey=MRT
    // Full bill with line items
    [HttpGet("detail")]
    public async Task<IActionResult> GetDetail([FromQuery] string billNo, [FromQuery] string hospitalKey)
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

        var bill = await ctx.IpdBillMas
             .AsNoTracking()
            .FirstOrDefaultAsync(b => b.BillNo == billNo);

        if (bill == null)
            return NotFound(new { message = "Bill not found." });

        var lines = await ctx.IpdBillDet
            .AsNoTracking()
            .Where(d => d.BillNo == billNo)
            .OrderBy(d => d.Ser_Type)
            .ThenBy(d => d.Ser_name)
            .ToListAsync();

        var dto = MapToDetail(bill, lines, hospitalKey, entry.Name);
        return Ok(dto);
    }

    // GET api/ipd-bills/active  (JWT-protected)
    // Bills for the currently-logged-in patient's active/recent admissions
    [HttpGet("active")]
    [Authorize]
    public async Task<IActionResult> GetActive()
    {
        var mobile = User.FindFirst("mobile")?.Value;
        var hospitalKey = User.FindFirst("hospitalKey")?.Value;

        if (mobile == null || hospitalKey == null)
            return Unauthorized();

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return Unauthorized();

        await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

        var uhidList = await ctx.PatientRegistrations
            .Where(p => p.MobileNo == mobile)
            .Select(p => p.UhidNo)
            .Distinct()
            .ToListAsync();

        if (!uhidList.Any())
            return Ok(new { bills = Array.Empty<object>() });

        var admNos = await ctx.IpdRegistrations
            .Where(i => uhidList.Contains(i.UhidNo))
            .Select(i => i.Adm_No)
            .Distinct()
            .ToListAsync();

        if (!admNos.Any())
            return Ok(new { bills = Array.Empty<object>() });

        var bills = await ctx.IpdBillMas
            .Where(b => admNos.Contains(b.Adm_No))
            .OrderByDescending(b => b.Bill_Date)
            .ToListAsync();

        var result = bills.Select(b => MapToSummary(b, hospitalKey, entry.Name)).ToList();
        return Ok(new { bills = result });
    }

    // Mappers

    private static IpdBillSummaryDto MapToSummary(
        IpdBillMas b,
        string hospitalKey,
        string hospitalName) => new()
        {
            Adm_No = b.Adm_No,
            BillNo = b.BillNo,
            PatientName = b.PatientName,
            Bill_Date = b.Bill_Date,
            BillTime = b.BillTime,
            NetAmount = b.NetAmount,
            DisAmt = b.DisAmt,
            ReceiptAmount = b.ReceiptAmount,
            TotalNetAmount = b.TotalNetAmount,
            GstAmount = b.GstAmount,
            UnderPackageAmt = b.UnderPackageAmt,
            T_status = b.T_status,
            Adm_Date = b.Adm_Date,
            DisDate = b.DisDate,
            HospitalKey = hospitalKey,
            HospitalName = hospitalName
        };

    private static IpdBillDetailDto MapToDetail(
        IpdBillMas b,
        List<IpdBillDet> lines,
        string hospitalKey,
        string hospitalName)
    {
        var dto = new IpdBillDetailDto
        {
            // Summary fields
            Adm_No = b.Adm_No,
            BillNo = b.BillNo,
            PatientName = b.PatientName,
            Bill_Date = b.Bill_Date,
            BillTime = b.BillTime,
            NetAmount = b.NetAmount,
            DisAmt = b.DisAmt,
            ReceiptAmount = b.ReceiptAmount,
            TotalNetAmount = b.TotalNetAmount,
            GstAmount = b.GstAmount,
            UnderPackageAmt = b.UnderPackageAmt,
            T_status = b.T_status,
            Adm_Date = b.Adm_Date,
            DisDate = b.DisDate,
            HospitalKey = hospitalKey,
            HospitalName = hospitalName,

            // Detail-only
            Remarks = b.Remarks,
            Remarks1 = b.Remarks1,
            Can_Remarks = b.Can_Remarks,
            CancelAt = b.CancelAt,
            IsImplantService = b.IsImplantService,
            ReOpenForReturn = b.ReOpenForReturn,
            AccountPosting = b.AccountPosting,
            UPAmt = b.UPAmt,

            LineItems = lines.Select(l => new IpdBillLineDto
            {
                Ser_Type = l.Ser_Type,
                Ser_name = l.Ser_name,
                Qty = l.Qty,
                Rate = l.Rate,
                Amount = l.Amount,
                DisPer = l.DisPer,
                DisAmt = l.DisAmt,
                NetAmount = l.NetAmount,
                GstAmt = l.GstAmt,
                UPAmt = l.UPAmt
            }).ToList()
        };

        return dto;
    }
}