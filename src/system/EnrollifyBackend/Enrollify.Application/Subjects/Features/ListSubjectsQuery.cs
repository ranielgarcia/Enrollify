using Ardalis.Result;
using Enrollify.Application.Subjects.DTOs;
using Enrollify.Application.Subjects.Specifications;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Subjects.Features;

public class ListSubjectsQuery : IQuery<Result<PagedResult<SubjectDTO>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ListSubjectsQueryHandler : IQueryHandler<ListSubjectsQuery, Result<PagedResult<SubjectDTO>>>
{
    private readonly IReadRepository<Subject> _readRepository;

    public ListSubjectsQueryHandler(IReadRepository<Subject> readRepository)
    {
        _readRepository = readRepository;
    }

        public async ValueTask<Result<PagedResult<SubjectDTO>>> Handle(ListSubjectsQuery request, CancellationToken cancellationToken)
        {
            var spec = new ListSubjectsSpec(request.Page, request.PageSize);
            var subjects = await _readRepository.ListAsync(spec, cancellationToken);
            var totalCount = await _readRepository.CountAsync(cancellationToken);

            var items = subjects
                .Select(SubjectDTO.FromEntity)
                .ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            return new PagedResult<SubjectDTO>(items.AsReadOnly(), request.Page, request.PageSize, totalCount, totalPages);
        }
    }