using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Application.Features.ClassSectionScheduling.Queries.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.ClassSectionScheduling.ClassSectionQueries;

[HttpGet("{sectionId:int}/subject-offerings")]
[Group<ClassSectionEndpointSubGroup>]
[Authorize(Policy = PolicyName.HasViewClassSectionsPermission)]
public class
  GetSubjectOfferingsByClassSectionIdEndpoint : EndpointWithoutRequest<
  List<ClassSectionSubjectOfferingDto>>
{
  private readonly IMediator _mediator;

  public GetSubjectOfferingsByClassSectionIdEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override async Task<List<ClassSectionSubjectOfferingDto>> ExecuteAsync(
    CancellationToken cancellationToken)
  {
    int sectionId = Route<int>("sectionId");

    Result<List<ClassSectionSubjectOfferingDto>> result = await _mediator.Send(
      new GetSubjectOfferingsByClassSectionIdQuery(ClassSectionId.From(sectionId)),
      cancellationToken);

    return result.Value;
  }
}
