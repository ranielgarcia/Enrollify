using Enrollify.Application.Features.Subjects.Models;

namespace Enrollify.Application.Features.Subjects.Specifications;

public class ListMinimumSubjectsByCodesSpec : Specification<Subject, MinimumSubjectProjection>
{
    public ListMinimumSubjectsByCodesSpec(List<SubjectCode> codes)
    {
        Query
            .Where(s => codes.Contains(s.Code))
            .Select(s => new MinimumSubjectProjection
            {
                Id = s.Id,
                Code = s.Code
            });
    }
}
