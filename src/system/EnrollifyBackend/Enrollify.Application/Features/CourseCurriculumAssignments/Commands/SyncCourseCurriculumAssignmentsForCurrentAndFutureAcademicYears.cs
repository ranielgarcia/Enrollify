namespace Enrollify.Application.Features.CourseCurriculumAssignments.Commands;

public static class SyncCourseCurriculumAssignmentsForCurrentAndFutureAcademicYears
{
  private const int MaxFutureAcademicYearsToSync = 2;
  private const int MaxPastAcademicYearsToSync = 4;

  public record Command(bool IncludePastYears = false) : IRequest<Result>;

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
        new ListFutureAcademicYearsSpec(today, MaxFutureAcademicYearsToSync), cancellationToken)).ToList();

      var errors = new List<string>();

      if (currentAcademicYear is not null)
      {
        Result syncResult =
          await _mediator.Send(
            new SyncCourseCurriculumAssignmentsForAcademicYear.Command(currentAcademicYear.Id),
            cancellationToken);
        if (!syncResult.IsSuccess)
        {
          _logger.LogError(
            "Failed to sync course curriculum assignments for current academic year with id {AcademicYearId}. Error: {Error}",
            currentAcademicYear.Id, syncResult.Errors.FirstOrDefault());
          errors.Add($"AcademicYear {currentAcademicYear.Id}: {syncResult.Errors.FirstOrDefault()}");
        }
      }
      else
      {
        _logger.LogWarning("No current academic year found. Skipping synchronization for current academic year.");
      }

      foreach (AcademicYear futureAcademicYear in futureAcademicYears)
      {
        Result syncResult =
          await _mediator.Send(
            new SyncCourseCurriculumAssignmentsForAcademicYear.Command(futureAcademicYear.Id),
            cancellationToken);
        if (!syncResult.IsSuccess)
        {
          _logger.LogError(
            "Failed to sync course curriculum assignments for future academic year with id {AcademicYearId}. Error: {Error}",
            futureAcademicYear.Id, syncResult.Errors.FirstOrDefault());
          errors.Add($"AcademicYear {futureAcademicYear.Id}: {syncResult.Errors.FirstOrDefault()}");
        }
      }

      if (request.IncludePastYears)
      {
        var pastAcademicYears = (await _academicYearReadRepository.ListAsync(
          new ListPreviousAcademicYearsSpec(MaxPastAcademicYearsToSync), cancellationToken)).ToList();

        foreach (AcademicYear pastAcademicYear in pastAcademicYears)
        {
          Result syncResult =
            await _mediator.Send(
              new SyncCourseCurriculumAssignmentsForAcademicYear.Command(pastAcademicYear.Id),
              cancellationToken);
          if (!syncResult.IsSuccess)
          {
            _logger.LogError(
              "Failed to sync course curriculum assignments for past academic year with id {AcademicYearId}. Error: {Error}",
              pastAcademicYear.Id, syncResult.Errors.FirstOrDefault());
            errors.Add($"AcademicYear {pastAcademicYear.Id}: {syncResult.Errors.FirstOrDefault()}");
          }
        }
      }

      return errors.Count > 0 ? Result.Error(string.Join("; ", errors)) : Result.Success();
    }
  }
}
