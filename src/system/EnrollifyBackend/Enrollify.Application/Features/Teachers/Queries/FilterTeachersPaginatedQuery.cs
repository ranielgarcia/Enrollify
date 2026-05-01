using Ardalis.Result;
using Enrollify.Application.Features.Subjects.Specifications;
using Enrollify.Application.Features.Teachers.DTOs;
using Enrollify.Application.Features.Teachers.Specifications;
using Enrollify.Application.Filtering;
using Enrollify.Core.Aggregates.SubjectAggregate;
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
    private readonly IReadRepository<Subject> _subjectReadRepository;

    public FilterTeachersPaginatedQueryHandler(IReadRepository<Teacher> readRepository, IReadRepository<Subject> subjectReadRepository)
    {
        _readRepository = readRepository;
        _subjectReadRepository = subjectReadRepository;
    }
    public async ValueTask<Result<PagedResult<TeacherDto>>> Handle(FilterTeachersPaginatedQuery request, CancellationToken cancellationToken)
    {
        JoinOperator joinOperator = Enum.TryParse<JoinOperator>(request.joinOperator, ignoreCase: true, out var parsed)
                ? parsed : JoinOperator.Or;

        var spec = new FilterTeachersPaginatedSpec(request.page, request.pageSize, request.filters, request.sorts, joinOperator);
        var teachers = await _readRepository.ListAsync(spec, cancellationToken);
        var totalCount = await _readRepository.CountAsync(spec, cancellationToken);

        var allSubjectIds = teachers?.SelectMany(t => t.Subjects).Select(s => s.SubjectId).Distinct().ToList();
        var getSubjectsSpec = new ListSubjectsByIdsSpec(allSubjectIds ?? new List<SubjectId>());
        var allSubjects = allSubjectIds?.Count > 0
                        ? await _subjectReadRepository.ListAsync(getSubjectsSpec, cancellationToken)
                        : [];
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
            items?.AsReadOnly() ?? Array.Empty<TeacherDto>().AsReadOnly(),
            request.page,
            request.pageSize,
            totalCount);
    }
}
