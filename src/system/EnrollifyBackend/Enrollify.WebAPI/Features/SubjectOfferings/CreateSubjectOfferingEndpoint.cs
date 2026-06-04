using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;

namespace Enrollify.WebAPI.Features.SubjectOfferings;

public class CreateSubjectOfferingRequest
{
  public int ClassSectionId { get; set; }
  public int CurriculumSubjectId { get; set; }
  public int? TeacherId { get; set; }
  public int? RoomId { get; set; }
  public int DaysPerWeek { get; set; }
  public decimal HoursPerDay { get; set; }
  public int? MaxNumberOfStudents { get; set; }
  public decimal? SubjectUnitsOverride { get; set; }
}

public class CreateSubjectOfferingRequestValidator : Validator<CreateSubjectOfferingRequest>
{
  public CreateSubjectOfferingRequestValidator()
  {
    RuleFor(x => x.ClassSectionId)
      .GreaterThan(0).WithMessage("A valid class section ID is required.");

    RuleFor(x => x.CurriculumSubjectId)
      .GreaterThan(0).WithMessage("A valid curriculum subject ID is required.");

    RuleFor(x => x.DaysPerWeek)
      .GreaterThan(0).WithMessage("Days per week must be greater than 0.")
      .LessThanOrEqualTo(7).WithMessage("Days per week cannot exceed 7.");

    RuleFor(x => x.HoursPerDay)
      .GreaterThan(0).WithMessage("Hours per day must be greater than 0.")
      .LessThanOrEqualTo(24).WithMessage("Hours per day cannot exceed 24.");

    RuleFor(x => x.MaxNumberOfStudents)
      .GreaterThan(0).WithMessage("Max students must be greater than 0 when provided.")
      .When(x => x.MaxNumberOfStudents.HasValue);

    RuleFor(x => x.SubjectUnitsOverride)
      .GreaterThan(0).WithMessage("Subject units override must be greater than 0 when provided.")
      .When(x => x.SubjectUnitsOverride.HasValue);
  }
}

[HttpPost("")]
[Group<SubjectOfferingsEndpointGroup>]
[Authorize(Policy = PolicyName.HasCreateSubjectOfferingPermission)]
public class CreateSubjectOfferingEndpoint : Endpoint<CreateSubjectOfferingRequest, CreatedApiResult<int>>
{
  private readonly IMediator _mediator;

  public CreateSubjectOfferingEndpoint(IMediator mediator)
  {
    _mediator = mediator;
  }

  public override async Task<CreatedApiResult<int>> ExecuteAsync(
    CreateSubjectOfferingRequest request, CancellationToken cancellationToken)
  {
    Result<ClassSectionSubjectOfferingId> result = await _mediator.Send(
      new CreateClassSectionSubjectOffering.Command(
        ClassSectionId.From(request.ClassSectionId),
        CurriculumSubjectId.From(request.CurriculumSubjectId),
        request.TeacherId.HasValue ? TeacherId.From(request.TeacherId.Value) : null,
        request.RoomId.HasValue ? RoomId.From(request.RoomId.Value) : null,
        request.DaysPerWeek,
        request.HoursPerDay,
        request.MaxNumberOfStudents,
        request.SubjectUnitsOverride),
      cancellationToken);

    return result.ToCreatedResult(
      id => $"/subject-offerings/{id.Value}",
      id => id.Value);
  }
}
