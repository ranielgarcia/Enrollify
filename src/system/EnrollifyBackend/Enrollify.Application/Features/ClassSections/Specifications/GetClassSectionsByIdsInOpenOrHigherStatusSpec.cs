using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Application.Features.ClassSections.Specifications;

public class GetClassSectionsByIdsInOpenOrHigherStatusSpec : Specification<ClassSection>
{
    public GetClassSectionsByIdsInOpenOrHigherStatusSpec(IEnumerable<ClassSectionId> ids)
    {
        Query.Where(cs => ids.Contains(cs.Id)
            && cs.StatusId != ClassSectionStatusEnum.Draft
            && cs.StatusId != ClassSectionStatusEnum.Cancelled);
    }
}
