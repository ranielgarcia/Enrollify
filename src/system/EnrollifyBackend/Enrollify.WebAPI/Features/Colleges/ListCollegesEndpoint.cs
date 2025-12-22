using Enrollify.Application.Colleges.DTOs;
using Enrollify.Application.Colleges.Features;
using Enrollify.WebAPI.Authorization;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Features.Colleges;

[HttpGet("")]
[Group<CollegeEndpointsGroup>]
[Authorize(Policy = PolicyName.HasViewCollegePermission)]
public class ListCollegesEndpoint : EndpointWithoutRequest<List<CollegeDTO>>
{
    private readonly IMediator _mediator;

    public ListCollegesEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task HandleAsync(CancellationToken c)
    {
        var result = await _mediator.Send(new ListCollegesQuery(), c);
        await Send.OkAsync(result.Value);
    }
}
