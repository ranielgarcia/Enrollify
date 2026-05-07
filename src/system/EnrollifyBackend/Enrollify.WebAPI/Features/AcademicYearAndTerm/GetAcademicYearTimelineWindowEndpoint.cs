using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Queries;

namespace Enrollify.WebAPI.Features.AcademicYearAndTerm;

[HttpGet("timeline-window")]
[Group<AcademicYearAndTermEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewAcademicYearAndTermPermission)]
public class GetAcademicYearTimelineWindowEndpoint : EndpointWithoutRequest<AcademicYearContextDto>
{
    private readonly IMediator _mediator;

    public GetAcademicYearTimelineWindowEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAcademicYearTimelineWindowQuery(), ct);
        await Send.OkAsync(result.Value);
    }
}
