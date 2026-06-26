namespace Enrollify.Application.Features.Curriculums.Specifications;

public class GetCurriculumBySubjectIdSpec : Specification<Curriculum>
{
    public GetCurriculumBySubjectIdSpec(SubjectId subjectId)
        => Query.Include(c => c.CurriculumSubjects.Where(cs => cs.IsActive && cs.SubjectId == subjectId));
}
