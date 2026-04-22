using Enrollify.Application.Features.Departments.Queries;
using Enrollify.Application.Features.Departments.DTOs;

namespace Enrollify.WebAPI.Features.Departments;

[HttpGet("")]
[Group<DepartmentEndpointGroup>]
[Authorize(Policy = PolicyName.HasViewDepartmentPermission)]
public class ListDepartmentsEndpoint (IMediator mediator) : EndpointWithoutRequest<List<DepartmentDto>>
{
    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListDepartmentsQuery(),cancellationToken);
        await Send.OkAsync(result.Value);
    }
}
