using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using MediatR;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.StateMachine;

public static class BulkOpenClassSectionsForEnrollment
{
  public sealed record Command(List<ClassSectionId> Ids) : IRequest<Result<BulkOpenClassSectionsResultDto>>;

  public sealed class Handler : IRequestHandler<Command, Result<BulkOpenClassSectionsResultDto>>
  {
    private readonly IMediator _mediator;

    public Handler(IMediator mediator)
    {
      _mediator = mediator;
    }

    public async Task<Result<BulkOpenClassSectionsResultDto>> Handle(Command command, CancellationToken cancellationToken)
    {
      var errors = new Dictionary<string, string>();
      var succeeded = 0;

      foreach (var id in command.Ids)
      {
        Result<ClassSectionId> result = await _mediator.Send(
          new OpenClassSectionForEnrollment.Command(id), cancellationToken);

        if (result.IsSuccess)
        {
          succeeded++;
        }
        else
        {
          errors[id.Value.ToString()] = result.Errors.FirstOrDefault() ?? result.ValidationErrors.FirstOrDefault()?.ErrorMessage ?? "An unexpected error occurred.";
        }
      }

      var status = succeeded == 0 ? BulkOperationStatus.Failed
        : succeeded < command.Ids.Count ? BulkOperationStatus.PartialSuccess
        : BulkOperationStatus.Success;

      return Result.Success(new BulkOpenClassSectionsResultDto
      {
        Status = status,
        TotalRequested = command.Ids.Count,
        Succeeded = succeeded,
        Failed = command.Ids.Count - succeeded,
        Errors = errors
      });
    }
  }
}

// TODO: Replace toast summary with action history (notifications) so users can
// review per-section results of all bulk operations in a dedicated UI.
