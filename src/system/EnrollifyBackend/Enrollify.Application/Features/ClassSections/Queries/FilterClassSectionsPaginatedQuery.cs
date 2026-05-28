using Ardalis.Result;
using Enrollify.Application.Features.ClassSections.DTOs;
using Enrollify.Application.Features.ClassSections.Specifications;
using Enrollify.Application.Filtering;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.ClassSections.Queries;

public record FilterClassSectionsPaginatedQuery(
    int page = 1,
    int pageSize = 10,
    IEnumerable<int>? academicTermIds = null,
    IEnumerable<FilterItem>? filters = null,
    IEnumerable<SortItem>? sorts = null,
    string? joinOperator = null
) : IRequest<Result<PagedResult<ClassSectionDto>>>;

public class FilterClassSectionsPaginatedQueryHandler
    : IRequestHandler<FilterClassSectionsPaginatedQuery, Result<PagedResult<ClassSectionDto>>>
{
    private readonly IReadRepository<ClassSection> _readRepository;

    public FilterClassSectionsPaginatedQueryHandler(IReadRepository<ClassSection> readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<PagedResult<ClassSectionDto>>> Handle(
        FilterClassSectionsPaginatedQuery request,
        CancellationToken cancellationToken)
    {
        JoinOperator joinOperator = Enum.TryParse<JoinOperator>(request.joinOperator, ignoreCase: true, out var parsed)
            ? parsed : JoinOperator.Or;

        var spec = new FilterClassSectionsPaginatedSpec(
            request.page,
            request.pageSize,
            request.academicTermIds,
            request.filters,
            request.sorts,
            joinOperator);

        var sections = await _readRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _readRepository.CountAsync(spec, cancellationToken);

        var items = sections
            .Select(ClassSectionDto.FromEntity)
            .ToList();

        return new PagedResult<ClassSectionDto>(
            items.AsReadOnly(),
            request.page,
            request.pageSize,
            totalCount);
    }
}
