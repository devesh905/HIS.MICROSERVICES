using Microsoft.EntityFrameworkCore;
using PatientAuthService.Data;
using PatientAuthService.Models.Entities.PatientPortalDb;

namespace PatientAuthService.Services;

public interface ILoginActivityService
{
    void LogAsync(LoginActivityLog entry);
}

public class LoginActivityService : ILoginActivityService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LoginActivityService> _logger;

    public LoginActivityService(IServiceScopeFactory scopeFactory, ILogger<LoginActivityService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public void LogAsync(LoginActivityLog entry)
    {
        // Fire-and-forget on a background task with its OWN DbContext scope,
        // since the request's PortalDbContext will be disposed once the
        // controller returns.
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PortalDbContext>();

                var existing = await db.LoginActivityLogs.FirstOrDefaultAsync(x =>
                    x.UserType == entry.UserType &&
                    x.HospitalKey == entry.HospitalKey &&
                    (entry.UhidNo != null ? x.UhidNo == entry.UhidNo : x.UserName == entry.UserName));

                var now = DateTime.UtcNow;

                if (existing == null)
                {
                    entry.FirstLoginAt = now;
                    entry.LastLoginAt = now;
                    entry.LoginCount = 1;
                    entry.LoginAt = now;
                    db.LoginActivityLogs.Add(entry);
                }
                else
                {
                    existing.LastLoginAt = now;
                    existing.LoginAt = now;
                    existing.LoginCount += 1;
                    existing.MobileNo = entry.MobileNo ?? existing.MobileNo;
                    existing.DisplayName = entry.DisplayName ?? existing.DisplayName;
                    existing.HospitalName = entry.HospitalName ?? existing.HospitalName;
                    existing.LoginMethod = entry.LoginMethod;
                    existing.IsSuccess = entry.IsSuccess;
                    existing.IpAddress = entry.IpAddress ?? existing.IpAddress;
                    existing.UserAgent = entry.UserAgent ?? existing.UserAgent;
                }

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Background login activity log failed for {UserType} {User}",
                    entry.UserType, entry.UserName ?? entry.MobileNo);
            }
        });
    }
}