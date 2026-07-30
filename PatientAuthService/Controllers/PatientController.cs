using PatientAuthService.Data;
using PatientAuthService.Models;
using PatientAuthService.Models.DTOs;
using PatientAuthService.Models.Entities.ViphaHms;
using PatientAuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientAuthService.Helpers;

namespace PatientAuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientController : ControllerBase
{
    private readonly IHospitalQueryService _hospitals;

    public PatientController(IHospitalQueryService hospitals) => _hospitals = hospitals;

    // GET api/patient/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var mobile = User.FindFirst("mobile")?.Value;
        var hospitalKey = User.FindFirst("hospitalKey")?.Value;

        if (mobile == null || hospitalKey == null) return Unauthorized();

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null) return Unauthorized();

        await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

        var patient = await ctx.PatientRegistrations
            .Where(p => p.MobileNo == mobile)
            .OrderByDescending(p => p.RegistrationDate)
            .FirstOrDefaultAsync();

        if (patient == null) return NotFound();

        return Ok(new PatientDto
        {
            UhidNo = patient.UhidNo,
            PatientName = patient.PatientName,
            Gender = patient.Gender,
            AgeInYears = patient.AgeInYears,
            BloodGroup = patient.BloodGroup,
            MobileNo = patient.MobileNo,
            Address = patient.Address,
            District = patient.District,
            OpdType = patient.OpdType
        });
    }

    // GET api/patient/by-mobile/{mobile}?hospitalKey=MRT
    [HttpGet("by-mobile/{mobile}")]
    public async Task<IActionResult> GetByMobile(string mobile, [FromQuery] string hospitalKey)
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

        var patient = await ctx.PatientRegistrations
            .Where(p => p.MobileNo == mobile)
            .OrderByDescending(p => p.RegistrationDate)
            .FirstOrDefaultAsync();

        if (patient == null)
            return NotFound(new { message = "No patient found with this mobile number." });

        return Ok(new PatientDto
        {
            UhidNo = patient.UhidNo,
            OpNo = patient.OpNo,
            Title = patient.Title,
            PatientName = patient.PatientName,
            Gender = patient.Gender,
            DateOfBirth = patient.DateOfBirth,
            AgeInYears = patient.AgeInYears,
            BloodGroup = patient.BloodGroup,
            MobileNo = patient.MobileNo,
            EmailId = patient.EmailId,
            Address = patient.Address,
            Town = patient.Town,
            District = patient.District,
            OpdType = patient.OpdType,
            RegistrationDate = patient.RegistrationDate,
            RelativeName = patient.RelativeName,
            RelTitle = patient.RelTitle,
            Nationality = patient.Nationality,
            AdharNo = patient.AdharNo
        });
    }

    // GET api/patient/visits/{mobile}?hospitalKey=MRT
    [HttpGet("visits/{mobile}")]
    public async Task<IActionResult> GetVisits(string mobile, [FromQuery] string hospitalKey)
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

        var registrations = await ctx.PatientRegistrations
            .Where(p => p.MobileNo == mobile)
            .OrderByDescending(p => p.RegistrationDate)
            .ToListAsync();

        if (!registrations.Any())
            return NotFound(new { message = "No visits found." });

        var uhids = registrations.Select(r => r.UhidNo).Distinct().ToList();

        var consultancies = await ctx.PatientConsultancies
            .Where(c => uhids.Contains(c.UhidNo))
            .OrderByDescending(c => c.VisitDate)
            .ToListAsync();

        // Use shared helper — no more anonymous dicts here
        var (doctorDict, sponsorDict) = await LoadLookupsAsync(ctx, registrations, consultancies);

        var allVisits = MapRegistrations(registrations, doctorDict, sponsorDict)
            .Union(MapConsultancies(consultancies, doctorDict, sponsorDict), new VisitByOpNoComparer())
            .OrderByDescending(v => v.VisitDate)
            .ToList();

        return Ok(allVisits);
    }

    // GET api/patient/report/{mobile}?hospitalKey=MRT
    [HttpGet("report/{mobile}")]
    public async Task<IActionResult> GetPatientReport(string mobile, [FromQuery] string hospitalKey)
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

        var registrations = await ctx.PatientRegistrations
            .Where(p => p.MobileNo == mobile)
            .OrderByDescending(p => p.RegistrationDate)
            .ToListAsync();

        if (!registrations.Any())
            return NotFound(new { message = "No records found for this mobile." });

        var latest = registrations.First();
        var uhids = registrations.Select(r => r.UhidNo).Distinct().ToList();

        var consultancies = await ctx.PatientConsultancies
            .Where(c => uhids.Contains(c.UhidNo))
            .OrderByDescending(c => c.VisitDate)
            .ToListAsync();

        var (doctorDict, sponsorDict) = await LoadLookupsAsync(ctx, registrations, consultancies);

        var visits = MapRegistrations(registrations, doctorDict, sponsorDict)
            .Union(MapConsultancies(consultancies, doctorDict, sponsorDict), new VisitByOpNoComparer())
            .OrderByDescending(v => v.VisitDate)
            .ToList();

        // For report/{mobile}: latest row is both primary and latest
        return Ok(BuildReport(latest, latest, visits));
    }

    [HttpGet("report-by-uhids")]
    public async Task<IActionResult> GetReportByUhids(
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

        var registrations = await ctx.PatientRegistrations
            .Where(p => uhidList.Contains(p.UhidNo))
            .OrderByDescending(p => p.RegistrationDate)
            .ToListAsync();

        if (!registrations.Any())
            return NotFound(new { message = "No records found." });

        var latest = registrations.First();  // most recent → mutable fields (age)
        var primary = registrations.Last();   // earliest   → master demographics

        var consultancies = await ctx.PatientConsultancies
            .Where(c => uhidList.Contains(c.UhidNo))
            .OrderByDescending(c => c.VisitDate)
            .ToListAsync();

        var (doctorDict, sponsorDict) = await LoadLookupsAsync(ctx, registrations, consultancies);

        var visits = MapRegistrations(registrations, doctorDict, sponsorDict)
            .Union(MapConsultancies(consultancies, doctorDict, sponsorDict), new VisitByOpNoComparer())
            .OrderByDescending(v => v.VisitDate)
            .ToList();

        return Ok(BuildReport(primary, latest, visits));
    }

    private static async Task<(Dictionary<int, DoctorLookup>, Dictionary<int, SponsorLookup>)>
        LoadLookupsAsync(
            AuthDbContext ctx,
            IEnumerable<PatientRegistration> regs,
            IEnumerable<PatientConsultancy> cons)
    {
        var doctorIds = regs.Where(r => r.DoctorId.HasValue).Select(r => r.DoctorId!.Value)
            .Union(cons.Where(c => c.DoctorId.HasValue).Select(c => c.DoctorId!.Value))
            .Distinct().ToList();

        var sponsorIds = regs.Where(r => r.SponsorId.HasValue).Select(r => (int)r.SponsorId!.Value)
            .Union(cons.Where(c => c.SponsorId.HasValue).Select(c => (int)c.SponsorId!.Value))
            .Distinct().ToList();

        // Project directly into our concrete record types — no anonymous types
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

    private static IEnumerable<VisitDetailDto> MapRegistrations(
        IEnumerable<PatientRegistration> rows,
        Dictionary<int, DoctorLookup> doctorDict,
        Dictionary<int, SponsorLookup> sponsorDict) => rows.Select(r =>
        {
            doctorDict.TryGetValue(r.DoctorId ?? -1, out var doc);
            sponsorDict.TryGetValue((int)(r.SponsorId ?? -1), out var sp);
            var qual = doc == null ? "" : string.Join(", ",
                new[] { doc.Qual1, doc.Qual2 }.Where(q => !string.IsNullOrWhiteSpace(q)));

            return new VisitDetailDto
            {
                OpNo = r.OpNo,
                UhidNo = r.UhidNo,
                RegistrationDate = r.RegistrationDate,
                VisitDate = r.RegistrationDate,
                RegistrationTime = r.RegistrationTime,
                OpdType = r.OpdType,
                AppointmentNo = r.AppointmentNo,
                AppointmentTime = r.AppointmentTime,
                DoctorName = doc?.Doc_Name,
                DoctorQualification = qual,
                SponsorName = sp?.Sponsor_Name,
                TotalAmt = r.TotalAmt,
                ConsultCharge = r.ConsultCharge,
                DisAmt = r.DisAmt,
                ReceiptMode = r.ReceiptMode,
                VisitNo = r.VisitNo,
                VisitSource = "Registration"
            };
        });

    private static IEnumerable<VisitDetailDto> MapConsultancies(
        IEnumerable<PatientConsultancy> rows,
        Dictionary<int, DoctorLookup> doctorDict,
        Dictionary<int, SponsorLookup> sponsorDict) => rows.Select(c =>
        {
            doctorDict.TryGetValue(c.DoctorId ?? -1, out var doc);
            sponsorDict.TryGetValue((int)(c.SponsorId ?? -1), out var sp);
            var qual = doc == null ? "" : string.Join(", ",
                new[] { doc.Qual1, doc.Qual2 }.Where(q => !string.IsNullOrWhiteSpace(q)));

            return new VisitDetailDto
            {
                OpNo = c.OpNo,
                UhidNo = c.UhidNo,
                RegistrationDate = c.VisitDate,
                VisitDate = c.VisitDate,
                RegistrationTime = c.VisitTime,
                OpdType = c.OpdType,
                AppointmentNo = c.AppointmentNo,
                AppointmentTime = c.AppointmentTime,
                DoctorName = doc?.Doc_Name,
                DoctorQualification = qual,
                SponsorName = sp?.Sponsor_Name,
                TotalAmt = c.TotalAmt,
                ConsultCharge = c.ConsultCharge,
                DisAmt = c.DisAmt,
                ReceiptMode = c.ReceiptMode,
                VisitNo = c.VisitNo,
                VisitSource = "Consultancy"
            };
        });

    private static PatientReportDto BuildReport(
        PatientRegistration primary,  
        PatientRegistration latest,  
        List<VisitDetailDto> visits) => new()
        {
            UhidNo = primary.UhidNo,
            Title = primary.Title,
            PatientName = primary.PatientName,
            Gender = primary.Gender,
            DateOfBirth = primary.DateOfBirth,
            AgeInYears = latest.AgeInYears,          
            BloodGroup = primary.BloodGroup,
            MobileNo = primary.MobileNo,
            AltMobileNo = primary.AltMobileNo,
            EmailId = primary.EmailId,
            Address = primary.Address,
            Town = primary.Town,
            District = primary.District,
            PinCode = primary.PinCode,
            Nationality = primary.Nationality,
            RelTitle = primary.RelTitle,    
            RelativeName = primary.RelativeName,
            MotherName = primary.MotherName,
            Religion = primary.Religion,
            IDProofType = primary.IDProofType,
            IDProofNo = primary.IDProofNo,
            AdharNo = primary.AdharNo,
            Visits = visits,
            TotalVisits = visits.Count,
            FirstVisit = visits.Any() ? visits.Min(v => v.VisitDate) : null,
            LastVisit = visits.Any() ? visits.Max(v => v.VisitDate) : null
        };

    // Private lookup record types
    // Concrete types are required so EF can project into them and the Map
    // helpers receive strongly-typed dictionaries (no anonymous types).

    private record DoctorLookup(int Id, string? Doc_Name, string? Qual1, string? Qual2);
    private record SponsorLookup(int Id, string? Sponsor_Name);
}