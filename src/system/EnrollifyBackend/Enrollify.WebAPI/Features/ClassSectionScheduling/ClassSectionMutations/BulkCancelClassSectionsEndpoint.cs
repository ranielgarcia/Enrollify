using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.StateMachine;
using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.ClassSectionScheduling.ClassSectionMutations;

public class BulkCancelClassSectionsRequest
{
  public List<int> SectionIds { get; set; } = new();
}

public class BulkCancelClassSectionsRequestValidator : Validator<BulkCancelClassSectionsRequest>
{
  public BulkCancelClassSectionsRequestValidator()
  {
    RuleFor(x => x.SectionIds)
      .NotEmpty().WithMessage("At least one section ID is required.");

    RuleForEach(x => x.SectionIds)
      .GreaterThan(0).WithMessage("Each section ID must be greater than zero.");
  }
}

[HttpPost("bulk/cancel")]
[Group<ClassSectionEndpointSubGroup>]
[Authorize(Policy = PolicyName.HasUpdateClassSectionPermission)]
public class BulkCancelClassSectionsEndpoint (IMediator mediator)
  : Endpoint<BulkCancelClassSectionsRequest, BulkApiResult<BulkStateChangeClassSectionsResultDto>>
{
  public override async Task<BulkApiResult<BulkStateChangeClassSectionsResultDto>> ExecuteAsync(
    BulkCancelClassSectionsRequest request,
    CancellationToken cancellationToken)
  {
    List<ClassSectionId> ids = request.SectionIds.Select(ClassSectionId.From).ToList();
    var command = new BulkCancelClassSections.Command(ids);
    Result<BulkStateChangeClassSectionsResultDto> result = await mediator.Send(command, cancellationToken);
    return result.ToBulkResult(dto => dto);
  }
}
