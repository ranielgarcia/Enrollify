using Ardalis.Result;
using Enrollify.Application.Features.Subjects.DTOs;
using Enrollify.Application.Features.Subjects.Specifications;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Subjects.Queries;

public record ListSubjectsPaginatedQuery(int page = 1, int pageSize = 10) : IQuery<Result<PagedResult<SubjectDto>>>;

public class ListSubjectsPaginatedQueryHandler : IQueryHandler<ListSubjectsPaginatedQuery, Result<PagedResult<SubjectDto>>>
{
    private readonly IReadRepository<Subject> _readRepository;

    public ListSubjectsPaginatedQueryHandler(IReadRepository<Subject> readRepository)
    {
        _readRepository = readRepository;
    }

    public async ValueTask<Result<PagedResult<SubjectDto>>> Handle(ListSubjectsPaginatedQuery request, CancellationToken cancellationToken)
    {
        var spec = new ListSubjectsPaginatedSpec(request.page, request.pageSize);
        var subjects = await _readRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _readRepository.CountAsync(cancellationToken);

        var items = subjects
            .Select(SubjectDto.FromEntity)
            .ToList();

        return new PagedResult<SubjectDto>(items.AsReadOnly(), request.page, request.pageSize, totalCount);
    }
}
