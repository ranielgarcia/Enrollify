using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.StateMachine;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.ClassSectionScheduling.ClassSectionMutations;

[HttpPut("{id:int}/cancel")]
[Group<ClassSectionEndpointSubGroup>]
[Authorize(Policy = PolicyName.HasUpdateClassSectionPermission)]
public class CancelClassSectionEndpoint(IMediator mediator)
  : EndpointWithoutRequest<OkOrNotFoundApiResult<int>>
{
  public override async Task<OkOrNotFoundApiResult<int>> ExecuteAsync(CancellationToken cancellationToken)
  {
    int classSectionId = Route<int>("id");
    Result<ClassSectionId> result = await mediator.Send(
      new CancelClassSection.Command(ClassSectionId.From(classSectionId)), cancellationToken);
    return result.ToUpdatedResult(id => id.Value);
  }
}
