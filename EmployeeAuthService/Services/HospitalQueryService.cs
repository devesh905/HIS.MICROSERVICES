using Microsoft.EntityFrameworkCore;
using EmployeeAuthService.Data;

namespace EmployeeAuthService.Services;

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

    public ViphaDbContext CreateViphaContext(string csName)
    {
        var cs = _config.GetConnectionString(csName)
                 ?? throw new InvalidOperationException($"Connection string '{csName}' not found.");
        var opts = new DbContextOptionsBuilder<ViphaDbContext>()
            .UseSqlServer(cs)
            .Options;
        return new ViphaDbContext(opts);
    }

    public async Task<List<HospitalResult<T>>> QueryAllViphaAsync<T>(
    Func<ViphaDbContext, HospitalRegistry.HospitalEntry, Task<T>> query)
    {
        var tasks = HospitalRegistry.All.Select(async hospital =>
        {
            try
            {
                using var cts = new CancellationTokenSource(HospitalQueryTimeout);
                await using var ctx = CreateViphaContext(hospital.ViphaCs);

                var queryTask = query(ctx, hospital);
                var completed = await Task.WhenAny(queryTask, Task.Delay(Timeout.Infinite, cts.Token));

                if (completed != queryTask)
                    throw new TimeoutException($"Hospital {hospital.Key} query exceeded {HospitalQueryTimeout.TotalSeconds}s hard timeout.");

                var result = await queryTask;
                return new HospitalResult<T>(hospital.Key, hospital.Name, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "QueryAllViphaAsync failed for hospital {Key} ({Name})", hospital.Key, hospital.Name);
                return new HospitalResult<T>(hospital.Key, hospital.Name, default!);
            }
        });

        var results = await Task.WhenAll(tasks);
        return results
            .Where(r => r.Result is not null)
            .ToList();
    }
}