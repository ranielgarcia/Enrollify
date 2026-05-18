using Enrollify.Application.Features.AcademicYearAndTerm.Queries;
using Enrollify.Application.Features.CourseCurriculumAssignments.Commands;
using Enrollify.Application.Features.Courses.Events;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.Courses.EventHandlers;

public class CourseCreatedEventSyncCourseCurriculumAssignmentsHandler : INotificationHandler<CourseCreatedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<CourseCreatedEventSyncCourseCurriculumAssignmentsHandler> _logger;

    public CourseCreatedEventSyncCourseCurriculumAssignmentsHandler
        (IMediator mediator, ILogger<CourseCreatedEventSyncCourseCurriculumAssignmentsHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async ValueTask Handle(CourseCreatedEvent notification, CancellationToken cancellationToken)
    {
        var currentAcademicYear = await _mediator.Send(new GetCurrentAcademicYearQuery(), cancellationToken);

        if (currentAcademicYear == null)
        {
            // TODO: Create new database table that will contain these warnings and show them in the admin dashboard.
            // This will allow us to track these warnings and take necessary actions to resolve them.
            _logger.LogWarning("No current academic year found. Skipping synchronization of course curriculum assignments for course {CourseId}.", notification.CourseId);
            return;
        }

        await _mediator.Send(new SyncCourseCurriculumAssignmentsForAcademicYear.Command(currentAcademicYear.Value.Id), cancellationToken);
    }
}
