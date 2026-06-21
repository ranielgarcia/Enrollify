using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Application.Features.ClassSectionScheduling.Queries.ClassSections;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.ClassSectionScheduling.ClassSectionQueries;

[HttpGet("{id:int}")]
[Group<ClassSectionEndpointSubGroup>]
[Authorize(Policy = PolicyName.HasViewClassSectionsPermission)]
public class GetClassSectionByIdEndpoint : EndpointWithoutRequest<OkOrNotFoundApiResult<ClassSectionDetailDto>>
{
  private readonly IMediator _mediator;

  public GetClassSectionByIdEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override async Task<OkOrNotFoundApiResult<ClassSectionDetailDto>> ExecuteAsync(
    CancellationToken cancellationToken)
  {
    int id = Route<int>("id");

    Result<ClassSectionDetailDto> result = await _mediator.Send(
      new GetClassSectionByIdQuery(ClassSectionId.From(id)),
      cancellationToken);

    return result.ToGetByIdResult(dto => dto);
  }
}
