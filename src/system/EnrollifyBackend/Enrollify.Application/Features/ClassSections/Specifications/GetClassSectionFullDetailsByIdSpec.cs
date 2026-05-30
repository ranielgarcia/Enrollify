using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.Application.Features.ClassSections.Specifications;

public class GetClassSectionFullDetailsByIdSpec : Specification<ClassSection>
{
  public GetClassSectionFullDetailsByIdSpec(ClassSectionId id)
  {
    Query
      .AsNoTracking()
      .AsSplitQuery()
      .Include(cs => cs.Course)
      .Include(cs => cs.Curriculum)
      .Include(cs => cs.Adviser)
      .Include(cs => cs.AcademicTerm)
      .Include(cs => cs.CohortAcademicYear)
      .Include(cs => cs.CreatedByUser)
      .Include(cs => cs.UpdatedByUser)
      .Where(cs => cs.Id == id);
  }
}
