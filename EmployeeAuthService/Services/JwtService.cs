using EmployeeAuthService.Models.Entities.Vipha;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmployeeAuthService.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config) => _config = config;

    public string GenerateEmployeeToken(User user, string hospitalKey)
    {
        var claims = new List<Claim>
{
    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new(ClaimTypes.Name,           user.UserName    ?? ""),
    new(ClaimTypes.Role,           "employee"),          
    new("employeeId",              user.Id.ToString()), 
    new("empCode",                 user.EmpCode     ?? ""),
    new("designation",             user.Designation ?? ""),
    new("isSysAdmin",              user.IsSysAdmin.ToString()),
    new("isSysSubAdmin",           user.IsSysSubAdmin.ToString()),
    new("hospitalKey",             hospitalKey),
    new("userType",                "employee"),
};
        //    var claims = new List<Claim>
        //{
        //    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        //    new(ClaimTypes.Name,           user.UserName    ?? ""),
        //    new("empCode",                 user.EmpCode     ?? ""),
        //    new("designation",             user.Designation ?? ""),
        //    new("isSysAdmin",              user.IsSysAdmin.ToString()),
        //    new("isSysSubAdmin",           user.IsSysSubAdmin.ToString()),
        //    new("hospitalKey",             hospitalKey),   // ← now included
        //    new("userType",                "employee"),
        //};

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
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