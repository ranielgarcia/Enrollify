using Enrollify.Application.Features.AcademicYearAndTerm.Events;
using Enrollify.Application.Features.CourseCurriculumAssignments.Commands;
using MediatR;

namespace Enrollify.Application.Features.AcademicYearAndTerm.EventHandlers;

public class AcademicYearCreatedEventSyncCourseCurriculumAssignmentsHandler (IMediator mediator) : INotificationHandler<AcademicYearCreatedEvent>
{
    public async Task Handle(AcademicYearCreatedEvent notification, CancellationToken cancellationToken)
    {
        await mediator.Send(new SyncCourseCurriculumAssignmentsForAcademicYear.Command(notification.AcademicYear.Id), cancellationToken);
    }
}
