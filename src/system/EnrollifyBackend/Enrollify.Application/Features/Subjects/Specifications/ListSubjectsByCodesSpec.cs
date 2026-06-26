namespace Enrollify.Application.Features.Subjects.Specifications;

public class ListSubjectsByCodesSpec : Specification<Subject>
{
    public ListSubjectsByCodesSpec(List<SubjectCode> codes)
    {
        Query
            .Where(s => codes.Contains(s.Code));
    }
}
