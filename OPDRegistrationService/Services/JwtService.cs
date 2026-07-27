using OPDRegistrationService.Models.Entities.Hms;
using Shared.Authentication;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace OPDRegistrationService.Services;

public class JwtService : IJwtService
{
    private readonly JwtTokenFactory _factory;
    private readonly IConfiguration _config;

    public JwtService(JwtTokenFactory factory, IConfiguration config)
    {
        _factory = factory;
        _config = config;
    }

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

        var expiryHours = int.TryParse(_config["Jwt:ExpiryInHours"], out var h) ? h : 24;
        return _factory.CreateToken(claims, TimeSpan.FromHours(expiryHours));
    }
}