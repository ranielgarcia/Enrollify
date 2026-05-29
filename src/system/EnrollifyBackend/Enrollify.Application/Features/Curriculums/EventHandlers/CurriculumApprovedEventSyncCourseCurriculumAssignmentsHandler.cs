using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Application.Features.CourseCurriculumAssignments.Commands;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Application.Features.Curriculums.Events;
using Enrollify.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.Curriculums.EventHandlers;

public class CurriculumApprovedEventSyncCourseCurriculumAssignmentsHandler(
    IMediator mediator,
    IReadRepository<AcademicYear> academicYearReadRepository,
    ILogger<CurriculumApprovedEventSyncCourseCurriculumAssignmentsHandler> logger)
    : INotificationHandler<CurriculumApprovedEvent>
{
    public async Task Handle(CurriculumApprovedEvent notification, CancellationToken cancellationToken)
    {
        var curriculum = notification.Curriculum;
        AcademicYear? academicYear = await academicYearReadRepository.FirstOrDefaultAsync(
            new GetAcademicYearByStartDateYearSpec(curriculum.EffectiveYear), cancellationToken);

        if (academicYear is null)
        {
            logger.LogWarning(
                "No academic year found for curriculum effective year {EffectiveYear} (CurriculumId: {CurriculumId}). Skipping course-curriculum assignment sync.",
                curriculum.EffectiveYear, curriculum.Id);
            return;
        }

        await mediator.Send(new SyncCourseCurriculumAssignments.Command(academicYear.Id), cancellationToken);
    }
}
