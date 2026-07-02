namespace Enrollify.Application.Features.ClassSectionScheduling.Repositories;

public interface IClassSectionSchedulingStatsRepository
{
  Task RefreshDraftSectionCountsForCourse(AcademicTermId termId, CourseId courseId,
    CancellationToken cancellationToken);

  Task RefreshOpenSectionCountsForCourse(AcademicTermId termId, CourseId courseId,
    CancellationToken cancellationToken);

  Task RefreshCancelledSectionCountsForCourse(AcademicTermId termId, CourseId courseId,
    CancellationToken cancellationToken);

  Task RefreshScheduleConflictIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshSchedulePolicyViolationIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshCapacityConstraintIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshResourceMisalignmentIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshMissingRequirementIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshDataInconsistencyIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshDefaultValueIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshOfferingCountWithIssueForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshTotalValidationIssuesCountAcrossOfferingsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId,
    CancellationToken ct);

  Task RefreshOfferingCountWithMissingTeacherIssueForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshOfferingCountWithMissingRoomIssueForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshOfferingCountWithNoScheduleIssueForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshOfferingsCountForClassSection(ClassSectionId classSectionId,
    CancellationToken ct);
}
