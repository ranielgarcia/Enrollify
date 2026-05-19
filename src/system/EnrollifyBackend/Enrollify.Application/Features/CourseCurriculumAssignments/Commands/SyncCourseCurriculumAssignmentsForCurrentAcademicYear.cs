using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.Queries;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.CourseCurriculumAssignments.Commands;

public static class SyncCourseCurriculumAssignmentsForCurrentAcademicYear
{
    public record Command() : ICommand<Result>;

    public class Handler : ICommandHandler<Command, Result>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<Handler> _logger;

        public Handler(IMediator mediator, ILogger<Handler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async ValueTask<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var currentAcademicYear = await _mediator.Send(new GetCurrentAcademicYearQuery(), cancellationToken);

            if (currentAcademicYear == null)
            {
                // TODO: Create new database table that will contain these warnings and show them in the admin dashboard.
                // This will allow us to track these warnings and take necessary actions to resolve them.
                _logger.LogWarning("No current academic year found. Skipping synchronization");
                return Result.Error(""); ;
            }

            await _mediator.Send(new SyncCourseCurriculumAssignmentsForAcademicYear.Command(currentAcademicYear.Value.Id), cancellationToken);

            return Result.Success();
        }
    }
}
