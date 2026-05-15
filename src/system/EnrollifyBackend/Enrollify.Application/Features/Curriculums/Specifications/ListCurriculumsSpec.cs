using Ardalis.Specification;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Application.Features.Curriculums.Specifications;

public class ListCurriculumsSpec : Specification<Curriculum>
{
    public ListCurriculumsSpec() =>
        Query
        .Include(c => c.Course)
        .Include(c => c.CreatedByUser)
        .Include(c => c.UpdatedByUser);
}
