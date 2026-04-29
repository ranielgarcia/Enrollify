using Enrollify.Application.Features.Departments.Commands;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.DepartmentAggregate;

namespace Enrollify.WebAPI.Features.Departments;


public class UpdateDepartmentResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Chairperson { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int CollegeId { get; set; }
}

public class UpdateDepartmentRequest
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Chairperson { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CollegeId { get; set; }
}

public class UpdateDepartmentRequestValidator : Validator<UpdateDepartmentRequest>
{
    public UpdateDepartmentRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid department ID.");

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


[HttpPut("{id:int}")]
[Group<DepartmentEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateDepartmentPermission)]
public class UpdateEndpoint(IMediator mediator)
    : Endpoint<UpdateDepartmentRequest, OkOrNotFoundApiResult<UpdateDepartmentResponse>>
{
    public override async Task<OkOrNotFoundApiResult<UpdateDepartmentResponse>>
        ExecuteAsync(UpdateDepartmentRequest request, CancellationToken cancellationToken)
    { 
        var result = await mediator.Send(
            new UpdateDepartment.Command(
                DepartmentId.From(request.Id),
                DepartmentCode.From(request.Code),
                request.Name,
                request.Chairperson,
                request.Description,
                CollegeId.From(request.CollegeId)
            ),
            cancellationToken
        );

        return result.ToUpdateResult(
            id => new UpdateDepartmentResponse
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
