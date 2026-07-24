using PatientAuthService.Data;
using PatientAuthService.Models.DTOs;
using PatientAuthService.Models.Entities.ViphaHms;
using PatientAuthService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PatientAuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IHospitalQueryService _hospitals;
    private readonly IOtpService _otp;
    private readonly ISmsService _sms;
    private readonly IJwtService _jwt;
    private readonly ILoginActivityService _loginActivity;

    public AuthController(
        IHospitalQueryService hospitals,
        IOtpService otp,
        ISmsService sms,
        IJwtService jwt,
        ILoginActivityService loginActivity)
    {
        _hospitals = hospitals;
        _otp = otp;
        _sms = sms;
        _jwt = jwt;
        _loginActivity = loginActivity;
    }

    // POST api/auth/send-otp
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Mobile))
            return BadRequest(new { message = "Mobile number is required." });

        var normalizedMobile = NormalizeMobile(req.Mobile);

        // Check if this mobile exists in ANY hospital (both searched in parallel)
        var existsResults = await _hospitals.QueryAllAsync(async ctx =>
            await ctx.PatientRegistrations.AnyAsync(p => p.MobileNo == normalizedMobile));

        var existsAnywhere = existsResults.Any(r => r.Result);

        // Still generate OTP regardless — don't leak which hospital has the patient
        var otp = await _otp.GenerateAsync(req.Mobile);
        var org = "A Personal Health Record";

        //try
        //{
        //    var message = $"{otp} is your otp for {org} and it will be valid for 15 minutes, Subharti University Jai Hind.";
        //    await _sms.SendAsync(normalizedMobile, message);
        //}
        //catch (Exception ex)
        //{
        //    // SMS failed — don't leave the OTP silently orphaned in cache with no way to reach the user
        //    return StatusCode(500, new { message = "Could not send OTP. Please try again in a moment." });
        //}

        return Ok(new
        {
            message = existsAnywhere
                ? "OTP sent successfully."
                : "OTP sent successfully.",
            otp
        });
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest req)
    {
        var valid = await _otp.VerifyAsync(req.Mobile, req.Otp);
        if (!valid)
            return BadRequest(new { message = "Invalid or expired OTP." });

        var normalizedMobile = NormalizeMobile(req.Mobile);

        // Query ALL hospitals in parallel
        var hospitalResults = await _hospitals.QueryAllAsync(async (ctx, hospital) =>
        {
            // Pull all registration rows for this mobile from this hospital
            return await ctx.PatientRegistrations
                .Where(p => p.MobileNo == normalizedMobile)
                .OrderByDescending(p => p.RegistrationDate)
                .Select(p => new
                {
                    p.UhidNo,
                    p.Title,
                    p.PatientName,
                    p.Gender,
                    p.AgeInYears,
                    p.RegistrationDate,
                    p.OpdType,
                    p.RelativeName,
                    p.OpNo,
                    p.DateOfBirth,
                    p.BloodGroup,
                    p.MobileNo,
                    p.EmailId,
                    p.Address,
                    p.Town,
                    p.District,
                    p.Nationality,
                    p.AdharNo,
                    p.RelTitle
                })
                .ToListAsync();
        });

        //var anyFound = hospitalResults.Any(r => r.Result?.Any() == true);
        //if (!anyFound)
        //    return NotFound(new { message = "Patient not found." });

        //  Check any hospital has data
        var anyFound = hospitalResults.Any(r => r.Result?.Any() == true);
        if (!anyFound)
        {
            // New patient — issue a guest token so they can self-register
            var guestToken = _jwt.GenerateGuestToken(normalizedMobile);
            return Ok(new
            {
                requiresSelection = false,
                isNewPatient = true,
                token = guestToken,
                mobile = normalizedMobile,
                message = "No existing registration found. Please complete OPD registration."
            });
        }

        //  Build profile cards — one group per logical person per hospital ─
        //  A patient registered in BOTH hospitals → two separate cards.
        var allProfiles = new List<object>();

        foreach (var hr in hospitalResults.Where(r => r.Result?.Any() == true))
        {
            var rows = hr.Result;

            // Enrich with latest consultancy visit date (same hospital context)
            var allUhidsInHospital = rows.Select(r => r.UhidNo).Distinct().ToList();

            // We need a fresh context to query consultancies for this hospital
            var hospitalEntry = HospitalRegistry.Find(hr.Key)!;
            await using var ctx2 = _hospitals.CreateHisContext(hospitalEntry.HisCs);

            var latestConsultancy = await ctx2.PatientConsultancies
                .Where(c => allUhidsInHospital.Contains(c.UhidNo) && c.VisitDate.HasValue)
                .GroupBy(c => c.UhidNo)
                .Select(g => new { UhidNo = g.Key, LastVisit = g.Max(c => c.VisitDate) })
                .ToListAsync();

            var conDict = latestConsultancy.ToDictionary(x => x.UhidNo, x => x.LastVisit);

            // Group rows by logical person (Name + Relative + Gender)
            // This merges duplicate UHIDs for the same real patient within this hospital
            var grouped = rows
                .GroupBy(p => new
                {
                    Name = (p.PatientName ?? "").Trim().ToUpper(),
                    Relative = (p.RelativeName ?? "").Trim().ToUpper(),
                    p.Gender
                })
                .Select(g =>
                {
                    var allUhidList = g.Select(x => x.UhidNo).Distinct().ToList();

                    // Latest reg date across all UHIDs in this group
                    var regLastVisit = g.Max(x => x.RegistrationDate);

                    // Latest consultancy date across all UHIDs in this group
                    var latestCon = allUhidList
                        .Where(u => conDict.ContainsKey(u))
                        .Select(u => conDict[u])
                        .Where(d => d.HasValue)
                        .Select(d => d!.Value)
                        .DefaultIfEmpty(DateTime.MinValue)
                        .Max();

                    var lastVisit = latestCon > regLastVisit ? latestCon : regLastVisit;

                    // Earliest UHID = primary (most original record)
                    var primaryRow = g.OrderBy(x => x.RegistrationDate).First();

                    return new
                    {
                        // Hospital identity — key for dashboard routing
                        HospitalKey = hr.Key,
                        HospitalName = hr.Name,

                        // Patient identity
                        UhidNo = primaryRow.UhidNo,
                        AllUhids = string.Join(",", allUhidList),

                        // Display fields
                        primaryRow.Title,
                        PatientName = primaryRow.PatientName,
                        primaryRow.Gender,
                        primaryRow.AgeInYears,
                        primaryRow.OpdType,

                        // Last visit — most recent of reg + consultancy
                        RegistrationDate = lastVisit
                    };
                })
                .OrderByDescending(p => p.RegistrationDate)
                .ToList<object>();

            allProfiles.AddRange(grouped);
        }

        //  Multiple profiles (same hospital different people, or cross-hospital) ─
        //       Sort by last visit descending across all hospitals
        if (allProfiles.Count > 1)
        {
            var sortedProfiles = allProfiles
                .Cast<dynamic>()
                .OrderByDescending(p => (DateTime)p.RegistrationDate)
                .ToList<object>();

            return Ok(new
            {
                requiresSelection = true,
                mobile = normalizedMobile,
                profiles = sortedProfiles
            });
        }

        // Single profile across ALL hospitals → auto-login, skip selection screen
        {
            dynamic singleProfile = allProfiles[0];
            string hKey = singleProfile.HospitalKey;
            string allUhids = singleProfile.AllUhids;
            string primaryUhid = singleProfile.UhidNo;

            var entry = HospitalRegistry.Find(hKey)!;
            await using var ctx3 = _hospitals.CreateHisContext(entry.HisCs);

            var patient = await ctx3.PatientRegistrations
                .Where(p => p.UhidNo == primaryUhid && p.MobileNo == normalizedMobile)
                .OrderBy(p => p.RegistrationDate)
                .FirstOrDefaultAsync();

            if (patient == null)
                return NotFound(new { message = "Patient not found." });

            var token = _jwt.GenerateToken(patient, entry.Key);
            var dto = BuildPatientDto(patient, hKey, entry.Name);


            var (ip, ua) = GetClientInfo();
           _loginActivity.LogAsync(new Models.Entities.PatientPortalDb.LoginActivityLog
            {
                UserType = "Patient",
                UhidNo = patient.UhidNo,
                MobileNo = normalizedMobile,
                DisplayName = patient.PatientName,
                HospitalKey = hKey,
                HospitalName = entry.Name,
                LoginMethod = "OTP",
                IsSuccess = true,
                IpAddress = ip,
                UserAgent = ua
            });

            return Ok(new
            {
                requiresSelection = false,
                token,
                allUhids,
                hospitalKey = hKey,
                patient = dto
            });
        }
    }

    // POST api/auth/select-profile
    [HttpPost("select-profile")]
    public async Task<IActionResult> SelectProfile([FromBody] SelectProfileRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Mobile) || string.IsNullOrWhiteSpace(req.HospitalKey))
            return BadRequest(new { message = "Mobile and HospitalKey are required." });

        var normalizedMobile = NormalizeMobile(req.Mobile);

        // Resolve the correct hospital context
        var entry = HospitalRegistry.Find(req.HospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {req.HospitalKey}" });

        // Build UHID list for this logical person
        var uhidList = new List<string>();
        if (!string.IsNullOrWhiteSpace(req.AllUhids))
            uhidList = req.AllUhids.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        else if (!string.IsNullOrWhiteSpace(req.UhidNo))
            uhidList = [req.UhidNo];

        if (!uhidList.Any())
            return BadRequest(new { message = "No UHID provided." });

        await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

        var patient = await ctx.PatientRegistrations
            .Where(p => p.MobileNo == normalizedMobile && uhidList.Contains(p.UhidNo))
            .OrderBy(p => p.RegistrationDate)
            .FirstOrDefaultAsync();

        if (patient == null)
            return NotFound(new { message = "Profile not found." });

        //var token = _jwt.GenerateToken(patient);
        var token = _jwt.GenerateToken(patient, entry.Key);
        var dto = BuildPatientDto(patient, entry.Key, entry.Name);

        var (ip, ua) = GetClientInfo();
        _loginActivity.LogAsync(new Models.Entities.PatientPortalDb.LoginActivityLog
        {
            UserType = "Patient",
            UhidNo = patient.UhidNo,
            MobileNo = normalizedMobile,
            DisplayName = patient.PatientName,
            HospitalKey = entry.Key,
            HospitalName = entry.Name,
            LoginMethod = "OTP-MultiProfile",
            IsSuccess = true,
            IpAddress = ip,
            UserAgent = ua
        });

        return Ok(new
        {
            token,
            allUhids = uhidList,
            hospitalKey = entry.Key,
            patient = dto
        });
    }

    // Helpers

    private static string NormalizeMobile(string mobile)
    {
        if (mobile.StartsWith("+91"))
            return mobile.Substring(3);
        if (mobile.StartsWith("91") && mobile.Length == 12)
            return mobile.Substring(2);
        return mobile;
    }

    private (string? ip, string? ua) GetClientInfo()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();
        return (ip, ua);
    }

    private static PatientDto BuildPatientDto(
        PatientRegistration p,
        string hospitalKey,
        string hospitalName) => new()
        {
            UhidNo = p.UhidNo,
            OpNo = p.OpNo,
            Title = p.Title,
            PatientName = p.PatientName,
            Gender = p.Gender,
            DateOfBirth = p.DateOfBirth,
            AgeInYears = p.AgeInYears,
            BloodGroup = p.BloodGroup,
            MobileNo = p.MobileNo,
            EmailId = p.EmailId,
            Address = p.Address,
            Town = p.Town,
            District = p.District,
            State = p.StateId,
            City = p.CityId,
            OpdType = p.OpdType,
            RegistrationDate = p.RegistrationDate,
            RelativeName = p.RelativeName,
            RelTitle = p.RelTitle,
            Nationality = p.Nationality,
            AdharNo = p.AdharNo,
            HospitalKey = hospitalKey,
            HospitalName = hospitalName
        };
}