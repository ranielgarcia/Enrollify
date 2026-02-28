using Enrollify.Application.Colleges.Features;
using Enrollify.Core.Aggregates.CollegeAggregate;

namespace Enrollify.WebAPI.Features.Colleges;

public class CreateCollegeResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Dean { get; set; } = string.Empty;
}


public class CreateCollegeRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Dean { get; set; } = string.Empty;
}

public class CreateCollegeRequestValidator : Validator<CreateCollegeRequest>
{
    public CreateCollegeRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Please provide a college code.")
            .MaximumLength(10).WithMessage("Code must be 10 characters or fewer.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Please provide a college name.")
            .MaximumLength(100).WithMessage("Name must be 100 characters or fewer.");
        RuleFor(x => x.Description)
            .MaximumLength(255).WithMessage("Description must be 255 characters or fewer.");
        RuleFor(x => x.Dean)
            .NotEmpty().WithMessage("Please provide a college dean.")
            .MaximumLength(100).WithMessage("Dean must be 100 characters or fewer.");
    }
}

[HttpPost("")]
[Group<CollegeEndpointsGroup>]
[Authorize(Policy = PolicyName.HasCreateCollegePermission)]
public class CreateEndpoint : Endpoint<CreateCollegeRequest, CreatedApiResult<CreateCollegeResponse>>
{
    private readonly IMediator _mediator;

    public CreateEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<CreatedApiResult<CreateCollegeResponse>> 
        ExecuteAsync (CreateCollegeRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateCollege.Command(
            CollegeCode.From(request.Code),
            request.Name,
            request.Description,
            request.Dean), ct);

        return result.ToCreatedResult(
            id => $"/colleges/{id}",
            id => new CreateCollegeResponse
            {
                Id = id.Value,
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                Dean = request.Dean
            });
    }
}
