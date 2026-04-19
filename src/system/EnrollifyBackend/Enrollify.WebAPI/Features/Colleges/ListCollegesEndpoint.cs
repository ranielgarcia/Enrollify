using Enrollify.Application.Features.Colleges.Queries;
using Enrollify.Application.Features.Colleges.DTOs;

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
