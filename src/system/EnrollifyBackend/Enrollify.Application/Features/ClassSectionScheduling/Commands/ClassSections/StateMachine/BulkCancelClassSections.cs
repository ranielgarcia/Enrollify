using Enrollify.Application.Features.ClassSectionScheduling.DTOs;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections.StateMachine;

public static class BulkCancelClassSections
{
  public sealed record Command(List<ClassSectionId> Ids) : IRequest<Result<BulkStateChangeClassSectionsResultDto>>;

  public sealed class Handler : IRequestHandler<Command, Result<BulkStateChangeClassSectionsResultDto>>
  {
    private readonly IMediator _mediator;

    public Handler(IMediator mediator)
    {
      _mediator = mediator;
    }

    public async Task<Result<BulkStateChangeClassSectionsResultDto>> Handle(Command command, CancellationToken cancellationToken)
    {
      var errors = new Dictionary<string, string>();
      var succeeded = 0;

      foreach (var id in command.Ids)
      {
        Result<ClassSectionId> result = await _mediator.Send(
          new CancelClassSection.Command(id), cancellationToken);

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

      return Result.Success(new BulkStateChangeClassSectionsResultDto
      {
        Status = status.Name,
        TotalRequested = command.Ids.Count,
        Succeeded = succeeded,
        Failed = command.Ids.Count - succeeded,
        Errors = errors
      });
    }
  }
}
