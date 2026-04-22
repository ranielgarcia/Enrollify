using Ardalis.Result;
using Enrollify.Application.Features.Subjects.DTOs;
using Enrollify.Application.Features.Subjects.Specifications;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.Subjects.Queries;

public record SearchSubjectsPaginatedQuery(string? searchTerm, int page = 1, int pageSize = 10) : IQuery<Result<PagedResult<SubjectDto>>>;

public class SearchSubjectsPaginatedQueryHandler : IQueryHandler<SearchSubjectsPaginatedQuery, Result<PagedResult<SubjectDto>>>
{
    private readonly IReadRepository<Subject> _readRepository;

    public SearchSubjectsPaginatedQueryHandler(IReadRepository<Subject> readRepository)
    {
        _readRepository = readRepository;
    }

    public async ValueTask<Result<PagedResult<SubjectDto>>> Handle(SearchSubjectsPaginatedQuery request, CancellationToken cancellationToken)
    {
        var spec = new SearchSubjectsPaginatedSpec(request.page, request.pageSize, request.searchTerm);
        var subjects = await _readRepository.ListAsync(spec, cancellationToken);
        // https://specification.ardalis.com/usage/use-built-in-abstract-repository.html#countasync-and-anyasync-methods
        // CountAsync and AnyAsync methods
        // The ISpecificationEvaluator.GetQuery() method accepts an additional optional bool evaluateCriteriaOnly = false
        // parameter.If set to true it will ignore the Take, Skip, OrderBy and Include conditions.
        // For paginated results, commonly we’d like to retrieve items in a given page, but also the total number of items.
        // The CountAsync repository method evaluates the specification in this special mode,
        // therefore we can reuse the same specification and avoid having unnecessary duplicates(one with pagination and another one without it).
        var totalCount = await _readRepository.CountAsync(spec, cancellationToken);

        var items = subjects
            .Select(SubjectDto.FromEntity)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.pageSize);
        return new PagedResult<SubjectDto>(items.AsReadOnly(), request.page, request.pageSize, totalCount, totalPages);
    }
}

