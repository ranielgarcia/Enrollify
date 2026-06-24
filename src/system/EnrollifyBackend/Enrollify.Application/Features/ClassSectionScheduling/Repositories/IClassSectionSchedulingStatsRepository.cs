using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Repositories;

public interface IClassSectionSchedulingStatsRepository
{
  Task RefreshDraftSectionCountsForCourse(AcademicTermId termId, CourseId courseId,
    CancellationToken cancellationToken);

  Task RefreshOpenSectionCountsForCourse(AcademicTermId termId, CourseId courseId,
    CancellationToken cancellationToken);

  Task RefreshCancelledSectionCountsForCourse(AcademicTermId termId, CourseId courseId,
    CancellationToken cancellationToken);

  Task RefreshHardConflictIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshSoftConflictIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshDataIntegrityIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId,
    CancellationToken cancellationToken);

  Task RefreshInformationalIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId,
    CancellationToken cancellationToken);

  Task RefreshOfferingCountWithIssueForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshOfferingCountWithMissingTeacherIssueForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshOfferingCountWithMissingRoomIssueForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshOfferingCountWithNoScheduleIssueForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken);

  Task RefreshOfferingsCountForClassSection(ClassSectionId classSectionId,
    CancellationToken ct);
}
