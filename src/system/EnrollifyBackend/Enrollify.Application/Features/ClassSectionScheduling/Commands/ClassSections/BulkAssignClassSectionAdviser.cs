using Enrollify.Application.Features.ClassSectionScheduling.Events;
using Enrollify.Application.Features.Notifications;
using Enrollify.Core.Constants.Authorization;
using Enrollify.Core.Services.NotificationServices.Models;
using INotificationPublisher = Enrollify.Core.Services.NotificationServices.INotificationPublisher;

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
    private readonly IApplicationEventDispatcher _eventDispatcher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<Handler> _logger;

    public Handler(
      IReadRepository<ClassSection> classSectionReadRepository,
      IClassSectionRepository classSectionRepository,
      IApplicationEventDispatcher eventDispatcher,
      IUnitOfWork unitOfWork,
      INotificationPublisher notificationPublisher,
      ILogger<Handler> logger)
    {
      _classSectionReadRepository = classSectionReadRepository;
      _classSectionRepository = classSectionRepository;
      _eventDispatcher = eventDispatcher;
      _unitOfWork = unitOfWork;
      _notificationPublisher = notificationPublisher;
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

        await using ITransactionScope transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        Result<ClassSectionId> updateResult = await _classSectionRepository.BulkUpdate(classSections, cancellationToken);
        if (!updateResult.IsSuccess)
        {
          _logger.LogError("Failed to bulk update class sections. Errors: {Errors}",
            string.Join(", ", updateResult.Errors));
          return Result.Error("Unable to bulk update the class sections.");
        }

        // TODO: Use the correct Target Role
        await _notificationPublisher.SuccessTargetRoleNotification(new NotificationForTargetRoleCreation(
          NotificationTypeConstants.BulkAssignClassSectionAdviser,
          "Bulk Assign Class Section Adviser",
          $"Successfully assigned advisers to class sections.",
          NotificationCategoryEnum.Academic,
          [RolesEnum.SystemAdmin]
        ));

        await _eventDispatcher.DispatchDeferredAsync(new RefreshClassSectionDataQualityValidationIssuesRequestedEvent(classSections.Select(x => x.Id).ToList()), cancellationToken);

        await _unitOfWork.SaveChangesAndFlushMessagesThenCommitAsync(cancellationToken);
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
