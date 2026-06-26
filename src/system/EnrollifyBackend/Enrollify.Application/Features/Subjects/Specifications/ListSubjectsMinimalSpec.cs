namespace Enrollify.Application.Features.Subjects.Specifications;

public class ListSubjectsMinimalSpec : Specification<Subject>
{
    public ListSubjectsMinimalSpec() =>
        Query.AsNoTracking();
}
