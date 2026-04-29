using Enrollify.Application.Features.Departments.Commands;
using Enrollify.Core.Aggregates.DepartmentAggregate;

namespace Enrollify.WebAPI.Features.Departments;

public class DeleteDepartmentRequest
{
    public int Id { get; set; }
}

public class DeleteDepartmentRequestValidator : Validator<DeleteDepartmentRequest>
{
    public DeleteDepartmentRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid department ID.");
    }
}

[HttpDelete("{id:int}")]
[Group<DepartmentEndpointGroup>]
[Authorize(Policy = PolicyName.HasDeleteDepartmentPermission)]
public class DeleteEndpoint(IMediator mediator)
    : Endpoint<DeleteDepartmentRequest, DeleteApiResult>
{

    public override async Task<DeleteApiResult>
        ExecuteAsync(DeleteDepartmentRequest request, CancellationToken ct)
    { 
        var result = await mediator.Send(new DeleteDepartment.Command(DepartmentId.From(request.Id)), ct);
        return result.ToDeleteResult();
    }
}
