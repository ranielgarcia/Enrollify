using Enrollify.Application.Features.Departments.Features;
using Enrollify.Core.Aggregates.DepartmentAggregate;

namespace Enrollify.WebAPI.Features.Departments;

public class DeleteDepartmentRequest
{

    [QueryParam]
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

[HttpDelete("")]
[Group<DepartmentEndpointGroup>]
[Authorize(Policy = PolicyName.HasDeleteDepartmentPermission)]
public class DeleteEndpoint(IMediator mediator)
    : Endpoint<DeleteDepartmentRequest, DeleteApiResult>
{

    public override async Task<DeleteApiResult>
        ExecuteAsync(DeleteDepartmentRequest request, CancellationToken cancellationToken)
    { 
        var result = await mediator.Send(new DeleteDepartment.Command(DepartmentId.From(request.Id)), cancellationToken);
        return result.ToDeleteResult();
    }
}
