using Ardalis.Result;
using Enrollify.Application.Features.Subjects.DTOs;
using Enrollify.Application.Features.Subjects.Specifications;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Subjects.Queries;

public class ListSubjectsMinimalQuery : IQuery<Result<List<SubjectDTO>>>
{
}

public class ListSubjectsMinimalQueryHandler : IQueryHandler<ListSubjectsMinimalQuery, Result<List<SubjectDTO>>>
{
    private readonly IReadRepository<Subject> _readRepository;
    public ListSubjectsMinimalQueryHandler(IReadRepository<Subject> readRepository)
    {
        _readRepository = readRepository;
    }
    public async ValueTask<Result<List<SubjectDTO>>> Handle(ListSubjectsMinimalQuery request, CancellationToken cancellationToken)
    {
        var subjects = await _readRepository.ListAsync(new ListSubjectsMinimalSpec(), cancellationToken);
        var items = subjects
            .Select(SubjectDTO.FromEntity)
            .ToList();
        return Result.Success(items);
    }
}
