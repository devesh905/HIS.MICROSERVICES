using EmployeeAuthService.Models.Entities.Vipha;
using PatientAuthService.Services;
using Shared.Authentication;
using System.Security.Claims;

namespace EmployeeAuthService.Services;

public class JwtService : IJwtService
{
    private readonly JwtTokenFactory _factory;

    public JwtService(JwtTokenFactory factory) => _factory = factory;

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

        return _factory.CreateToken(claims, TimeSpan.FromHours(8));
    }
}