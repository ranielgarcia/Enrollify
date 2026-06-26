namespace Enrollify.Application.Features.Curriculums.Specifications;

public class BulkGetMinimumCurriculumsByIdsSpec : Specification<Curriculum>
{
    public BulkGetMinimumCurriculumsByIdsSpec(List<CurriculumId> curriculumIds)
    {
        var distinctIds = curriculumIds.Distinct().ToList();
        Query
            .AsNoTracking()
            .Where(c => distinctIds.Contains(c.Id));
    }
}
