using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Events;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections;

public static class BulkAssignClassSectionAdviser
{
  public sealed record ClassSectionAdviserAssignment(ClassSectionId ClassSectionId, TeacherId AdviserId);

  /// <summary>
  /// Updates the adviser of a class section.
  /// Allowed in Draft and Open status (as per domain aggregate guard on UpdateAdviser).
  /// </summary>
  public sealed record Command(
    List<ClassSectionAdviserAssignment> ClassSectionAdviserAssignments) : IRequest<Result<Unit>>;

  public sealed class Handler : IRequestHandler<Command, Result<Unit>>
  {
    private readonly IReadRepository<ClassSection> _classSectionReadRepository;
    private readonly IClassSectionRepository _classSectionRepository;
    private readonly IPublisher _publisher;
    private readonly ILogger<Handler> _logger;

    public Handler(
      IReadRepository<ClassSection> classSectionReadRepository,
      IClassSectionRepository classSectionRepository,
      IPublisher publisher,
      ILogger<Handler> logger)
    {
      _classSectionReadRepository = classSectionReadRepository;
      _classSectionRepository = classSectionRepository;
      _publisher = publisher;
      _logger = logger;
    }

    public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
    {
      var duplicateAssignments = request.ClassSectionAdviserAssignments.GroupBy(x => x.ClassSectionId)
        .Where(g => g.Count() > 1)
        .Select(g => g.Key)
        .ToList();
      if (duplicateAssignments.Any())
      {
        return Result.Invalid(new ValidationError($"Duplicate assignments found for class section IDs: {string.Join(", ", duplicateAssignments.Select(id => id.Value))}"));
      }

      var classSectionIds = request.ClassSectionAdviserAssignments.Select((x => x.ClassSectionId));
      var assignAdviserPerSection = request.ClassSectionAdviserAssignments.ToDictionary(x => x.ClassSectionId, x => x.AdviserId);

      var classSections =
        await _classSectionReadRepository.ListAsync(new GetClassSectionsByIdsSpec(classSectionIds), cancellationToken);
      if (!classSections.Any())
      {
        return Result.NotFound("No class sections found for the provided IDs.");
      }

      var missingClassSections = classSectionIds.Except(classSections.Select(cs => cs.Id)).ToList();
      if (missingClassSections.Any())
      {
        return Result.NotFound($"Class sections with IDs {string.Join(", ", missingClassSections.Select(id => id.Value))} not found.");
      }

      try
      {
        foreach (ClassSection classSection in classSections)
        {
          if (!assignAdviserPerSection.TryGetValue(classSection.Id, out TeacherId adviserId))
          {
            return Result.Invalid(new ValidationError($"No adviser assignment found for class section {classSection.Id.Value}"));
          }
          classSection.UpdateAdviser(adviserId);
        }

        Result<ClassSectionId> updateResult = await _classSectionRepository.BulkUpdate(classSections, cancellationToken);
        if (!updateResult.IsSuccess)
        {
          _logger.LogError("Failed to bulk update class sections. Errors: {Errors}",
            string.Join(", ", updateResult.Errors));
          return Result.Error("Unable to bulk update the class sections.");
        }

        await _publisher.Publish(new RefreshClassSectionDataIntegrityValidationIssuesRequestedEvent(classSections.Select(x => x.Id).ToList()), cancellationToken);

      }
      catch (ArgumentException ex)
      {
        return Result.Invalid(new ValidationError(ex.Message));
      }

      _logger.LogDebug("Successfully updated class sections: {@ClassSectionIds}", classSections.Select(cs => cs.Id));
      return Result.Success(Unit.Value);
    }
  }

}
