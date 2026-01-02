using Ardalis.Specification;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Subjects.Specifications;

public class GetSubjectByIdWithPrerequisitesSpec : Specification<Subject>
{
    public GetSubjectByIdWithPrerequisitesSpec(SubjectId subjectId) 
        => Query.Where(s => s.Id == subjectId)
             .Include(s => s.Prerequisites);
}
