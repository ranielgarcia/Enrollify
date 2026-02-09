using Ardalis.Specification;
using Enrollify.Core.Aggregates.SubjectAggregate;

namespace Enrollify.Application.Subjects.Specifications;

public class SearchSubjectsCountSpec : Specification<Subject>
{
    public SearchSubjectsCountSpec(string searchTerm) =>
        Query
            .AsNoTracking()
            .Where(s => s.Code == SubjectCode.From(searchTerm) || s.Title.Contains(searchTerm) || s.Description.Contains(searchTerm));
}