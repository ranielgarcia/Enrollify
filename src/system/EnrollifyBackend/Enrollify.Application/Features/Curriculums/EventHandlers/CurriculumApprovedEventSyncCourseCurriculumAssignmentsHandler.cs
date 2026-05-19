using Enrollify.Application.Features.CourseCurriculumAssignments.Commands;
using Enrollify.Application.Features.Curriculums.Events;
using Mediator;

namespace Enrollify.Application.Features.Curriculums.EventHandlers;

public class CurriculumApprovedEventSyncCourseCurriculumAssignmentsHandler(IMediator mediator) : INotificationHandler<CurriculumApprovedEvent>
{
    public async ValueTask Handle(CurriculumApprovedEvent notification, CancellationToken cancellationToken)
    {
        await mediator.Send(new SyncCourseCurriculumAssignmentsForCurrentAcademicYear.Command(), cancellationToken);
    }
}
