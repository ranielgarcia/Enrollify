using Ardalis.Specification;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Application.Features.Curriculums.Specifications;

public class BulkGetCurriculumsByIdsSpec : Specification<Curriculum>
{
    public BulkGetCurriculumsByIdsSpec(List<CurriculumId> curriculumIds)
    {
        var distinctIds = curriculumIds.Distinct().ToList();
        Query
            .AsNoTracking()
            .Where(c => distinctIds.Contains(c.Id));
    }
}
