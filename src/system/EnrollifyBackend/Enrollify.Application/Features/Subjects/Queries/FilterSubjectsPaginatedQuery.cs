using Enrollify.Application.Features.Subjects.DTOs;
using Enrollify.Application.Features.Subjects.Specifications;
using Enrollify.Application.Filtering;

namespace Enrollify.Application.Features.Subjects.Queries;

public record FilterSubjectsPaginatedQuery(
    int page = 1,
    int pageSize = 10,
    IEnumerable<FilterItem>? filters = null,
    IEnumerable<SortItem>? sorts = null,
    string? joinOperator = null
    ) : IRequest<Result<PagedResult<SubjectDto>>>;

public class FilterSubjectsPaginatedQueryHandler : IRequestHandler<FilterSubjectsPaginatedQuery, Result<PagedResult<SubjectDto>>>
{
    private readonly IReadRepository<Subject> _readRepository;

    public FilterSubjectsPaginatedQueryHandler(IReadRepository<Subject> readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<PagedResult<SubjectDto>>> Handle(FilterSubjectsPaginatedQuery request, CancellationToken cancellationToken)
    {
        JoinOperator joinOperator = Enum.TryParse<JoinOperator>(request.joinOperator, ignoreCase: true, out var parsed)
                ? parsed : JoinOperator.Or;

        var spec = new FilterSubjectsPaginatedSpec(request.page, request.pageSize, request.filters, request.sorts, joinOperator);
        var subjects = await _readRepository.ListAsync(spec, cancellationToken);
        // https://specification.ardalis.com/usage/use-built-in-abstract-repository.html#countasync-And-anyasync-methods
        // CountAsync And AnyAsync methods
        // The ISpecificationEvaluator.GetQuery() method accepts an additional optional bool evaluateCriteriaOnly = false
        // parameter.If set to true it will ignore the Take, Skip, OrderBy And Include conditions.
        // For paginated results, commonly we’d like to retrieve items in a given page, but also the total number of items.
        // The CountAsync repository method evaluates the specification in this special mode,
        // therefore we can reuse the same specification And avoid having unnecessary duplicates(one with pagination And another one without it).
        var totalCount = await _readRepository.CountAsync(spec, cancellationToken);

        var items = subjects
            .Select(SubjectDto.FromEntity)
            .ToList();

        return new PagedResult<SubjectDto>(items.AsReadOnly(), request.page, request.pageSize, totalCount);
    }
}

