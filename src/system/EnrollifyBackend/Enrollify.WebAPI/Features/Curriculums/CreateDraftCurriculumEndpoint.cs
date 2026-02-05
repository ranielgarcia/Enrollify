using Enrollify.Application.Curriculums.Features;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.WebAPI.Features.Curriculums;

public class CreateDraftCurriculumRequest
{
    public required int CourseId { get; set; }

    public required int EffectiveYear { get; set; }

    public required string Version { get; set; }

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
            .NotEmpty().WithMessage("Please provide a curriculum version.")
            .MaximumLength(20).WithMessage("Version must be 20 characters or fewer.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be 500 characters or fewer.")
            .When(x => x.Description is not null);
    }
}

[HttpPost("")]
[Group<CurriculumEndpointGroup>]
[Authorize(Policy = PolicyName.HasCreateCurriculumPermission)]
public class CreateDraftCurriculumEndpoint (IMediator mediator)
    : Endpoint<CreateDraftCurriculumRequest, Results<Created<int>, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
{
    public override async Task<Results<Created<int>, ValidationProblem, Conflict<string[]>, ProblemHttpResult>>
        ExecuteAsync(CreateDraftCurriculumRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new CreateDraftCurriculum
            .Command(CourseId.From(request.CourseId), request.EffectiveYear, request.Version, request.Description));

        return result.ToCreatedResult(
            id => $"/curriculums/{id}",
            id => id.Value);
    }
}
