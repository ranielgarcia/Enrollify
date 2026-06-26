using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.StateMachine;
using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Core.Aggregates.ClassSectionAggregate;

namespace Enrollify.WebAPI.Features.ClassSectionScheduling.ClassSectionMutations;

public class BulkOpenClassSectionsRequest
{
  public List<int> SectionIds { get; set; } = new();
}

public class BulkOpenClassSectionsRequestValidator : Validator<BulkOpenClassSectionsRequest>
{
  public BulkOpenClassSectionsRequestValidator()
  {
    RuleFor(x => x.SectionIds)
      .NotEmpty().WithMessage("At least one section ID is required.");

    RuleForEach(x => x.SectionIds)
      .GreaterThan(0).WithMessage("Each section ID must be greater than zero.");
  }
}

[HttpPost("bulk/open")]
[Group<ClassSectionEndpointSubGroup>]
[Authorize(Policy = PolicyName.HasUpdateClassSectionPermission)]
public class BulkOpenClassSectionsEndpoint(IMediator mediator)
  : Endpoint<BulkOpenClassSectionsRequest, BulkApiResult<BulkOpenClassSectionsResultDto>>
{
  public override async Task<BulkApiResult<BulkOpenClassSectionsResultDto>> ExecuteAsync(
    BulkOpenClassSectionsRequest request,
    CancellationToken cancellationToken)
  {
    List<ClassSectionId> ids = request.SectionIds.Select(ClassSectionId.From).ToList();
    var command = new BulkOpenClassSectionsForEnrollment.Command(ids);
    Result<BulkOpenClassSectionsResultDto> result = await mediator.Send(command, cancellationToken);
    return result.ToBulkResult(dto => dto);
  }
}
