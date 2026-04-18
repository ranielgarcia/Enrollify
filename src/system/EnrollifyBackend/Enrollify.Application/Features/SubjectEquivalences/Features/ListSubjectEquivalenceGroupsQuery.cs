using Ardalis.Result;
using Enrollify.Application.Features.SubjectEquivalences.DTOs;
using Enrollify.Application.Features.SubjectEquivalences.Specifications;
using Enrollify.Core.Aggregates.SubjectEquivalenceGroupAggregate;
using Enrollify.SharedKernel;
using Mediator;

namespace Enrollify.Application.Features.SubjectEquivalences.Features;

public class ListSubjectEquivalenceGroupsQuery : IQuery<Result<List<SubjectEquivalenceGroupDTO>>>
{
}

public class ListSubjectEquivalenceGroupsQueryHandler(IReadRepository<SubjectEquivalenceGroup> readRepository)
    : IQueryHandler<ListSubjectEquivalenceGroupsQuery, Result<List<SubjectEquivalenceGroupDTO>>>
{
    public async ValueTask<Result<List<SubjectEquivalenceGroupDTO>>> Handle(ListSubjectEquivalenceGroupsQuery query, CancellationToken cancellationToken)
    {
        var groups = await readRepository.ListAsync(new ListSubjectEquivalenceGroupsSpec(), cancellationToken);
        var toReturn =
            groups.Select(SubjectEquivalenceGroupDTO.FromEntity).ToList();
        return Result.Success(toReturn);
    }
}
