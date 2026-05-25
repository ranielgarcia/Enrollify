using Ardalis.Result;
using Enrollify.Application.Features.Teachers.DTOs;
using Enrollify.Application.Features.Teachers.Specifications;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.Teachers.Queries;

public record SearchTeachersPaginatedQuery(string? searchTerm, int page = 1, int pageSize = 10) : IRequest<Result<PagedResult<TeacherDto>>>;

public class SearchTeachersPaginatedQueryHandler : IRequestHandler<SearchTeachersPaginatedQuery, Result<PagedResult<TeacherDto>>>
{
    private readonly IReadRepository<Teacher> _readRepository;

    public SearchTeachersPaginatedQueryHandler(IReadRepository<Teacher> readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<Result<PagedResult<TeacherDto>>> Handle(SearchTeachersPaginatedQuery request, CancellationToken cancellationToken)
    {
        var spec = new SearchTeachersPaginatedSpec(request.page, request.pageSize, request.searchTerm);
        var teachers = await _readRepository.ListAsync(spec, cancellationToken);
        // https://specification.ardalis.com/usage/use-built-in-abstract-repository.html#countasync-And-anyasync-methods
        // CountAsync And AnyAsync methods
        var totalCount = await _readRepository.CountAsync(spec, cancellationToken);

        var items = teachers
            .Select(t => TeacherDto.FromEntity(t))
            .ToList();

        return new PagedResult<TeacherDto>(items.AsReadOnly(), request.page, request.pageSize, totalCount);
    }
}
