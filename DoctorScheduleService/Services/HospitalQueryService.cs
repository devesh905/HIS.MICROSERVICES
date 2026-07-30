using DoctorScheduleService.Data;
using Microsoft.EntityFrameworkCore;

namespace DoctorScheduleService.Services;

public class HospitalQueryService : IHospitalQueryService
{
    private readonly IConfiguration _config;
    private readonly ILogger<HospitalQueryService> _logger;

    // Hard cap — no single hospital query is ever allowed to exceed this,
    // regardless of what the connection string's timeout claims.
    private static readonly TimeSpan HospitalQueryTimeout = TimeSpan.FromSeconds(6);

    public HospitalQueryService(IConfiguration config, ILogger<HospitalQueryService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public HisDbContext CreateHisContext(string csName)
    {
        var cs = _config.GetConnectionString(csName)
                 ?? throw new InvalidOperationException($"Connection string '{csName}' not found.");
        var opts = new DbContextOptionsBuilder<HisDbContext>()
            .UseSqlServer(cs)
            .Options;
        return new HisDbContext(opts);
    }

    public LisDbContext CreateLisContext(string csName)
    {
        var cs = _config.GetConnectionString(csName)
                 ?? throw new InvalidOperationException($"Connection string '{csName}' not found.");
        var opts = new DbContextOptionsBuilder<LisDbContext>()
            .UseSqlServer(cs)
            .Options;
        return new LisDbContext(opts);
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

    public async Task<List<HospitalResult<T>>> QueryAllAsync<T>(
        Func<HisDbContext, Task<T>> query)
    {
        return await QueryAllAsync((ctx, _) => query(ctx));
    }

    public async Task<List<HospitalResult<T>>> QueryAllAsync<T>(
        Func<HisDbContext, HospitalRegistry.HospitalEntry, Task<T>> query)
    {
        var tasks = HospitalRegistry.All.Select(async hospital =>
        {
            try
            {
                using var cts = new CancellationTokenSource(HospitalQueryTimeout);
                await using var ctx = CreateHisContext(hospital.HisCs);

                // Race the actual query against the hard timeout.
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
                    "QueryAllAsync failed for hospital {Key} ({Name})", hospital.Key, hospital.Name);
                return new HospitalResult<T>(hospital.Key, hospital.Name, default!);
            }
        });

        var results = await Task.WhenAll(tasks);
        return results
            .Where(r => r.Result is not null)
            .ToList();
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