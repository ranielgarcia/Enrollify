using Enrollify.Application.Features.CourseCurriculumAssignments.Commands;
using Enrollify.Application.Features.Curriculums.Events;
using MediatR;

namespace Enrollify.Application.Features.Curriculums.EventHandlers;

public class CurriculumApprovedEventSyncCourseCurriculumAssignmentsHandler(IMediator mediator) : INotificationHandler<CurriculumApprovedEvent>
{
    public async Task Handle(CurriculumApprovedEvent notification, CancellationToken cancellationToken)
    {
        await mediator.Send(new SyncCourseCurriculumAssignmentsForCurrentAcademicYear.Command(), cancellationToken);
    }
}
