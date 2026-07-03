namespace Enrollify.Application.Features.Subjects.Specifications;

public class ListSubjectsByIdsSpec : Specification<Subject>
{
    public ListSubjectsByIdsSpec(List<SubjectId> ids)
    {
        Query
            .Where(s => ids.Contains(s.Id));
    }
}
