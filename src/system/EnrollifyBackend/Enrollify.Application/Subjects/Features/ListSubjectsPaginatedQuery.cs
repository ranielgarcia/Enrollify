using Ardalis.Result;
using Enrollify.Application.Subjects.DTOs;
using Enrollify.Application.Subjects.Specifications;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Subjects.Features;

public record ListSubjectsPaginatedQuery(int page = 1, int pageSize = 10) : IQuery<Result<PagedResult<SubjectDTO>>>;

public class ListSubjectsPaginatedQueryHandler : IQueryHandler<ListSubjectsPaginatedQuery, Result<PagedResult<SubjectDTO>>>
{
    private readonly IReadRepository<Subject> _readRepository;

    public ListSubjectsPaginatedQueryHandler(IReadRepository<Subject> readRepository)
    {
        _readRepository = readRepository;
    }

    public async ValueTask<Result<PagedResult<SubjectDTO>>> Handle(ListSubjectsPaginatedQuery request, CancellationToken cancellationToken)
    {
        var spec = new ListSubjectsPaginatedSpec(request.page, request.pageSize);
        var subjects = await _readRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _readRepository.CountAsync(cancellationToken);

        var items = subjects
            .Select(SubjectDTO.FromEntity)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.pageSize);
        return new PagedResult<SubjectDTO>(items.AsReadOnly(), request.page, request.pageSize, totalCount, totalPages);
    }
}