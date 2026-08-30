
using Enrollify.Application.Features.ClassSectionScheduling.Events;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSections;

public static class UpdateClassSection
{
  /// <summary>
  /// Updates the adviser of a class section.
  /// Allowed in Draft and Open status (as per domain aggregate guard on UpdateAdviser).
  /// </summary>
  public sealed record Command(
    ClassSectionId Id,
    TeacherId AdviserId) : IRequest<Result<ClassSectionId>>;

  public sealed class Handler : IRequestHandler<Command, Result<ClassSectionId>>
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

    public async Task<Result<ClassSectionId>> Handle(Command command, CancellationToken cancellationToken)
    {
      ClassSection? section = await _classSectionReadRepository.GetByIdAsync(command.Id, cancellationToken);
      if (section is null)
      {
        _logger.LogWarning("Class section with ID {ClassSectionId} not found for update", command.Id.Value);
        return Result.NotFound($"Class section with ID {command.Id.Value} not found.");
      }

      try
      {
        section.UpdateAdviser(command.AdviserId);
      }
      catch (ArgumentException ex)
      {
        return Result.Invalid(new ValidationError(ex.Message));
      }

      Result<ClassSectionId> updateResult = await _classSectionRepository.Update(section, cancellationToken);
      if (!updateResult.IsSuccess)
      {
        _logger.LogError("Failed to update class section {ClassSectionId}: {Errors}",
          command.Id.Value, string.Join(", ", updateResult.Errors));
        return Result.Error("Unable to update the class section.");
      }

      await _publisher.Publish(new RefreshClassSectionValidationIssuesRequestedEvent(section.Id), cancellationToken);

      _logger.LogInformation("Successfully updated class section {ClassSectionId}", command.Id.Value);
      return Result.Success(section.Id);
    }
  }
}
