using IpdService.Models.DTOs;
using IpdService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace IpdService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProvisionalBillController : ControllerBase
{
    private readonly IHospitalQueryService _hospitals;
    private readonly ILogger<ProvisionalBillController> _logger;

    public ProvisionalBillController(
        IHospitalQueryService hospitals,
        ILogger<ProvisionalBillController> logger)
    {
        _hospitals = hospitals;
        _logger = logger;
    }

    // GET api/ProvisionalBill/IP-240716081?hospitalKey=MRT
    [HttpGet("{admNo}")]
    public async Task<IActionResult> GetProvisionalBill(
        string admNo,
        [FromQuery] string hospitalKey)
    {
        if (string.IsNullOrWhiteSpace(hospitalKey))
            return BadRequest(new { message = "hospitalKey is required." });

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        try
        {
            await using var hisDb = _hospitals.CreateHisContext(entry.HisCs);
            await using var lisDb = _hospitals.CreateLisContext(entry.LisCs);
            await using var viphaDb = _hospitals.CreateViphaContext(entry.ViphaCs);

            var svc = new ProvisionalBillService(hisDb, lisDb, viphaDb);
            var bill = await svc.GetProvisionalBillAsync(admNo);

            if (bill == null)
                return NotFound(new { message = $"No active admission found for: {admNo}" });

            return Ok(bill);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error generating provisional bill for {AdmNo}: {ExType} | {ExMsg} | {Inner}",
                admNo, ex.GetType().Name, ex.Message, ex.InnerException?.Message);

            return StatusCode(500, new { message = "Failed to generate provisional bill." });
        }
    }

    // Debug endpoints

    [HttpGet("debug-his/{admNo}")]
    public async Task<IActionResult> DebugHis(string admNo, [FromQuery] string hospitalKey)
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        await using var hisDb = _hospitals.CreateHisContext(entry!.HisCs);
        var results = new Dictionary<string, string>();

        await TrySection(results, "IpdServiceBillMas",
            () => hisDb.IpdServiceBillMas.AsNoTracking()
                        .Where(r => r.Adm_No == admNo)
                        .Select(r => new { r.Adm_No, r.BillNo, r.IsCancel })
                        .ToListAsync());

        await TrySection(results, "IpdServiceBillDet",
            () => hisDb.IpdServiceBillDet.AsNoTracking()
                        .Select(r => new { r.BillNo, r.SerId, r.NetAmount })
                        .Take(5).ToListAsync());

        return Ok(results);
    }

    // Helpers
    private static async Task TrySection<T>(
        Dictionary<string, string> results,
        string key,
        Func<Task<List<T>>> query)
    {
        try
        {
            var rows = await query();
            results[key] = $"OK ({rows.Count} rows)";
        }
        catch (Exception ex)
        {
            results[key] = $"FAIL: {ex.GetType().Name} | {ex.Message}";
        }
    }
}