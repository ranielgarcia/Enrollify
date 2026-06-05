using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.DomainExceptions;
using Enrollify.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands;

public static class AddMultipleSchedulesToOffering
{
    public sealed record ScheduleToAdd(
        string DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime);

    public sealed record Command(
        ClassSectionSubjectOfferingId OfferingId,
        List<ScheduleToAdd> Schedules) : IRequest<Result<List<ClassScheduleId>>>;

    public sealed class Handler : IRequestHandler<Command, Result<List<ClassScheduleId>>>
    {
        private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
        private readonly IClassSectionSubjectOfferingRepository _offeringRepository;
        private readonly IPublisher _publisher;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
            IClassSectionSubjectOfferingRepository offeringRepository,
            IPublisher publisher,
            ILogger<Handler> logger)
        {
            _offeringReadRepository = offeringReadRepository;
            _offeringRepository = offeringRepository;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<Result<List<ClassScheduleId>>> Handle(Command command, CancellationToken cancellationToken)
        {
            if (command.Schedules == null || command.Schedules.Count == 0)
            {
                return Result.Invalid(new ValidationError(nameof(command.Schedules),
                    "At least one schedule must be provided.",
                    string.Empty, ValidationSeverity.Error));
            }

            // Enforce max 7 schedules per batch (one per day of week)
            if (command.Schedules.Count > 7)
            {
                return Result.Invalid(new ValidationError(nameof(command.Schedules),
                    "Cannot add more than 7 schedules in a single batch.",
                    string.Empty, ValidationSeverity.Error));
            }

            ClassSectionSubjectOffering? offering = await _offeringReadRepository.FirstOrDefaultAsync(
                new GetClassSectionSubjectOfferingWithSchedulesByIdSpec(command.OfferingId), cancellationToken);

            if (offering is null)
            {
                _logger.LogWarning("Offering {OfferingId} not found when adding schedules", command.OfferingId.Value);
                return Result.NotFound($"Subject offering with ID {command.OfferingId.Value} was not found.");
            }

            var addedSchedules = new List<ClassSchedule>();
            var validationErrors = new List<ValidationError>();

            // Validate and parse all day-of-week values first
            foreach (var scheduleToAdd in command.Schedules)
            {
                if (!DayOfWeekEnum.TryFromValue(scheduleToAdd.DayOfWeek, out DayOfWeekEnum? dayOfWeek))
                {
                    validationErrors.Add(new ValidationError(nameof(scheduleToAdd.DayOfWeek),
                        $"'{scheduleToAdd.DayOfWeek}' is not a valid day of week. Valid values: {string.Join(", ", DayOfWeekEnum.List.Select(d => d.Value))}",
                        string.Empty, ValidationSeverity.Error));
                }
            }

            if (validationErrors.Count > 0)
            {
                return Result.Invalid(validationErrors);
            }

            // Check for duplicate days in the request
            var requestedDays = command.Schedules.Select(s => s.DayOfWeek).ToList();
            var duplicateDays = requestedDays.GroupBy(d => d).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateDays.Count > 0)
            {
                return Result.Invalid(new ValidationError(nameof(command.Schedules),
                    $"Duplicate days found in request: {string.Join(", ", duplicateDays)}. Each day can only be scheduled once.",
                    string.Empty, ValidationSeverity.Error));
            }

            // Add all schedules - domain validation will be applied for each
            foreach (var scheduleToAdd in command.Schedules)
            {
                DayOfWeekEnum.TryFromValue(scheduleToAdd.DayOfWeek, out DayOfWeekEnum? dayOfWeek);
                var newSchedule = new ClassSchedule(command.OfferingId, dayOfWeek!, scheduleToAdd.StartTime, scheduleToAdd.EndTime);

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

            // Save all changes in a single transaction
            Result<ClassSectionSubjectOfferingId> saveResult = await _offeringRepository.Update(offering, cancellationToken);

            if (!saveResult.IsSuccess)
            {
                _logger.LogError("Failed to persist new schedules for offering {OfferingId}: {Errors}",
                    command.OfferingId.Value, string.Join(", ", saveResult.Errors));
                return Result.Error("Unable to add schedules to offering.");
            }

            // Retrieve all added schedule IDs
            var addedIds = new List<ClassScheduleId>();
            foreach (var addedSchedule in addedSchedules)
            {
                var scheduleId = offering.ClassSchedules
                    .FirstOrDefault(s => s.DayOfWeek == addedSchedule.DayOfWeek 
                                      && s.StartTime == addedSchedule.StartTime 
                                      && s.EndTime == addedSchedule.EndTime)
                    ?.Id;

                if (scheduleId is not null)
                {
                    addedIds.Add(scheduleId.Value);
                }
            }

            // Publish eligibility recompute event once after all schedules are added
            await _publisher.Publish(new ClassSectionEligibilityRecomputeRequestedEvent(offering.ClassSectionId), cancellationToken);

            _logger.LogInformation("Added {Count} schedule(s) to offering {OfferingId}", addedIds.Count, command.OfferingId.Value);
            return Result.Success(addedIds);
        }
    }
}
