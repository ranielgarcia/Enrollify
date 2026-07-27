using Enrollify.Application.Features.AcademicYearAndTerm.Events;
using Enrollify.Application.Features.CourseCurriculumAssignments.Commands;

namespace Enrollify.Application.Features.CourseCurriculumAssignments.OnAcademicYearCreated;

public class OnAcademicYearCreatedEventSyncCourseCurriculumAssignmentsHandler (IMediator mediator) : INotificationHandler<AcademicYearCreatedEvent>
{
    public async Task Handle(AcademicYearCreatedEvent notification, CancellationToken cancellationToken)
    {
        await mediator.Send(new SyncCourseCurriculumAssignmentsForAcademicYear.Command(notification.AcademicYear.Id), cancellationToken);
    }
}
