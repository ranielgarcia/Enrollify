using Ardalis.Result;
using Enrollify.Application.Features.AcademicYearAndTerm.DTOs;
using Enrollify.Application.Features.AcademicYearAndTerm.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.SharedKernel;
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
    private readonly IReadRepository<AcademicYear> _academicYearReadRepository;

    public Handler(IMediator mediator, ILogger<Handler> logger,
      IReadRepository<AcademicYear> academicYearReadRepository)
    {
      _mediator = mediator;
      _logger = logger;
      _academicYearReadRepository = academicYearReadRepository;
    }

    public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
    {
      DateTime today = DateTime.UtcNow.Date;
      AcademicYear? currentAcademicYear =
        await _academicYearReadRepository.FirstOrDefaultAsync(new GetActiveAcademicYearSpec(), cancellationToken);
      var futureAcademicYears = (await _academicYearReadRepository.ListAsync(
        new ListFutureAcademicYearsSpec(today, 2), cancellationToken)).ToList();

      if (currentAcademicYear is not null)
      {
        Result syncResult =
          await _mediator.Send(
            new SyncCourseCurriculumAssignments.Command(currentAcademicYear),
            cancellationToken);
        if (!syncResult.IsSuccess)
          _logger.LogError(
            "Failed to sync course curriculum assignments for current academic year with id {AcademicYearId}. Error: {Error}",
            currentAcademicYear.Id, syncResult.Errors.FirstOrDefault());
      }

      if (futureAcademicYears.Count > 0)
        foreach (AcademicYear futureAcademicYear in futureAcademicYears)
        {
          Result syncResult =
            await _mediator.Send(
              new SyncCourseCurriculumAssignments.Command(futureAcademicYear),
              cancellationToken);
          if (!syncResult.IsSuccess)
            _logger.LogError(
              "Failed to sync course curriculum assignments for current academic year with id {AcademicYearId}. Error: {Error}",
              futureAcademicYear.Id, syncResult.Errors.FirstOrDefault());
        }

      return Result.Success();
    }
  }
}
