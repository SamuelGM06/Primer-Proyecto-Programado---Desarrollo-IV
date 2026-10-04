namespace TodoApi.Services;

public class OverdueBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OverdueBackgroundService> _logger;

    public OverdueBackgroundService(IServiceScopeFactory scopeFactory,
                                    ILogger<OverdueBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Intervalo de revisión. Para probar rápido, usa FromSeconds(10).
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<OverdueTaskService>();
                var count = await service.NotifyOverdueAsync();
                _logger.LogInformation("Background check done. Notified: {Count}", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background check failed");
            }
        }
    }
}