using Enrollify.Application.Subjects.DTOs;
using Enrollify.Application.Subjects.Features;

namespace Enrollify.WebAPI.Features.Subjects;

public class SearchSubjectsPaginatedRequest
{

    [Microsoft.AspNetCore.Mvc.FromRoute]
    public required int Page { get; set; }

    [Microsoft.AspNetCore.Mvc.FromRoute]
    public required int PageSize { get; set; }

    [QueryParam]
    public required string SearchTerm { get; set; }
}

[HttpGet("search/{page}/{pageSize}")]
[Group<SubjectEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewSubjectsPermission)]
public class SearchSubjectsPaginatedEndpoint (IMediator mediator) : Endpoint<SearchSubjectsPaginatedRequest, Application.PagedResult<SubjectDTO>>
{
    public override async Task HandleAsync(SearchSubjectsPaginatedRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SearchSubjectsPaginatedQuery(request.SearchTerm, request.Page, request.PageSize), cancellationToken);
        await Send.OkAsync(result.Value);
    }
}
