using Enrollify.Application.Features.CourseCurriculumAssignments.Commands;

namespace Enrollify.WebAPI.BackgroundJobs;

public class SyncCourseCurriculumAssignmentsJob : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<SyncCourseCurriculumAssignmentsJob> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(10);

    public SyncCourseCurriculumAssignmentsJob(IServiceProvider services, ILogger<SyncCourseCurriculumAssignmentsJob> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{Job} started. Running every {Interval} minutes.", nameof(SyncCourseCurriculumAssignmentsJob), _interval.TotalMinutes);

        using var timer = new PeriodicTimer(_interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunAsync(stoppingToken);
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{Job} triggered at {Time}.", nameof(SyncCourseCurriculumAssignmentsJob), DateTimeOffset.UtcNow);

        try
        {
            using var scope = _services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var result = await mediator.Send(new SyncCourseCurriculumAssignmentsForCurrentAcademicYear.Command(), cancellationToken);

            if (result.IsSuccess)
                _logger.LogInformation("{Job} completed successfully.", nameof(SyncCourseCurriculumAssignmentsJob));
            else
                _logger.LogWarning("{Job} completed with errors: {Errors}", nameof(SyncCourseCurriculumAssignmentsJob), string.Join("; ", result.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Job} failed unexpectedly.", nameof(SyncCourseCurriculumAssignmentsJob));
        }
    }
}
