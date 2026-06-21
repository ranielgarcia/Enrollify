using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.StateMachine;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.ClassSectionScheduling.ClassSectionMutations;

public class OpenClassSectionRequest
{
  public int Id { get; set; }
}

public class OpenClassSectionRequestValidator : Validator<OpenClassSectionRequest>
{
  public OpenClassSectionRequestValidator()
  {
    RuleFor(x => x.Id)
      .GreaterThan(0).WithMessage("Please provide a valid class section ID.");
  }
}

[HttpPut("{id:int}/open")]
[Group<ClassSectionEndpointSubGroup>]
[Authorize(Policy = PolicyName.HasUpdateClassSectionPermission)]
public class OpenClassSectionEndpoint(IMediator mediator)
  : Endpoint<OpenClassSectionRequest, OkOrNotFoundApiResult<int>>
{
  public override async Task<OkOrNotFoundApiResult<int>> ExecuteAsync(
    OpenClassSectionRequest request, CancellationToken cancellationToken)
  {
    Result<ClassSectionId> result = await mediator.Send(
      new OpenClassSectionForEnrollment.Command(ClassSectionId.From(request.Id)), cancellationToken);
    return result.ToUpdateResult(id => id.Value);
  }
}
