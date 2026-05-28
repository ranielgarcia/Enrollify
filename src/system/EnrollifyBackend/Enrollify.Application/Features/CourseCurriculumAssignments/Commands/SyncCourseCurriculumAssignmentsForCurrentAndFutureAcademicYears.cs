using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Queries;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.CourseCurriculumAssignments.Commands;

public static class SyncCourseCurriculumAssignmentsForCurrentAndFutureAcademicYears
{
  public record Command() : IRequest<Result>;

  public class Handler : IRequestHandler<Command, Result>
  {
    private readonly IMediator _mediator;
    private readonly ILogger<Handler> _logger;

    public Handler(IMediator mediator, ILogger<Handler> logger)
    {
      _mediator = mediator;
      _logger = logger;
    }

    public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
    {
      Result<AcademicYearTimelineDto> academicYearsResult =
        await _mediator.Send(new GetAcademicYearTimelineWindowQuery
        {
          IncludeFutureYears = true,
          NumberOfFutureYears = 2
        }, cancellationToken);

      if (academicYearsResult.IsSuccess && academicYearsResult.Value.Current is not null)
      {
        Result syncResult =
          await _mediator.Send(
            new SyncCourseCurriculumAssignmentsForAcademicYear.Command(academicYearsResult.Value.Current.Id),
            cancellationToken);
        if (!syncResult.IsSuccess)
          _logger.LogError(
            "Failed to sync course curriculum assignments for current academic year with id {AcademicYearId}. Error: {Error}",
            academicYearsResult.Value.Current.Id, syncResult.Errors.FirstOrDefault());
      }

      return Result.Success();
    }
  }
}
