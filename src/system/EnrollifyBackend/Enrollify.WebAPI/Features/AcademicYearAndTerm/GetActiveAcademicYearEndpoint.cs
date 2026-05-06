using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Queries;

namespace Enrollify.WebAPI.Features.AcademicYearAndTerm;

[HttpGet("active")]
[Group<AcademicYearAndTermEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewAcademicYearAndTermPermission)]
public class GetActiveAcademicYearEndpoint(IMediator mediator) : EndpointWithoutRequest<AcademicYearDto>
{
    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await mediator.Send(new GetActiveAcademicYearQuery(), ct);

        if (result.IsNotFound()) await Send.NotFoundAsync(ct);

        await Send.OkAsync(result.Value);
    }
}

