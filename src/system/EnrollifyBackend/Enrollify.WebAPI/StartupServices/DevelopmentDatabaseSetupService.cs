using Enrollify.Application.Features.CourseCurriculumAssignments.Commands;
using Enrollify.DatabaseMigration;

namespace Enrollify.WebAPI.StartupServices;

public class DevelopmentDatabaseSetupService(
    IConfiguration configuration,
    IMediator mediator,
    ILogger<DevelopmentDatabaseSetupService> logger) : IStartupService
{
    public async Task Initialize(CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        logger.LogInformation("Running database migration for development environment...");

        await Task.Run(() => DatabaseUpgrader.RunDbUpgradeActivities(
            connectionString,
            forceEnsureDatabase: true,
            createMockData: true,
            createSeedData: true), cancellationToken);

        logger.LogInformation("Database migration complete. Running course-curriculum assignment sync...");

        await mediator.Send(new SyncCourseCurriculumAssignmentsForCurrentAndFutureAcademicYears.Command(IncludePastYears: true), cancellationToken);

        logger.LogInformation("Development database setup complete.");
    }
}
