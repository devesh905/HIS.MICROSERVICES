using HealthCampService.BackgroundJobs;

namespace HealthCampService.BackgroundJobs;

public class HealthCampProcessingHostedService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<HealthCampProcessingHostedService> _logger;

    // Start conservative — every 15 min. Tighten later if reception needs faster turnaround.
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    public HealthCampProcessingHostedService(
        IServiceProvider services,
        ILogger<HealthCampProcessingHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Small initial delay so the app finishes starting up before the first DB hit.
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<HealthCampProcessingJob>();
                await job.RunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Never let the hosted service die — log and try again next interval.
                _logger.LogError(ex, "HealthCampProcessingHostedService run failed");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // shutting down
            }
        }
    }
}