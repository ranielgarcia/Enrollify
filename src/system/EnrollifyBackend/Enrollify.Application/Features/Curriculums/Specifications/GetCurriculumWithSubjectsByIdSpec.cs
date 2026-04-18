using Ardalis.Specification;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Application.Features.Curriculums.Specifications;

public class GetCurriculumWithSubjectsByIdSpec : Specification<Curriculum>
{
    public GetCurriculumWithSubjectsByIdSpec(CurriculumId id) =>
        Query
            .Include(c => c.CurriculumSubjects.Where(cs => cs.IsActive)).ThenInclude(cs => cs.Prerequisites.Where(p => p.IsActive))
            .Where(c => c.Id == id);
}
