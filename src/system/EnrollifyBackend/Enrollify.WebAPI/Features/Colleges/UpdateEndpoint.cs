using Enrollify.Application.Colleges.Features;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.WebAPI.Authorization;
using Enrollify.WebAPI.Extensions;
using FastEndpoints;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Enrollify.WebAPI.Features.Colleges;

public class UpdateCollegeResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Dean { get; set; } = string.Empty;
}


public class UpdateCollegeRequest
{

    [QueryParam]
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Dean { get; set; } = string.Empty;
}

public class UpdateCollegeRequestValidator : Validator<UpdateCollegeRequest>
{
    public UpdateCollegeRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid college ID.");
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Please provide a college code.")
            .MaximumLength(10).WithMessage("Name must be 10 characters or fewer.");
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

[HttpPut("")]
[Group<CollegeEndpointsGroup>]
[Authorize(Policy = PolicyName.HasUpdateCollegePermission)]
public class UpdateEndpoint : Endpoint<UpdateCollegeRequest, Results<Ok<UpdateCollegeResponse>, NotFound, Conflict<string[]>, ProblemHttpResult>>
{
    private readonly IMediator _mediator;

    public UpdateEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<Results<Ok<UpdateCollegeResponse>, NotFound, Conflict<string[]>, ProblemHttpResult>> 
        ExecuteAsync (UpdateCollegeRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateCollege.Command(CollegeId.From(request.Id), CollegeCode.From(request.Code), request.Name, request.Description, request.Dean));
        return result.ToUpdateResult(
            id => new UpdateCollegeResponse
            {
                Id = id.Value,
                Code =  request.Code,
                Name = request.Name,
                Description = request.Description,
                Dean = request.Dean
            });
    }
}
