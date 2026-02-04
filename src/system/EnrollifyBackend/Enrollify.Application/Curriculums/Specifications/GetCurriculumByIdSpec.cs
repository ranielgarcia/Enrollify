using Ardalis.Specification;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Application.Curriculums.Specifications;

public class GetCurriculumByIdSpec : Specification<Curriculum>
{
    public GetCurriculumByIdSpec(CurriculumId id) =>
        Query
        .Include(c => c.CurriculumSubjects).ThenInclude(cs => cs.Prerequisites)
        .Include(c => c.Course)
        .Include(c => c.CreatedByUser)
        .Include(c => c.UpdatedByUser)
        .Where(c => c.Id == id);
}
