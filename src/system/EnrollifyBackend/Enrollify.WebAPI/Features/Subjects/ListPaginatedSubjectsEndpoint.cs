using Ardalis.Result;
using Enrollify.Application.Subjects.DTOs;
using Enrollify.Application.Subjects.Features;

namespace Enrollify.WebAPI.Features.Subjects;

[HttpGet("{page}/{pageSize}")]
[Group<SubjectEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewSubjectsPermission)]
public class ListPaginatedSubjectsEndpoint (IMediator mediator) : EndpointWithoutRequest<Result<Application.PagedResult<SubjectDTO>>>
{
    public override async Task<Result<Application.PagedResult<SubjectDTO>>> HandleAsync(CancellationToken cancellationToken)
    {
        var page = Route<int>("page");
        var pageSize = Route<int>("pageSize");
        var result = await mediator.Send(new ListSubjectsQuery(page, pageSize), cancellationToken);
        return result;
    }
}
