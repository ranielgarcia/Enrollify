using Enrollify.Application.Features.Subjects.Specifications;
using Enrollify.Application.Features.Teachers.DTOs;
using Enrollify.Application.Features.Teachers.Specifications;
using Enrollify.Application.Filtering;

namespace Enrollify.Application.Features.Teachers.Queries;

public record FilterTeachersPaginatedQuery(
    int page = 1,
    int pageSize = 10,
    IEnumerable<FilterItem>? filters = null,
    IEnumerable<SortItem>? sorts = null,
    string? joinOperator = null
    ) : IRequest<Result<PagedResult<TeacherDto>>>;

public class FilterTeachersPaginatedQueryHandler : IRequestHandler<FilterTeachersPaginatedQuery, Result<PagedResult<TeacherDto>>>
{
    private readonly IReadRepository<Teacher> _readRepository;
    private readonly IReadRepository<Subject> _subjectReadRepository;

    public FilterTeachersPaginatedQueryHandler(IReadRepository<Teacher> readRepository, IReadRepository<Subject> subjectReadRepository)
    {
        _readRepository = readRepository;
        _subjectReadRepository = subjectReadRepository;
    }
    public async Task<Result<PagedResult<TeacherDto>>> Handle(FilterTeachersPaginatedQuery request, CancellationToken cancellationToken)
    {
        JoinOperator joinOperator = Enum.TryParse<JoinOperator>(request.joinOperator, ignoreCase: true, out var parsed)
                ? parsed : JoinOperator.Or;

        var spec = new FilterTeachersPaginatedSpec(request.page, request.pageSize, request.filters, request.sorts, joinOperator);
        List<Teacher> teachers = await _readRepository.ListAsync(spec, cancellationToken) ?? [];
        var totalCount = await _readRepository.CountAsync(spec, cancellationToken);

        List<SubjectId> allSubjectIds = teachers
            .SelectMany(t => t.Subjects)
            .Select(s => s.SubjectId)
            .Distinct()
            .ToList();
        List<Subject> allSubjects = [];
        if (allSubjectIds.Count > 0)
        {
            var getSubjectsSpec = new ListSubjectsByIdsSpec(allSubjectIds);
            allSubjects = await _subjectReadRepository.ListAsync(getSubjectsSpec, cancellationToken) ?? [];
        }
        var subjectLookup = allSubjects.ToDictionary(s => s.Id);

        var items = teachers
            .Select(t => TeacherDto.FromEntity(
                t,
                t.Subjects
                    .Where(ts => subjectLookup.ContainsKey(ts.SubjectId))
                    .Select(ts => subjectLookup[ts.SubjectId])
                    .ToList()))
            .ToList();

        return new PagedResult<TeacherDto>(
            items.AsReadOnly(),
            request.page,
            request.pageSize,
            totalCount);
    }
}
