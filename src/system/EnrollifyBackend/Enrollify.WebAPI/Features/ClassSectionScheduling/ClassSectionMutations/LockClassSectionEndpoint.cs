using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.StateMachine;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.ClassSectionScheduling.ClassSectionMutations;

public class LockClassSectionRequest
{
  public int Id { get; set; }
}

public class LockClassSectionRequestValidator : Validator<LockClassSectionRequest>
{
  public LockClassSectionRequestValidator()
  {
    RuleFor(x => x.Id)
      .GreaterThan(0).WithMessage("Please provide a valid class section ID.");
  }
}

[HttpPut("{id:int}/lock")]
[Group<ClassSectionEndpointSubGroup>]
[Authorize(Policy = PolicyName.HasUpdateClassSectionPermission)]
public class LockClassSectionEndpoint(IMediator mediator)
  : Endpoint<LockClassSectionRequest, OkOrNotFoundApiResult<int>>
{
  public override async Task<OkOrNotFoundApiResult<int>> ExecuteAsync(
    LockClassSectionRequest request, CancellationToken cancellationToken)
  {
    Result<ClassSectionId> result = await mediator.Send(
      new LockClassSectionEnrollment.Command(ClassSectionId.From(request.Id)), cancellationToken);
    return result.ToUpdateResult(id => id.Value);
  }
}
