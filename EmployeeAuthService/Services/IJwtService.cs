using EmployeeAuthService.Models.Entities.Vipha;

namespace EmployeeAuthService.Services;

public interface IJwtService
{
    string GenerateEmployeeToken(User user, string hospitalKey);
}