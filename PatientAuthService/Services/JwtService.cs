using PatientAuthService.Data;
using PatientAuthService.Models.Entities.ViphaHms;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PatientAuthService.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config) => _config = config;

    public string GenerateToken(PatientRegistration patient)
        => GenerateToken(patient, "");

    public string GenerateToken(PatientRegistration patient, string hospitalKey)
    {
        var key = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("uhid",        patient.UhidNo      ?? ""),
            new Claim("name",        patient.PatientName  ?? ""),
            new Claim("mobile",      patient.MobileNo     ?? ""),
            new Claim("hospitalKey", hospitalKey),
            new Claim(ClaimTypes.Role,    "patient"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(
                                    int.Parse(_config["Jwt:ExpiryInHours"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateGuestToken(string mobile)
    {
        var claims = new[]
        {
        new Claim("mobile", mobile),
        new Claim(ClaimTypes.Role, "GuestPatient"),
        new Claim("isGuest", "true")
    };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),   
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}