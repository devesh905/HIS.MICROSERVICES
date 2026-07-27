using Microsoft.Extensions.Configuration;
using PatientAuthService.Models.Entities.ViphaHms;
using Shared.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PatientAuthService.Services;

public class JwtService : IJwtService
{
    private readonly JwtTokenFactory _factory;
    private readonly IConfiguration _config;

    public JwtService(JwtTokenFactory factory, IConfiguration config)
    {
        _factory = factory;
        _config = config;
    }

    public string GenerateToken(PatientRegistration patient)
        => GenerateToken(patient, "");

    public string GenerateToken(PatientRegistration patient, string hospitalKey)
    {
        var claims = new[]
        {
            new Claim("uhid",        patient.UhidNo      ?? ""),
            new Claim("name",        patient.PatientName ?? ""),
            new Claim("mobile",      patient.MobileNo    ?? ""),
            new Claim("hospitalKey", hospitalKey),
            new Claim(ClaimTypes.Role, "patient"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expiryHours = int.Parse(_config["Jwt:ExpiryInHours"]!);
        return _factory.CreateToken(claims, TimeSpan.FromHours(expiryHours));
    }

    public string GenerateGuestToken(string mobile)
    {
        var claims = new[]
        {
            new Claim("mobile", mobile),
            new Claim(ClaimTypes.Role, "GuestPatient"),
            new Claim("isGuest", "true")
        };

        return _factory.CreateToken(claims, TimeSpan.FromHours(1));
    }
}