using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;

namespace Enrollify.WebAPI.Features.ClassSectionScheduling.ClassSectionMutations;

public class AdviserAssignmentRequest
{
  public int ClassSectionId { get; set; }
  public int AdviserId { get; set; }
}

public class BulkAssignClassSectionAdviserRequest
{
  public List<AdviserAssignmentRequest> ClassSectionAdviserAssignments { get; set; } = new();
}

public class BulkAssignClassSectionAdviserRequestValidator : Validator<BulkAssignClassSectionAdviserRequest>
{
  public BulkAssignClassSectionAdviserRequestValidator()
  {
    RuleFor(x => x.ClassSectionAdviserAssignments)
      .NotEmpty().WithMessage("At least one adviser assignment is required.");

    RuleForEach(x => x.ClassSectionAdviserAssignments)
      .ChildRules(assignment =>
      {
        assignment.RuleFor(x => x.ClassSectionId)
          .GreaterThan(0).WithMessage("Each class section ID must be greater than zero.");

        assignment.RuleFor(x => x.AdviserId)
          .GreaterThan(0).WithMessage("Each adviser ID must be greater than zero.");
      });
  }
}

[HttpPost("bulk/assign-adviser")]
[Group<ClassSectionEndpointSubGroup>]
[Authorize(Policy = PolicyName.HasUpdateClassSectionPermission)]
public class BulkAssignClassSectionAdviserEndpoint(IMediator mediator)
  : Endpoint<BulkAssignClassSectionAdviserRequest, OkOrNotFoundApiResult<Unit>>
{
  public override async Task<OkOrNotFoundApiResult<Unit>> ExecuteAsync(
    BulkAssignClassSectionAdviserRequest request,
    CancellationToken cancellationToken)
  {
    var assignments = request.ClassSectionAdviserAssignments
      .Select(a => new BulkAssignClassSectionAdviser.ClassSectionAdviserAssignment(
        ClassSectionId.From(a.ClassSectionId),
        TeacherId.From(a.AdviserId)))
      .ToList();

    var command = new BulkAssignClassSectionAdviser.Command(assignments);
    Result<Unit> result = await mediator.Send(command, cancellationToken);
    return result.ToOkOnlyResult(x => Unit.Value);
  }
}
