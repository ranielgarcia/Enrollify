using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.StateMachine;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.ClassSectionScheduling.ClassSectionMutations;

public class CancelClassSectionRequest
{
  public int Id { get; set; }
}

public class CancelClassSectionRequestValidator : Validator<CancelClassSectionRequest>
{
  public CancelClassSectionRequestValidator()
  {
    RuleFor(x => x.Id)
      .GreaterThan(0).WithMessage("Please provide a valid class section ID.");
  }
}

[HttpPut("{id:int}/cancel")]
[Group<ClassSectionEndpointSubGroup>]
[Authorize(Policy = PolicyName.HasUpdateClassSectionPermission)]
public class CancelClassSectionEndpoint(IMediator mediator)
  : Endpoint<CancelClassSectionRequest, OkOrNotFoundApiResult<int>>
{
  public override async Task<OkOrNotFoundApiResult<int>> ExecuteAsync(
    CancelClassSectionRequest request, CancellationToken cancellationToken)
  {
    Result<ClassSectionId> result = await mediator.Send(
      new CancelClassSection.Command(ClassSectionId.From(request.Id)), cancellationToken);
    return result.ToUpdateResult(id => id.Value);
  }
}
