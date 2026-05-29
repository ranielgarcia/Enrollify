using Ardalis.Result;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.CourseCurriculumAssignments.Commands;

public static class SyncCourseCurriculumAssignmentsForAcademicYear
{
  public record Command(AcademicYearId AcademicYearId) : IRequest<Result>;

  public class Handler : IRequestHandler<Command, Result>
  {
    private readonly IReadRepository<AcademicYear> _academicYearReadRepository;
    private readonly ILogger<Command> _logger;
    private readonly IMediator _mediator;

    public Handler(
      IReadRepository<AcademicYear> academicYearReadRepository,
      ILogger<Command> logger,
      IMediator mediator)
    {
      _academicYearReadRepository = academicYearReadRepository;
      _logger = logger;
      _mediator = mediator;
    }

    public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
    {
      AcademicYear? academicYear =
        await _academicYearReadRepository.GetByIdAsync(command.AcademicYearId, cancellationToken);
      if (academicYear == null)
      {
        _logger.LogError("Unable to find the academic year with an id of {AcademicYearId}", command.AcademicYearId);
        return Result.Invalid(new ValidationError("Academic year not found."));
      }

      return await _mediator.Send(new SyncCourseCurriculumAssignments.Command(academicYear), cancellationToken);
    }
  }
}
