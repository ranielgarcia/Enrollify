using Enrollify.Application.Features.CourseCurriculumAssignments.Commands;
using Enrollify.Application.Features.Curriculums.Events;

namespace Enrollify.Application.Features.CourseCurriculumAssignments.OnCurriculumApproved;

public class OnCurriculumApprovedEventSyncCourseCurriculumAssignmentsHandler(
    IMediator mediator)
    : INotificationHandler<CurriculumApprovedEvent>
{
    public async Task Handle(CurriculumApprovedEvent notification, CancellationToken cancellationToken)
    {
        var curriculum = notification.Curriculum;
        await mediator.Send(new SyncCourseCurriculumAssignmentForCurriculum.Command(curriculum.Id), cancellationToken);
    }
}
