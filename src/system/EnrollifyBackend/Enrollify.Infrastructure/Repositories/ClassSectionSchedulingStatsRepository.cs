using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Infrastructure.Repositories;

public class ClassSectionSchedulingStatsRepository : IClassSectionSchedulingStatsRepository
{
  public async Task RefreshDraftSectionCountsForCourse(AcademicTermId termId, CourseId courseId,
    CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public async Task RefreshOpenSectionCountsForCourse(AcademicTermId termId, CourseId courseId,
    CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public async Task RefreshCancelledSectionCountsForCourse(AcademicTermId termId, CourseId courseId,
    CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public async Task RefreshHardConflictIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public async Task RefreshSoftConflictIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public async Task RefreshDataIntegrityIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public async Task RefreshInformationalIssueCountsForClassSection(AcademicTermId termId, CourseId courseId,
    ClassSectionId classSectionId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}
