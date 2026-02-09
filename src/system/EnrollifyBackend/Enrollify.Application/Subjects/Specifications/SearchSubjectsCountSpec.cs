using Ardalis.Specification;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Microsoft.EntityFrameworkCore;

namespace Enrollify.Application.Subjects.Specifications;

public class SearchSubjectsCountSpec : Specification<Subject>
{
    public SearchSubjectsCountSpec(string searchTerm) =>
        Query
            .AsNoTracking()
            .Where(s => EF.Property<string>(s, nameof(Subject.Code)).Contains(searchTerm) || s.Title.Contains(searchTerm) || s.Description.Contains(searchTerm));
}