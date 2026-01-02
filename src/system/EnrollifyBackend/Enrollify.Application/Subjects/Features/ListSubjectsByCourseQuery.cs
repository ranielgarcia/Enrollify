using Ardalis.Result;
using Enrollify.Application.Subjects.DTOs;
using Enrollify.Application.Subjects.Specifications;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Subjects.Features;

public class ListSubjectsByCourseQuery : IQuery<Result<List<SubjectDTO>>>
{
}

public class ListSubjectsByCourseQueryHandler : IQueryHandler<ListSubjectsByCourseQuery, Result<List<SubjectDTO>>>
{
    private readonly IReadRepository<Subject> _readRepository;

    public ListSubjectsByCourseQueryHandler(IReadRepository<Subject> readRepository)
    {
        _readRepository = readRepository;
    }

    public async ValueTask<Result<List<SubjectDTO>>> Handle(ListSubjectsByCourseQuery request, CancellationToken cancellationToken)
    {
        var spec = new ListSubjectsSpec();
        var subjects = await _readRepository.ListAsync(spec, cancellationToken);

        var toReturn = subjects
            .Select(SubjectDTO.FromEntity)
            .ToList();
        return toReturn;
    }
}