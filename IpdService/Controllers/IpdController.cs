using IpdService.Data;
using IpdService.Models.DTOs;
using IpdService.Models.Entities.ViphaHms;
using IpdService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IpdService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IpdController : ControllerBase
{
    private readonly IHospitalQueryService _hospitals;

    public IpdController(IHospitalQueryService hospitals) => _hospitals = hospitals;

    // GET api/ipd/by-uhids?uhids=X,Y&hospitalKey=MRT
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

        var admissions = await ctx.IpdRegistrations
            .Where(i => uhidList.Contains(i.UhidNo))
            .OrderByDescending(i => i.Adm_Date)
            .ToListAsync();

        if (!admissions.Any())
            return NotFound(new { message = "No IPD records found." });

        var (doctorDict, sponsorDict) = await LoadLookupsAsync(ctx, admissions);
        var dischargeDict = await LoadDischargeLookupsAsync(ctx, admissions);

        var result = admissions
            .Select(a => MapToSummary(a, doctorDict, sponsorDict, dischargeDict, hospitalKey, entry.Name))
            .ToList();

        return Ok(result);
    }

    // GET api/ipd/detail/{admNo}?hospitalKey=MRT
    [HttpGet("detail/{admNo}")]
    public async Task<IActionResult> GetDetail(string admNo, [FromQuery] string hospitalKey)
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

        var admission = await ctx.IpdRegistrations
            .FirstOrDefaultAsync(i => i.Adm_No == admNo);

        if (admission == null)
            return NotFound(new { message = "Admission not found." });

        var (doctorDict, sponsorDict) = await LoadLookupsAsync(ctx, [admission]);

        return Ok(MapToDetail(admission, doctorDict, sponsorDict, hospitalKey, entry.Name));
    }

    // GET api/ipd/by-mobile/{mobile}?hospitalKey=MRT
    [HttpGet("by-mobile/{mobile}")]
    public async Task<IActionResult> GetByMobile(string mobile, [FromQuery] string hospitalKey)
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

        // Resolve UHIDs from PatientRegistration
        var uhidList = await ctx.PatientRegistrations
            .Where(p => p.MobileNo == mobile)
            .Select(p => p.UhidNo)
            .Distinct()
            .ToListAsync();

        if (!uhidList.Any())
            return NotFound(new { message = "No patient found for this mobile." });

        // Fetch IPD from same HisDb
        var admissions = await ctx.IpdRegistrations
            .Where(i => uhidList.Contains(i.UhidNo))
            .OrderByDescending(i => i.Adm_Date)
            .ToListAsync();

        if (!admissions.Any())
            return NotFound(new { message = "No IPD records found." });

        var (doctorDict, sponsorDict) = await LoadLookupsAsync(ctx, admissions);
        var dischargeDict = await LoadDischargeLookupsAsync(ctx, admissions);

        var result = admissions
            .Select(a => MapToSummary(a, doctorDict, sponsorDict, dischargeDict, hospitalKey, entry.Name))
            .ToList();

        return Ok(result);
    }

    // GET api/ipd/active  (JWT-protected — uses token claims)
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
            return Ok(new { active = false, admissions = Array.Empty<object>() });

        // Relieve_Status is null or empty string when still admitted
        var admissions = await ctx.IpdRegistrations
            .Where(i => uhidList.Contains(i.UhidNo)
                     && (i.Relieve_Status == null || i.Relieve_Status == ""))
            .OrderByDescending(i => i.Adm_Date)
            .ToListAsync();

        if (!admissions.Any())
            return Ok(new { active = false, admissions = Array.Empty<object>() });

        var (doctorDict, sponsorDict) = await LoadLookupsAsync(ctx, admissions);
        var dischargeDict = await LoadDischargeLookupsAsync(ctx, admissions);

        var result = admissions
            .Select(a => MapToSummary(a, doctorDict, sponsorDict, dischargeDict, hospitalKey, entry.Name))
            .ToList();

        return Ok(new { active = true, admissions = result });
    }

    // Private helpers

    private static async Task<Dictionary<string, DischargeReqLookup>> LoadDischargeLookupsAsync(
    HmsDbContext ctx, IEnumerable<IpdRegistration> admissions)
    {
        var admNos = admissions
            .Where(a => !string.IsNullOrEmpty(a.Adm_No))
            .Select(a => a.Adm_No!)
            .Distinct()
            .ToList();

        var dischargeReqs = await ctx.DischargeReqs
            .AsNoTracking()
            .Where(d => admNos.Contains(d.Adm_No!) && d.IsCancel != true)
            .OrderByDescending(d => d.DocDate)
            .ToListAsync();

        // Keep only latest per Adm_No
        return dischargeReqs
            .GroupBy(d => d.Adm_No!)
            .ToDictionary(
                g => g.Key,
                g => {
                    var d = g.First();
                    return new DischargeReqLookup(
                        d.DocNo,
                        d.DocDate,
                        d.DocTime,
                        d.Discharge_Status,
                        d.P_Condition,
                        d.Discharge_Mode switch
                        {
                            1 => "Relieved",
                            2 => "LAMA",
                            3 => "Referred",
                            4 => "Absconded",
                            5 => "Expired",
                            9 => "Normal",
                            _ => d.Discharge_Mode?.ToString()
                        }
                    );
                });
    }

    private static async Task<(Dictionary<int, DoctorLookup> doctors, Dictionary<int, SponsorLookup> sponsors)>
        LoadLookupsAsync(HmsDbContext ctx, IEnumerable<IpdRegistration> admissions)
    {
        var admList = admissions.ToList();

        var doctorIds = admList
            .Where(a => a.DocId.HasValue)
            .Select(a => a.DocId!.Value)
            .Distinct()
            .ToList();

        var sponsorIds = admList
            .Where(a => a.Sponsor_Id.HasValue)
            .Select(a => a.Sponsor_Id!.Value)
            .Distinct()
            .ToList();

        var doctors = await ctx.DoctorMasters
            .Where(d => doctorIds.Contains(d.Id))
            .Select(d => new DoctorLookup(d.Id, d.Doc_Name, d.Qual1, d.Qual2))
            .ToListAsync();

        var sponsors = await ctx.SponsorMasters
            .Where(s => sponsorIds.Contains(s.Id))
            .Select(s => new SponsorLookup(s.Id, s.Sponsor_Name))
            .ToListAsync();

        return (
            doctors.ToDictionary(d => d.Id),
            sponsors.ToDictionary(s => s.Id)
        );
    }

    private static IpdSummaryDto MapToSummary(
        IpdRegistration a,
        Dictionary<int, DoctorLookup> doctorDict,
        Dictionary<int, SponsorLookup> sponsorDict,
        Dictionary<string, DischargeReqLookup> dischargeDict,
        string hospitalKey,
        string hospitalName)
    {
        doctorDict.TryGetValue(a.DocId ?? -1, out var doc);
        sponsorDict.TryGetValue(a.Sponsor_Id ?? -1, out var sp);

        var qual = doc == null ? "" : string.Join(", ",
            new[] { doc.Qual1, doc.Qual2 }.Where(q => !string.IsNullOrWhiteSpace(q)));

        return new IpdSummaryDto
        {
            UhidNo = a.UhidNo,
            Adm_No = a.Adm_No,
            OpNo = a.OpNo,
            Adm_Date = a.Adm_Date,
            Timein = a.Timein,
            Adm_Type = a.Adm_Type,
            Patient_Type = a.Patient_Type,
            IpdType = a.IpdType,
            IsEmergencyAdm = a.IsEmergencyAdm,
            Pro_Diagnos = a.Pro_Diagnos,
            Surgery_Name = a.Surgery_Name,
            Relieve_Status = a.Relieve_Status,
            DOM = a.DOM,
            DoctorName = doc?.Doc_Name,
            DoctorQualification = qual,
            SponsorName = sp?.Sponsor_Name,
            Cur_Bed = a.Cur_Bed,
            IsDischarge = dischargeDict.ContainsKey(a.Adm_No ?? ""),
            DischargeDocNo = dischargeDict.TryGetValue(a.Adm_No ?? "", out var dr) ? dr.DocNo : null,
            DischargeDate = dr?.DischargeDate,
            DischargeTime = dr?.DischargeTime,
            DischargeStatus = dr?.DischargeStatus,
            DischargeMode = dr?.DischargeMode,
            PatientCondition = dr?.PatientCondition,
            HospitalKey = hospitalKey,
            HospitalName = hospitalName
        };
    }

    private static IpdDetailDto MapToDetail(
        IpdRegistration a,
        Dictionary<int, DoctorLookup> doctorDict,
        Dictionary<int, SponsorLookup> sponsorDict,
        string hospitalKey,
        string hospitalName)
    {
        doctorDict.TryGetValue(a.DocId ?? -1, out var doc);
        sponsorDict.TryGetValue(a.Sponsor_Id ?? -1, out var sp);

        var qual = doc == null ? "" : string.Join(", ",
            new[] { doc.Qual1, doc.Qual2 }.Where(q => !string.IsNullOrWhiteSpace(q)));

        return new IpdDetailDto
        {
            // Base fields (same as summary)
            UhidNo = a.UhidNo,
            Adm_No = a.Adm_No,
            OpNo = a.OpNo,
            Adm_Date = a.Adm_Date,
            Timein = a.Timein,
            Adm_Type = a.Adm_Type,
            Patient_Type = a.Patient_Type,
            IpdType = a.IpdType,
            IsEmergencyAdm = a.IsEmergencyAdm,
            Pro_Diagnos = a.Pro_Diagnos,
            Surgery_Name = a.Surgery_Name,
            Relieve_Status = a.Relieve_Status,
            DOM = a.DOM,
            DoctorName = doc?.Doc_Name,
            DoctorQualification = qual,
            SponsorName = sp?.Sponsor_Name,
            Cur_Bed = a.Cur_Bed,
            HospitalKey = hospitalKey,
            HospitalName = hospitalName,

            // Detail-only fields
            Adm_Mode = a.Adm_Mode,
            Ref_Type = a.Ref_Type,
            MLCNO = a.MLCNO,
            IsMlc = a.IsMlc,
            Accidental_Status = a.Accidental_Status,
            Package_Status = a.Package_Status,
            Policy_No = a.Policy_No,
            ClaimId = a.ClaimId,
            MStatus = a.MStatus,
            DateOfBirth = a.DateOfBirth,
            AgeInYears = a.AgeInYears,
            AgeInMonths = a.AgeInMonths,
            AgeInDays = a.AgeInDays,
            Mother_Name = a.Mother_Name,
            Remarks = a.Remarks,
            AbhaNo = a.AbhaNo,
            modepatient = a.modepatient,
            KnowTo = a.KnowTo
        };
    }

    private record DoctorLookup(int Id, string? Doc_Name, string? Qual1, string? Qual2);
    private record SponsorLookup(int Id, string? Sponsor_Name);

    private record DischargeReqLookup(
    string? DocNo,
    DateTime? DischargeDate,
    string? DischargeTime,
    string? DischargeStatus,
    string? PatientCondition,
    string? DischargeMode);
}