using EmployeeAuthService.Models.DTOs;
using EmployeeAuthService.Models.Entities.Vipha;
using EmployeeAuthService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeAuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeAuthController : ControllerBase
{
    private readonly IHospitalQueryService _hospitals;
    private readonly IOtpService _otp;
    private readonly ISmsService _sms;
    private readonly IJwtService _jwt;
    private readonly ILogger<EmployeeAuthController> _logger;
    private readonly ILoginActivityService _loginActivity;

    public EmployeeAuthController(
        IHospitalQueryService hospitals,   
        IOtpService otp,
        ISmsService sms,
        IJwtService jwt,
        ILogger<EmployeeAuthController> logger,
        ILoginActivityService loginActivity)
    {
        _hospitals = hospitals;
        _otp = otp;
        _sms = sms;
        _jwt = jwt;
        _logger = logger;
        _loginActivity = loginActivity;
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendEmployeeOtpRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserName))
            return BadRequest(new { message = "UserName is required." });

        var userName = req.UserName.Trim();

        var existsResults = await _hospitals.QueryAllViphaAsync(async (ctx, _) =>
            await ctx.Users.FirstOrDefaultAsync(u => u.UserName == userName && u.IsActive));

        // user exists but has no mobile
        var userExistsNoMobile = existsResults.Any(r =>
            r.Result != null && string.IsNullOrWhiteSpace(r.Result.Mobile));

        var found = existsResults.FirstOrDefault(r =>
            r.Result != null && !string.IsNullOrWhiteSpace(r.Result.Mobile));

        if (found == null)
        {
            if (userExistsNoMobile)
            {
                // User found but no mobile — tell them clearly
                return BadRequest(new
                {
                    message = "Your mobile number is not registered in the system. Please contact the administrator to add your mobile number to your account.",
                    errorCode = "NO_MOBILE"
                });
            }

            // User doesn't exist at all — keep vague for security
            _logger.LogWarning("Employee OTP request for unknown/inactive user: {UserName}", userName);
            return BadRequest(new
            {
                message = "Username not found. Please check your username or contact the administrator.",
                errorCode = "USER_NOT_FOUND"
            });
        }

        var otp = await _otp.GenerateAsync(found.Result!.Mobile!);
        var mobile = found.Result.Mobile!;
        var org = "A Personal Health Record";

        //try
        //{
        //    var message = $"{otp} is your otp for {org} and it will be valid for 15 minutes, Subharti University Jai Hind.";
        //    await _sms.SendAsync(mobile, message);
        //}
        //catch (Exception ex)
        //{
        //    // SMS failed — don't leave the OTP silently orphaned in cache with no way to reach the user
        //    return StatusCode(500, new { message = "Could not send OTP. Please try again in a moment." });
        //}

        return Ok(new
        {
            message = "OTP sent successfully.",
            otp,
            maskedMobile = MaskMobile(found.Result.Mobile!)
        });
    }

    // POST api/employeeauth/verify-otp
    // Client sends { userName, otp }
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyEmployeeOtpRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Otp))
            return BadRequest(new { message = "UserName and OTP are required." });

        var userName = req.UserName.Trim();

        // 1. Find this user across ALL hospitals
        var hospitalResults = await _hospitals.QueryAllViphaAsync(async (ctx, _) =>
            await ctx.Users
                .Where(u => u.UserName == userName && u.IsActive)
                .FirstOrDefaultAsync());

        // Collect all hospitals where this user exists with a mobile number
        var foundInHospitals = hospitalResults
            .Where(r => r.Result?.Mobile != null)
            .ToList();

        if (!foundInHospitals.Any())
            return BadRequest(new { message = "Invalid username or OTP." });

        // 2. Verify OTP — all hospital entries for this user share the same mobile,
        //    so verify against the first found
        var primaryEntry = foundInHospitals.First();
        var mobile = primaryEntry.Result!.Mobile!;

        var valid = await _otp.VerifyAsync(mobile, req.Otp);
        if (!valid)
            return BadRequest(new { message = "Invalid or expired OTP." });

        // 3. Build profile cards — one per hospital where user exists
        //    (mirrors patient profile card logic)
        var allProfiles = foundInHospitals.Select(hr => new
        {
            HospitalKey = hr.Key,
            HospitalName = hr.Name,
            hr.Result!.Id,
            hr.Result.Code,
            hr.Result.UserName,
            hr.Result.FirstName,
            hr.Result.LastName,
            hr.Result.EmpCode,
            hr.Result.Email,
            hr.Result.Mobile,
            hr.Result.Designation,
            hr.Result.DeptId,
            hr.Result.IsSysAdmin,
            hr.Result.IsSysSubAdmin,
            hr.Result.DateOfJoining,
        }).ToList();

        // Single hospital — skip selection, issue token immediately
        if (allProfiles.Count == 1)
        {
            var profile = allProfiles[0];
            var entry = HospitalRegistry.Find(profile.HospitalKey)!;
            var token = _jwt.GenerateEmployeeToken(primaryEntry.Result!, entry.Key);

            var (ip, ua) = GetClientInfo();
            _loginActivity.LogAsync(new Models.Entities.Portal.LoginActivityLog
            {
                UserType = "Employee",
                UserName = userName,
                MobileNo = mobile,
                DisplayName = $"{primaryEntry.Result!.FirstName} {primaryEntry.Result.LastName}".Trim(),
                HospitalKey = entry.Key,
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
                hospitalKey = entry.Key,
                employee = BuildEmployeeDto(primaryEntry.Result!, entry.Key, entry.Name)
            });
        }

        // Found in MULTIPLE hospitals — let user pick which context to log into
        return Ok(new
        {
            requiresSelection = true,
            userName,
            profiles = allProfiles
                .OrderBy(p => p.HospitalName)
                .ToList<object>()
        });
    }

    // POST api/employeeauth/select-hospital
    // Client sends { userName, hospitalKey } after user picks from the list
    [HttpPost("select-hospital")]
    public async Task<IActionResult> SelectHospital([FromBody] SelectEmployeeHospitalRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.HospitalKey))
            return BadRequest(new { message = "UserName and HospitalKey are required." });

        var entry = HospitalRegistry.Find(req.HospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {req.HospitalKey}" });

        var userName = req.UserName.Trim();

        await using var ctx = _hospitals.CreateViphaContext(entry.ViphaCs);

        var user = await ctx.Users
            .Where(u => u.UserName == userName && u.IsActive)
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new { message = "Employee not found in selected hospital." });

        var token = _jwt.GenerateEmployeeToken(user, entry.Key);

        var (ip, ua) = GetClientInfo();
        await _loginActivity.LogAsync(new Models.Entities.Portal.LoginActivityLog
        {
            UserType = "Employee",
            UserName = userName,
            MobileNo = user.Mobile,
            DisplayName = $"{user.FirstName} {user.LastName}".Trim(),
            HospitalKey = entry.Key,
            HospitalName = entry.Name,
            LoginMethod = "OTP-MultiHospital",
            IsSuccess = true,
            IpAddress = ip,
            UserAgent = ua
        });

        return Ok(new
        {
            token,
            hospitalKey = entry.Key,
            employee = BuildEmployeeDto(user, entry.Key, entry.Name)
        });
    }

    // Helpers

    private static string MaskMobile(string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile)) return "****";
        if (mobile.Length <= 4) return "****";
        return new string('*', mobile.Length - 4) + mobile[^4..];
    }

    private (string? ip, string? ua) GetClientInfo()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString();
        return (ip, ua);
    }

    private static EmployeeDto BuildEmployeeDto(User u, string hospitalKey, string hospitalName) => new()
    {
        Id = u.Id,
        Code = u.Code,
        UserName = u.UserName,
        FirstName = u.FirstName,
        LastName = u.LastName,
        EmpCode = u.EmpCode,
        Email = u.Email,
        Mobile = u.Mobile,
        Designation = u.Designation,
        DeptId = u.DeptId,
        IsSysAdmin = u.IsSysAdmin,
        IsSysSubAdmin = u.IsSysSubAdmin,
        DateOfJoining = u.DateOfJoining,
        HospitalKey = hospitalKey,      
        HospitalName = hospitalName     
    };
}