using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionValidationIssues;
using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.ClassSectionScheduling.ClassSectionMutations;

[HttpPut("{id:int}/recompute-and-get-validation-issues")]
[Group<ClassSectionEndpointSubGroup>]
[Authorize(Policy = PolicyName.HasUpdateClassSectionPermission)]
public class
  RecomputeClassSectionValidationIssuesEndpoint : EndpointWithoutRequest<
  OkOrNotFoundApiResult<List<ClassSectionValidationIssueDto>>>
{
  private readonly IMediator _mediator;

  public RecomputeClassSectionValidationIssuesEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override async Task<
    OkOrNotFoundApiResult<List<ClassSectionValidationIssueDto>>> ExecuteAsync(CancellationToken cancellationToken)
  {
    int id = Route<int>("id");

    Result<List<ClassSectionValidationIssueDto>> result = await _mediator.Send(
      new ComputeAndGetValidationIssuesForClassSection.Command(ClassSectionId.From(id)),
      cancellationToken);

    return result.ToGetByIdResult(dto => dto);
  }
}
