using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Queries;

namespace Enrollify.WebAPI.Features.AcademicYearAndTerm;

public class GetAcademicYearTimelineWindowRequest
{
    [QueryParam]
    public bool? IncludePastYears { get; set; }
    [QueryParam]
    public int? NumberOfPastYears { get; set; }
    [QueryParam]
    public bool? IncludeFutureYears { get; set; }
    [QueryParam]
    public int? NumberOfFutureYears { get; set; }
}

[HttpGet("timeline-window")]
[Group<AcademicYearAndTermEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewAcademicYearAndTermPermission)]
public class GetAcademicYearTimelineWindowEndpoint : Endpoint<GetAcademicYearTimelineWindowRequest, AcademicYearTimelineDto>
{
    private readonly IMediator _mediator;

    public GetAcademicYearTimelineWindowEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task HandleAsync(GetAcademicYearTimelineWindowRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAcademicYearTimelineWindowQuery
        {
            IncludePastYears = request.IncludePastYears ?? false,
            NumberOfPastYears = request.NumberOfPastYears ?? 2,
            IncludeFutureYears = request.IncludeFutureYears ?? false,
            NumberOfFutureYears = request.NumberOfFutureYears ?? 2
        }, ct);
        await Send.OkAsync(result.Value);
    }
}
