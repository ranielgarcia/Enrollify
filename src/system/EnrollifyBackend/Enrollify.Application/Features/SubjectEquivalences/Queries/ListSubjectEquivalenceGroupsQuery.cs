using Enrollify.Application.Features.SubjectEquivalences.DTOs;
using Enrollify.Application.Features.SubjectEquivalences.Specifications;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;

namespace Enrollify.Application.Features.SubjectEquivalences.Queries;

public class ListSubjectEquivalenceGroupsQuery : IRequest<Result<List<SubjectEquivalenceGroupDto>>>
{
}

public class ListSubjectEquivalenceGroupsQueryHandler(IReadRepository<SubjectEquivalenceGroup> readRepository)
    : IRequestHandler<ListSubjectEquivalenceGroupsQuery, Result<List<SubjectEquivalenceGroupDto>>>
{
    public async Task<Result<List<SubjectEquivalenceGroupDto>>> Handle(ListSubjectEquivalenceGroupsQuery query, CancellationToken cancellationToken)
    {
        var groups = await readRepository.ListAsync(new ListSubjectEquivalenceGroupsSpec(), cancellationToken);
        var toReturn =
            groups.Select(SubjectEquivalenceGroupDto.FromEntity).ToList();
        return Result.Success(toReturn);
    }
}
