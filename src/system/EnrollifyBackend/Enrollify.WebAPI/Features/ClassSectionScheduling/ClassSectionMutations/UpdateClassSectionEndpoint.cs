using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;

namespace Enrollify.WebAPI.Features.ClassSectionScheduling.ClassSectionMutations;

public class UpdateClassSectionResponse
{
  public int Id { get; set; }
  public int AdviserId { get; set; }
}

public class UpdateClassSectionRequest
{
  public int Id { get; set; }
  public int AdviserId { get; set; }
}

public class UpdateClassSectionRequestValidator : Validator<UpdateClassSectionRequest>
{
  public UpdateClassSectionRequestValidator()
  {
    RuleFor(x => x.Id)
      .GreaterThan(0).WithMessage("Please provide a valid class section ID.");

    RuleFor(x => x.AdviserId)
      .GreaterThan(0).WithMessage("Adviser ID is required.");
  }
}

[HttpPut("{id:int}")]
[Group<ClassSectionEndpointSubGroup>]
[Authorize(Policy = PolicyName.HasUpdateClassSectionPermission)]
public class
  UpdateClassSectionEndpoint : Endpoint<UpdateClassSectionRequest, OkOrNotFoundApiResult<UpdateClassSectionResponse>>
{
  private readonly IMediator _mediator;

  public UpdateClassSectionEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override async Task<OkOrNotFoundApiResult<UpdateClassSectionResponse>> ExecuteAsync(
    UpdateClassSectionRequest request, CancellationToken cancellationToken)
  {
    Result<ClassSectionId> result = await _mediator.Send(
      new UpdateClassSection.Command(
        ClassSectionId.From(request.Id),
        TeacherId.From(request.AdviserId)),
      cancellationToken);

    return result.ToUpdatedResult(id => new UpdateClassSectionResponse
    {
      Id = id.Value,
      AdviserId = request.AdviserId
    });
  }
}
