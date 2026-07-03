using Enrollify.Application.Features.Subjects.DTOs;
using Enrollify.Application.Features.Subjects.Specifications;

namespace Enrollify.Application.Features.Subjects.Queries;

public class ListSubjectsMinimalQuery : IRequest<Result<List<SubjectDto>>>
{
}

public class ListSubjectsMinimalQueryHandler : IRequestHandler<ListSubjectsMinimalQuery, Result<List<SubjectDto>>>
{
    private readonly IReadRepository<Subject> _readRepository;
    public ListSubjectsMinimalQueryHandler(IReadRepository<Subject> readRepository)
    {
        _readRepository = readRepository;
    }
    public async Task<Result<List<SubjectDto>>> Handle(ListSubjectsMinimalQuery request, CancellationToken cancellationToken)
    {
        var subjects = await _readRepository.ListAsync(new ListSubjectsMinimalSpec(), cancellationToken);
        var items = subjects
            .Select(SubjectDto.FromEntity)
            .ToList();
        return Result.Success(items);
    }
}
