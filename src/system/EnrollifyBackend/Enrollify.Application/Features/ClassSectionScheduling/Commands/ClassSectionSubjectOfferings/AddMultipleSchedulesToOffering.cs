using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.DomainExceptions;
using Enrollify.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionSubjectOfferings;

public static class AddMultipleSchedulesToOffering
{
  public sealed record ScheduleToAdd(
    string DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime);

  public sealed record Response(List<ClassScheduleId> AddedScheduleIds);

  public sealed record Command(
    ClassSectionSubjectOfferingId OfferingId,
    List<ScheduleToAdd> Schedules,
    bool ValidateConflicts = false) : IRequest<Result<Response>>;

  public sealed class Handler : IRequestHandler<Command, Result<Response>>
  {
    private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
    private readonly IReadRepository<ClassSection> _sectionReadRepository;
    private readonly IClassSectionSubjectOfferingRepository _offeringRepository;
    private readonly IPublisher _publisher;
    private readonly ILogger<Handler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public Handler(
      IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
      IReadRepository<ClassSection> sectionReadRepository,
      IClassSectionSubjectOfferingRepository offeringRepository,
      IPublisher publisher,
      ILogger<Handler> logger,
      IUnitOfWork unitOfWork)
    {
      _offeringReadRepository = offeringReadRepository;
      _sectionReadRepository = sectionReadRepository;
      _offeringRepository = offeringRepository;
      _publisher = publisher;
      _logger = logger;
      _unitOfWork = unitOfWork;
    }

    public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
    {
      if (command.Schedules == null || command.Schedules.Count == 0)
        return Result.Invalid(new ValidationError(nameof(command.Schedules),
          "At least one schedule must be provided.",
          string.Empty, ValidationSeverity.Error));

      // Enforce max 7 schedules per batch (one per day of week)
      if (command.Schedules.Count > 7)
        return Result.Invalid(new ValidationError(nameof(command.Schedules),
          "Cannot add more than 7 schedules in a single batch.",
          string.Empty, ValidationSeverity.Error));

      ClassSectionSubjectOffering? offering = await _offeringReadRepository.FirstOrDefaultAsync(
        new GetClassSectionSubjectOfferingWithSchedulesByIdSpec(command.OfferingId), cancellationToken);

      if (offering is null)
      {
        _logger.LogWarning("Offering {OfferingId} not found when adding schedules", command.OfferingId.Value);
        return Result.NotFound($"Subject offering with ID {command.OfferingId.Value} was not found.");
      }

      // Load the offering's parent section to get AcademicTermId
      ClassSection? section = await _sectionReadRepository.GetByIdAsync(offering.ClassSectionId, cancellationToken);
      if (section == null)
      {
        _logger.LogWarning("Section {SectionId} not found when detecting conflicts for offering {OfferingId}",
          offering.ClassSectionId.Value, offering.Id.Value);
        return Result.NotFound($"Parent class section with ID {offering.ClassSectionId.Value} was not found.");
      }

      if (section.StatusId == ClassSectionStatusEnum.Open)
      {
        _logger.LogWarning(
          "Attempted to add schedules to offering {OfferingId} in section {SectionId} which is open for enrollment",
          offering.Id.Value, section.Id.Value);

        return Result.Forbidden(
          "Cannot modify schedules for offerings in sections that are open for enrollment. Please close enrollment for the section before making changes to schedules.");
      }


      var addedSchedules = new List<ClassSchedule>();
      var validationErrors = new List<ValidationError>();

      // Validate and parse all day-of-week values first
      foreach (ScheduleToAdd scheduleToAdd in command.Schedules)
        if (!DayOfWeekEnum.TryFromValue(scheduleToAdd.DayOfWeek, out DayOfWeekEnum? dayOfWeek))
          validationErrors.Add(new ValidationError(nameof(scheduleToAdd.DayOfWeek),
            $"'{scheduleToAdd.DayOfWeek}' is not a valid day of week. Valid values: {string.Join(", ", DayOfWeekEnum.List.Select(d => d.Value))}",
            string.Empty, ValidationSeverity.Error));

      if (validationErrors.Count > 0) return Result.Invalid(validationErrors);

      // Check for duplicate days in the request
      var requestedDays = command.Schedules.Select(s => s.DayOfWeek).ToList();
      var duplicateDays = requestedDays.GroupBy(d => d).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
      if (duplicateDays.Count > 0)
        return Result.Invalid(new ValidationError(nameof(command.Schedules),
          $"Duplicate days found in request: {string.Join(", ", duplicateDays)}. Each day can only be scheduled once.",
          string.Empty, ValidationSeverity.Error));

      // Add all schedules - domain validation will be applied for each
      foreach (ScheduleToAdd scheduleToAdd in command.Schedules)
      {
        DayOfWeekEnum.TryFromValue(scheduleToAdd.DayOfWeek, out DayOfWeekEnum? dayOfWeek);
        var newSchedule =
          new ClassSchedule(command.OfferingId, dayOfWeek!, scheduleToAdd.StartTime, scheduleToAdd.EndTime);

        try
        {
          offering.AddClassSchedule(newSchedule);
          addedSchedules.Add(newSchedule);
        }
        catch (InvalidClassScheduleException ex)
        {
          _logger.LogWarning("Domain rule violation adding schedule to offering {OfferingId}: {Message}",
            command.OfferingId.Value, ex.Message);
          return Result.Invalid(new ValidationError(string.Empty, ex.Message, string.Empty, ValidationSeverity.Error));
        }
      }

      await using ITransactionScope transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

      try
      {
        // Save all changes in a single transaction
        Result<ClassSectionSubjectOfferingId>
          saveResult = await _offeringRepository.Update(offering, cancellationToken);

        if (!saveResult.IsSuccess)
        {
          _logger.LogError("Failed to persist new schedules for offering {OfferingId}: {Errors}",
            command.OfferingId.Value, string.Join(", ", saveResult.Errors));
          return Result.Error("Unable to add schedules to offering.");
        }

        // Retrieve all added schedule IDs
        var addedIds = new List<ClassScheduleId>();
        foreach (ClassSchedule addedSchedule in addedSchedules)
        {
          ClassScheduleId? scheduleId = offering.ClassSchedules
            .FirstOrDefault(s => s.DayOfWeek == addedSchedule.DayOfWeek
                                 && s.StartTime == addedSchedule.StartTime
                                 && s.EndTime == addedSchedule.EndTime)
            ?.Id;

          if (scheduleId is not null) addedIds.Add(scheduleId.Value);
        }

        // Publish eligibility recompute event once after all schedules are added
        await _publisher.Publish(new ClassSectionValidationRecomputeRequestedEvent(offering.ClassSectionId),
          cancellationToken);

        _logger.LogInformation("Added {Count} schedule(s) to offering {OfferingId}", addedIds.Count,
          command.OfferingId.Value);

        await transaction.CommitAsync(cancellationToken);
        return Result.Success(new Response(addedIds));
      }
      catch (Exception e)
      {
        _logger.LogError(e, "Unexpected error adding schedules to offering {OfferingId}", command.OfferingId.Value);
        return Result.Error("An unexpected error occurred while adding schedules to the offering.");
      }
    }
  }
}
