using Ardalis.Result;
using Enrollify.Application.Features.Teachers.DTOs;
using Enrollify.Application.Features.Teachers.Specifications;
using Enrollify.Application.Filtering;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Teachers.Queries;

public record FilterTeachersPaginatedQuery(
    int page = 1,
    int pageSize = 10,
    IEnumerable<FilterItem>? filters = null,
    IEnumerable<SortItem>? sorts = null,
    string? joinOperator = null
    ) : IQuery<Result<PagedResult<TeacherDto>>>;

public class FilterTeachersPaginatedQueryHandler : IQueryHandler<FilterTeachersPaginatedQuery, Result<PagedResult<TeacherDto>>>
{
    private readonly IReadRepository<Teacher> _readRepository;
    public FilterTeachersPaginatedQueryHandler(IReadRepository<Teacher> readRepository)
    {
        _readRepository = readRepository;
    }
    public async ValueTask<Result<PagedResult<TeacherDto>>> Handle(FilterTeachersPaginatedQuery request, CancellationToken cancellationToken)
    {
        JoinOperator joinOperator = Enum.TryParse<JoinOperator>(request.joinOperator, ignoreCase: true, out var parsed)
                ? parsed : JoinOperator.Or;

        var spec = new FilterTeachersPaginatedSpec(request.page, request.pageSize, request.filters, request.sorts, joinOperator);
        var teachers = await _readRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _readRepository.CountAsync(spec, cancellationToken);
        var items = teachers
            .Select(TeacherDto.FromEntity)
            .ToList();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.pageSize);
        return new PagedResult<TeacherDto>(items.AsReadOnly(), request.page, request.pageSize, totalCount, totalPages);
    }
}
