using Ardalis.Result;
using Enrollify.Application.Features.SubjectEquivalences.DTOs;
using Enrollify.Application.Features.SubjectEquivalences.Specifications;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.SubjectEquivalences.Queries;

public class ListSubjectEquivalenceGroupsQuery : IQuery<Result<List<SubjectEquivalenceGroupDto>>>
{
}

public class ListSubjectEquivalenceGroupsQueryHandler(IReadRepository<SubjectEquivalenceGroup> readRepository)
    : IQueryHandler<ListSubjectEquivalenceGroupsQuery, Result<List<SubjectEquivalenceGroupDto>>>
{
    public async ValueTask<Result<List<SubjectEquivalenceGroupDto>>> Handle(ListSubjectEquivalenceGroupsQuery query, CancellationToken cancellationToken)
    {
        var groups = await readRepository.ListAsync(new ListSubjectEquivalenceGroupsSpec(), cancellationToken);
        var toReturn =
            groups.Select(SubjectEquivalenceGroupDto.FromEntity).ToList();
        return Result.Success(toReturn);
    }
}
