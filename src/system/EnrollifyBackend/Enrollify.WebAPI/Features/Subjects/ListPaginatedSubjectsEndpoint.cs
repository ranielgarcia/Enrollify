using Enrollify.Application.Features.Subjects.DTOs;
using Enrollify.Application.Features.Subjects.Queries;

namespace Enrollify.WebAPI.Features.Subjects;

[HttpGet("{page}/{pageSize}")]
[Group<SubjectEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewSubjectsPermission)]
public class ListPaginatedSubjectsEndpoint (IMediator mediator) : EndpointWithoutRequest<Application.PagedResult<SubjectDto>>
{
    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var page = Route<int>("page");
        var pageSize = Route<int>("pageSize");
        var result = await mediator.Send(new ListSubjectsPaginatedQuery(page, pageSize), cancellationToken);
        await Send.OkAsync(result.Value);
    }
}
