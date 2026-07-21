using Enrollify.Application.Features.Curriculums.Commands;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.WebAPI.Features.Curriculums;

public class UpdateCurriculumRequest
{
    public int Id { get; set; }
    public required int CourseId { get; set; }
    public required int EffectiveYear { get; set; }
    public required string Version { get; set; }
    public string? Description { get; set; }
}

public class UpdateCurriculumRequestValidator : Validator<UpdateCurriculumRequest>
{
    public UpdateCurriculumRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotNull().WithMessage("Please provide a valid curriculum ID.");

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

[HttpPut("{id:int}")]
[Group<CurriculumEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateCurriculumPermission)]
public class UpdateEndpoint(IMediator mediator)
    : Endpoint<UpdateCurriculumRequest, OkOrNotFoundApiResult<int>>
{
    public override async Task<OkOrNotFoundApiResult<int>>
        ExecuteAsync(UpdateCurriculumRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateCurriculum
            .Command(CurriculumId.From(request.Id), CourseId.From(request.CourseId), request.EffectiveYear, request.Version, request.Description));

        return result.ToUpdatedResult(
            id => request.Id);
    }
}
