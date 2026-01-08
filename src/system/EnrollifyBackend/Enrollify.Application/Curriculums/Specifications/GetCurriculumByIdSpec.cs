using Ardalis.Specification;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Application.Curriculums.Specifications;

public class GetCurriculumByIdSpec : Specification<Curriculum>
{
    public GetCurriculumByIdSpec(CurriculumId id) =>
        Query.Include(c => c.Course).Where(c => c.Id == id);
}
