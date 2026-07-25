using EmployeeAuthService.Data;
using EmployeeAuthService.Models.Entities.Portal;


namespace EmployeeAuthService.Services;

public interface ILoginActivityService
{
    Task LogAsync(LoginActivityLog log);
}

public class LoginActivityService : ILoginActivityService
{
    private readonly PortalDbContext _db;

    public LoginActivityService(PortalDbContext db) => _db = db;

    public async Task LogAsync(LoginActivityLog log)
    {
        _db.LoginActivityLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}