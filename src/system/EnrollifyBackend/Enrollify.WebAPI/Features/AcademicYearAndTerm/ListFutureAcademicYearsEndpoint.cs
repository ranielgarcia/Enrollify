using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Queries;

namespace Enrollify.WebAPI.Features.AcademicYearAndTerm;

[HttpGet("future")]
[Group<AcademicYearAndTermEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewAcademicYearAndTermPermission)]
public class ListFutureAcademicYearsEndpoint : EndpointWithoutRequest<List<AcademicYearDto>>
{
    private readonly IMediator _mediator;

    public ListFutureAcademicYearsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new ListFutureAcademicYearsQuery(), ct);
        await Send.OkAsync(result.Value);
    }
}
