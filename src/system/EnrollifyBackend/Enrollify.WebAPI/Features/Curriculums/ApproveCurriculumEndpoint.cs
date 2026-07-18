using Enrollify.Application.Features.Curriculums.Commands;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.WebAPI.Features.Curriculums;

public class ApproveCurriculumRequest
{
    public int Id { get; set; }
}

public class ApproveCurriculumRequestValidator : Validator<ApproveCurriculumRequest>
{
    public ApproveCurriculumRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Please provide a valid curriculum ID.");
    }
}

[HttpPut("{id:int}/approve")]
[Group<CurriculumEndpointGroup>]
[Authorize(Policy = PolicyName.CanApproveCurriculumPermission)]
public class ApproveCurriculumEndpoint(IMediator mediator)
    : Endpoint<ApproveCurriculumRequest, OkOrNotFoundApiResult<int>>
{
    public override async Task<OkOrNotFoundApiResult<int>>
        ExecuteAsync(ApproveCurriculumRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ApproveCurriculum.Command(CurriculumId.From(request.Id)), cancellationToken);
        return result.ToUpdatedResult(
            id => request.Id);
    }
}
