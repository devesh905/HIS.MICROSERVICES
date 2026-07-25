using EmployeeAuthService.Models.Entities.Vipha;

namespace HIS.API.Services;

public interface IJwtService
{
    string GenerateEmployeeToken(User user, string hospitalKey);
}