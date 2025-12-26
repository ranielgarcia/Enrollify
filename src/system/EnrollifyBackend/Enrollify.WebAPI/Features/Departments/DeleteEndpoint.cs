using Enrollify.Application.Departments.Features;
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
    : Endpoint<DeleteDepartmentRequest, Results<NoContent, NotFound, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
{

    public override async Task<Results<NoContent, NotFound, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
        ExecuteAsync(DeleteDepartmentRequest request, CancellationToken cancellationToken)
    { 
        var result = await mediator.Send(new DeleteDepartment.Command(DepartmentId.From(request.Id)), cancellationToken);
        return result.ToDeleteResult();
    }
}
