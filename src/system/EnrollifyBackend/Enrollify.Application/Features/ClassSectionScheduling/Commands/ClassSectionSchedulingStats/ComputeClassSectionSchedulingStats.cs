using Enrollify.Core.Aggregates.UserAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionSchedulingStats;

public static class ComputeClassSectionSchedulingStats
{
  public sealed record Command(AcademicTermId TermId,
    CourseId CourseId, ClassSectionId? ClassSectionId = null, UserId? TriggeredBy = null) : IRequest;

  public sealed class Handler (
    IClassSectionSchedulingStatsRepository statsRepository,
    IReadRepository<ClassSection> classSectionReadRepository,
    IClassSectionRepository classSectionRepository) : IRequestHandler<Command>
  {
    public async Task Handle(Command request, CancellationToken cancellationToken)
    {
      Task refreshDraftSectionCountsForCourseTask = statsRepository.RefreshDraftSectionCountsForCourse(
        request.TermId,
        request.CourseId, cancellationToken);

      Task refreshOpenSectionCountsForCourseTask = statsRepository.RefreshOpenSectionCountsForCourse(request.TermId,
        request.CourseId, cancellationToken);

      Task refreshCancelledSectionCountsForCourseTask = statsRepository.RefreshCancelledSectionCountsForCourse(
        request.TermId,
        request.CourseId, cancellationToken);

      await Task.WhenAll(
        refreshDraftSectionCountsForCourseTask,
        refreshOpenSectionCountsForCourseTask,
        refreshCancelledSectionCountsForCourseTask);

      if (request.ClassSectionId is not null)
      {
        Task refreshOfferingsCountForClassSectionTask =
          statsRepository.RefreshOfferingsCountForClassSection((ClassSectionId)request.ClassSectionId,
            cancellationToken);

        Task refreshScheduleConflictIssueCountsForClassSectionTask =
          statsRepository.RefreshScheduleConflictIssueCountsForClassSection(request.TermId,
            request.CourseId, (ClassSectionId)request.ClassSectionId, cancellationToken);

        Task refreshSchedulePolicyViolationIssueCountsForClassSectionTask =
          statsRepository.RefreshSchedulePolicyViolationIssueCountsForClassSection(request.TermId,
            request.CourseId, (ClassSectionId)request.ClassSectionId, cancellationToken);

        Task refreshCapacityConstraintIssueCountsForClassSectionTask =
          statsRepository.RefreshCapacityConstraintIssueCountsForClassSection(request.TermId,
            request.CourseId, (ClassSectionId)request.ClassSectionId, cancellationToken);

        Task refreshResourceMisalignmentIssueCountsForClassSectionTask =
          statsRepository.RefreshResourceMisalignmentIssueCountsForClassSection(request.TermId,
            request.CourseId, (ClassSectionId)request.ClassSectionId, cancellationToken);

        Task refreshMissingRequirementIssueCountsForClassSectionTask =
          statsRepository.RefreshMissingRequirementIssueCountsForClassSection(request.TermId,
            request.CourseId, (ClassSectionId)request.ClassSectionId, cancellationToken);

        Task refreshDataInconsistencyIssueCountsForClassSectionTask =
          statsRepository.RefreshDataInconsistencyIssueCountsForClassSection(request.TermId,
            request.CourseId, (ClassSectionId)request.ClassSectionId, cancellationToken);

        Task refreshDefaultValueIssueCountsForClassSectionTask =
          statsRepository.RefreshDefaultValueIssueCountsForClassSection(request.TermId,
            request.CourseId, (ClassSectionId)request.ClassSectionId, cancellationToken);

        Task refreshOfferingCountWithIssueForClassSectionTask =
          statsRepository.RefreshOfferingCountWithIssueForClassSection(request.TermId,
            request.CourseId, (ClassSectionId)request.ClassSectionId, cancellationToken);

        Task refreshTotalValidationIssuesCountAcrossOfferingsForClassSectionTask =
          statsRepository.RefreshTotalValidationIssuesCountAcrossOfferingsForClassSection(request.TermId,
            request.CourseId, (ClassSectionId)request.ClassSectionId, cancellationToken);

        Task refreshOfferingCountWithMissingTeacherIssueForClassSectionTask =
          statsRepository.RefreshOfferingCountWithMissingTeacherIssueForClassSection(request.TermId,
            request.CourseId, (ClassSectionId)request.ClassSectionId, cancellationToken);

        Task refreshOfferingCountWithMissingRoomIssueForClassSectionTask =
          statsRepository.RefreshOfferingCountWithMissingRoomIssueForClassSection(request.TermId,
            request.CourseId, (ClassSectionId)request.ClassSectionId, cancellationToken);

        Task refreshOfferingCountWithNoScheduleIssueForClassSectionTask =
          statsRepository.RefreshOfferingCountWithNoScheduleIssueForClassSection(request.TermId,
            request.CourseId, (ClassSectionId)request.ClassSectionId, cancellationToken);

        await Task.WhenAll(
          refreshOfferingsCountForClassSectionTask,
          refreshScheduleConflictIssueCountsForClassSectionTask,
          refreshSchedulePolicyViolationIssueCountsForClassSectionTask,
          refreshCapacityConstraintIssueCountsForClassSectionTask,
          refreshResourceMisalignmentIssueCountsForClassSectionTask,
          refreshMissingRequirementIssueCountsForClassSectionTask,
          refreshDataInconsistencyIssueCountsForClassSectionTask,
          refreshDefaultValueIssueCountsForClassSectionTask,
          refreshOfferingCountWithIssueForClassSectionTask,
          refreshTotalValidationIssuesCountAcrossOfferingsForClassSectionTask,
          refreshOfferingCountWithMissingTeacherIssueForClassSectionTask,
          refreshOfferingCountWithMissingRoomIssueForClassSectionTask,
          refreshOfferingCountWithNoScheduleIssueForClassSectionTask);

        ClassSection? section =
          await classSectionReadRepository.FirstOrDefaultAsync(
            new GetClassSectionFullDetailsByIdSpec((ClassSectionId)request.ClassSectionId),
            cancellationToken);

        if (section is null)
          return;

        // Draft only, higher status will not invoke this event handler in any way. Updates to a class with higher status is not allowed
        section.MoveToDraft(request.TriggeredBy);
        await classSectionRepository.Update(section, cancellationToken);

      }
    }
  }
}
