using Enrollify.Application.Departments.Features;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.DepartmentAggregate;

namespace Enrollify.WebAPI.Features.Departments;

public class CreateDepartmentResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Chairperson { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int CollegeId { get; set; }
}

public class CreateDepartmentRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Chairperson { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CollegeId { get; set; }
}

public class CreateDepartmentRequestValidator : Validator<CreateDepartmentRequest>
{
    public CreateDepartmentRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Please provide a department code.")
            .MaximumLength(10).WithMessage("Code must be 10 characters or fewer.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Please provide a department name.")
            .MaximumLength(100).WithMessage("Name must be 100 characters or fewer.");

        RuleFor(x => x.Chairperson)
            .NotEmpty().WithMessage("Please provide a department chairperson.")
            .MaximumLength(255).WithMessage("Chairperson must be 255 characters or fewer.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Please provide a department description.")
            .MaximumLength(255).WithMessage("Description must be 255 characters or fewer.");

        RuleFor(x => x.CollegeId)
            .NotNull().WithMessage("Please provide a valid college ID.");
    }
}

[HttpPost("")]
[Group<DepartmentEndpointGroup>]
[Authorize(Policy = PolicyName.HasCreateDepartmentPermission)]
public class CreateEndpoint
    (IMediator mediator)
    : Endpoint<CreateDepartmentRequest, Results<Created<CreateDepartmentResponse>, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
{
    public override async Task<Results<Created<CreateDepartmentResponse>, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
        ExecuteAsync(CreateDepartmentRequest request, CancellationToken cancellationToken)
    { 
        var result = await mediator.Send
            (new CreateDepartment.Command(
                DepartmentCode.From(request.Code), 
                request.Name,
                request.Chairperson,
                request.Description,
                CollegeId.From(request.CollegeId)),
             cancellationToken);

        return result.ToCreatedResult(
            id => $"/departments/{id}",
            id => new CreateDepartmentResponse
            {
                Id = id.Value,
                Code = request.Code,
                Name = request.Name,
                Chairperson = request.Chairperson,
                Department = request.Description,
                CollegeId = request.CollegeId
            });
    }
    
}
