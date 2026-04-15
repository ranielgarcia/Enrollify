namespace Enrollify.WebAPI.StartupServices;

public class StartupRunner : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<StartupRunner> _logger;

    public StartupRunner(IServiceProvider services, ILogger<StartupRunner> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Beginning startup operations");
        using var scope = _services.CreateScope();

        var initializers = scope.ServiceProvider.GetServices<IStartupService>();
        await Task.WhenAll(initializers.Select(i => i.Initialize(stoppingToken)));
        _logger.LogInformation("Completed startup operations");
    }
}

