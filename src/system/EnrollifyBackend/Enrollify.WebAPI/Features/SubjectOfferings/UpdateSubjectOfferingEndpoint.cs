using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;

namespace Enrollify.WebAPI.Features.SubjectOfferings;

public class UpdateSubjectOfferingRequest
{
  public int Id { get; set; }
  public int? TeacherId { get; set; }
  public int? RoomId { get; set; }
  public int DaysPerWeek { get; set; }
  public decimal HoursPerDay { get; set; }
  public int? MaxNumberOfStudents { get; set; }
}

public class UpdateSubjectOfferingRequestValidator : Validator<UpdateSubjectOfferingRequest>
{
  public UpdateSubjectOfferingRequestValidator()
  {
    RuleFor(x => x.Id)
      .GreaterThan(0).WithMessage("A valid offering ID is required.");

    RuleFor(x => x.DaysPerWeek)
      .GreaterThan(0).WithMessage("Days per week must be greater than 0.")
      .LessThanOrEqualTo(7).WithMessage("Days per week cannot exceed 7.");

    RuleFor(x => x.HoursPerDay)
      .GreaterThan(0).WithMessage("Hours per day must be greater than 0.")
      .LessThanOrEqualTo(24).WithMessage("Hours per day cannot exceed 24.");

    RuleFor(x => x.MaxNumberOfStudents)
      .GreaterThan(0).WithMessage("Max students must be greater than 0 when provided.")
      .When(x => x.MaxNumberOfStudents.HasValue);
  }
}

[HttpPut("{id:int}")]
[Group<SubjectOfferingsEndpointGroup>]
[Authorize(Policy = PolicyName.HasUpdateSubjectOfferingPermission)]
public class UpdateSubjectOfferingEndpoint : Endpoint<UpdateSubjectOfferingRequest, OkOrNotFoundApiResult<int>>
{
  private readonly IMediator _mediator;

  public UpdateSubjectOfferingEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override async Task<OkOrNotFoundApiResult<int>> ExecuteAsync(
    UpdateSubjectOfferingRequest request, CancellationToken cancellationToken)
  {
    Result<ClassSectionSubjectOfferingId> result = await _mediator.Send(
      new UpdateClassSectionSubjectOffering.Command(
        ClassSectionSubjectOfferingId.From(request.Id),
        request.TeacherId.HasValue ? TeacherId.From(request.TeacherId.Value) : null,
        request.RoomId.HasValue ? RoomId.From(request.RoomId.Value) : null,
        request.DaysPerWeek,
        request.HoursPerDay,
        request.MaxNumberOfStudents),
      cancellationToken);

    return result.ToUpdatedResult(id => id.Value);
  }
}
