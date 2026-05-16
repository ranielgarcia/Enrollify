using Ardalis.Specification;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Application.Features.Curriculums.Specifications;

public class BulkGetCurriculumWithSubjectsByIdsSpec : Specification<Curriculum>
{
    public BulkGetCurriculumWithSubjectsByIdsSpec(List<CurriculumId> curriculumIds)
    {
        Query
            .Include(c => c.CurriculumSubjects.Where(cs => cs.IsActive))
            .Where(c => curriculumIds.Contains(c.Id));
    }
}
