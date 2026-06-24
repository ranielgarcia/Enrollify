using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSections;

public class GetClassSectionsByAcademicTermIdsSpec : Specification<ClassSection>
{
    public GetClassSectionsByAcademicTermIdsSpec(IEnumerable<AcademicTermId> termIds)
    {
        Query.Where(cs => termIds.Contains(cs.AcademicTermId));
    }
}
