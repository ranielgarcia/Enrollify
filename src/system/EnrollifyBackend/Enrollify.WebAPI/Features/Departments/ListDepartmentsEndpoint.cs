using Enrollify.Application.Departments.DTOs;
using Enrollify.Application.Departments.Features;

namespace Enrollify.WebAPI.Features.Departments;

[HttpGet("")]
[Group<DepartmentEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewDepartmentPermission)]
public class ListDepartmentsEndpoint (IMediator mediator) : EndpointWithoutRequest<List<DepartmentDTO>>
{
    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListDepartmentsQuery(),cancellationToken);
        await Send.OkAsync(result.Value);
    }
}
