using Ardalis.Specification;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Application.Curriculums.Specifications;

public class GetCurriculumWithSubjectsByIdSpec : Specification<Curriculum>
{
    public GetCurriculumWithSubjectsByIdSpec(CurriculumId id) =>
        Query
            .Include(c => c.CurriculumSubjects).ThenInclude(cs => cs.Prerequisites)
            .Where(c => c.Id == id);
}
