namespace Enrollify.Application.Features.Curriculums.Specifications;

public class GetCurriculumsWithSubjectsByIdsSpec : Specification<Curriculum>
{
    public GetCurriculumsWithSubjectsByIdsSpec(List<CurriculumId> ids) =>
        Query
            .Include(c => c.CurriculumSubjects.Where(cs => cs.IsActive)).ThenInclude(cs => cs.Prerequisites.Where(p => p.IsActive))
            .Where(c => ids.Contains(c.Id));
}
