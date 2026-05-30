using Ardalis.Specification;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Application.Features.Curriculums.Specifications;

public class GetCurriculumWithSubjectsByIdSpec : Specification<Curriculum>
{
    public GetCurriculumWithSubjectsByIdSpec(CurriculumId curriculumId)
    {
        Query
            .Include(c => c.CurriculumSubjects.Where(cs => cs.IsActive))
                .ThenInclude(cs => cs.Subject)
            .Where(c => c.Id == curriculumId);
    }
}
