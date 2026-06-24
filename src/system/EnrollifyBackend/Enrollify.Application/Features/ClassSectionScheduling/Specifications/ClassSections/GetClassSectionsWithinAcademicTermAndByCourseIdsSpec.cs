using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSections;

public class GetClassSectionsWithinAcademicTermAndByCourseIdsSpec : Specification<ClassSection>
{
  public GetClassSectionsWithinAcademicTermAndByCourseIdsSpec(AcademicTermId academicTermId, List<CourseId> courseIds)
  {
    Query
      .AsNoTracking()
      .AsSplitQuery()
      .Include(cs => cs.Adviser)
      .Where(x => courseIds.Contains(x.CourseId) && x.AcademicTermId == academicTermId)
      .OrderBy(cs => cs.Name).ThenBy(cs => (char)cs.SectionCode);
  }
}
