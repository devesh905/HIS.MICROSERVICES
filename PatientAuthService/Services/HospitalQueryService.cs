using Microsoft.EntityFrameworkCore;
using PatientAuthService.Data;

namespace PatientAuthService.Services;

public class HospitalQueryService : IHospitalQueryService
{
    private readonly IConfiguration _config;
    private readonly ILogger<HospitalQueryService> _logger;
    private static readonly TimeSpan HospitalQueryTimeout = TimeSpan.FromSeconds(6);

    public HospitalQueryService(IConfiguration config, ILogger<HospitalQueryService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public AuthDbContext CreateHisContext(string csName)
    {
        var cs = _config.GetConnectionString(csName)
                 ?? throw new InvalidOperationException($"Connection string '{csName}' not found.");
        var opts = new DbContextOptionsBuilder<AuthDbContext>().UseSqlServer(cs).Options;
        return new AuthDbContext(opts);
    }

    public async Task<List<HospitalResult<T>>> QueryAllAsync<T>(Func<AuthDbContext, Task<T>> query)
        => await QueryAllAsync((ctx, _) => query(ctx));

    public async Task<List<HospitalResult<T>>> QueryAllAsync<T>(
        Func<AuthDbContext, HospitalRegistry.HospitalEntry, Task<T>> query)
    {
        var tasks = HospitalRegistry.All.Select(async hospital =>
        {
            try
            {
                using var cts = new CancellationTokenSource(HospitalQueryTimeout);
                await using var ctx = CreateHisContext(hospital.HisCs);

                var queryTask = query(ctx, hospital);
                var completed = await Task.WhenAny(queryTask, Task.Delay(Timeout.Infinite, cts.Token));

                if (completed != queryTask)
                    throw new TimeoutException($"Hospital {hospital.Key} query exceeded {HospitalQueryTimeout.TotalSeconds}s hard timeout.");

                var result = await queryTask;
                return new HospitalResult<T>(hospital.Key, hospital.Name, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QueryAllAsync failed for hospital {Key} ({Name})", hospital.Key, hospital.Name);
                return new HospitalResult<T>(hospital.Key, hospital.Name, default!);
            }
        });

        var results = await Task.WhenAll(tasks);
        return results.Where(r => r.Result is not null).ToList();
    }
}