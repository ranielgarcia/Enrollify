using Ardalis.Result;
using Enrollify.Application.Features.Subjects.DTOs;
using Enrollify.Application.Features.Subjects.Specifications;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Subjects.Features;

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
        // https://specification.ardalis.com/usage/use-built-in-abstract-repository.html#countasync-And-anyasync-methods
        // CountAsync And AnyAsync methods
        var totalCount = await _readRepository.CountAsync(spec, cancellationToken);

        var items = subjects
            .Select(SubjectDto.FromEntity)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.pageSize);
        return new PagedResult<SubjectDto>(items.AsReadOnly(), request.page, request.pageSize, totalCount, totalPages);
    }
}
