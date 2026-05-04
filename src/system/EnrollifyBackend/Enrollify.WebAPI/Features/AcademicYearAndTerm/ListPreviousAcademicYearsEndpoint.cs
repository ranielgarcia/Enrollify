using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Queries;
using Enrollify.WebAPI.Features.Colleges;

namespace Enrollify.WebAPI.Features.AcademicYearAndTerm;

[HttpGet("previous")]
[Group<AcademicYearAndTermEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewAcademicYearAndTermPermission)]
public class ListPreviousAcademicYearsEndpoint : EndpointWithoutRequest<List<AcademicYearDto>>
{
    private readonly IMediator _mediator;

    public ListPreviousAcademicYearsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new ListPreviousAcademicYearsQuery(), ct);
        await Send.OkAsync(result.Value);
    }
}
