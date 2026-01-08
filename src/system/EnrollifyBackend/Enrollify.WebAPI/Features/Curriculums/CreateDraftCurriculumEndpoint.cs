using Enrollify.Application.Curriculums.Features;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.WebAPI.Utilities;

namespace Enrollify.WebAPI.Features.Curriculums;

public class CreateDraftCurriculumRequest
{
    public int CourseId { get; set; }
    public int EffectiveYear { get; set; }

    public string Version { get; set; } = null!;
    public string? Description { get; set; }
}


public class CreateDraftCurriculumRequestValidator : Validator<CreateDraftCurriculumRequest>
{
    public CreateDraftCurriculumRequestValidator()
    {
        RuleFor(x => x.CourseId)
            .NotNull().WithMessage("Please provide a valid course ID.");

        RuleFor(x => x.EffectiveYear)
            .NotNull().WithMessage("Please provide effective year.")
            .GreaterThanOrEqualTo(2000).WithMessage("Please enter an effective year greater than or equal to 2000.");

        RuleFor(x => x.Version)
        .NotEmpty().WithMessage("Please provide a curriculum description.")
        .MaximumLength(20).WithMessage("Description must be 20 characters or fewer.");

        RuleFor(x => x.Description)
        .NotEmpty().WithMessage("Please provide a curriculum description.")
        .MaximumLength(500).WithMessage("Description must be 500 characters or fewer.");
    }
}

[HttpPost("")]
[Group<CurriculumEndpointGroup>]
[Authorize(Policy = PolicyName.HasCreateCurriculumPermission)]
public class CreateDraftCurriculumEndpoint (IMediator mediator, IIdObfuscator idObfuscator)
    : Endpoint<CreateDraftCurriculumRequest, Results<Created<string>, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
{
    public override async Task<Results<Created<string>, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
        ExecuteAsync(CreateDraftCurriculumRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateDraftCurricula
            .Command(CourseId.From(request.CourseId), request.EffectiveYear, request.Version, request.Description));

        return result.ToCreatedResult(
            id => $"/curriculumns/{id}",
            id => idObfuscator.Encode(id.Value));
    }
}
