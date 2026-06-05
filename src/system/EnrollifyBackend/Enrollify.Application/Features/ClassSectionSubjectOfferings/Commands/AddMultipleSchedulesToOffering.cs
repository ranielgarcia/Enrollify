using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;
using Enrollify.Application.Features.ClassSchedules.Models;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.DomainExceptions;
using Enrollify.Core.Services.ScheduleConflictDetection;
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

    public sealed record Response(
        List<ClassScheduleId> AddedScheduleIds,
        List<ConflictResultDto> Conflicts);
    
    public sealed record Command(
        ClassSectionSubjectOfferingId OfferingId,
        List<ScheduleToAdd> Schedules) : IRequest<Result<Response>>;

    public sealed class Handler : IRequestHandler<Command, Result<Response>>
    {
        private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
        private readonly IReadRepository<ClassSection> _sectionReadRepository;
        private readonly IClassSectionSubjectOfferingRepository _offeringRepository;
        private readonly IPublisher _publisher;
        private readonly ILogger<Handler> _logger;
        private readonly ScheduleConflictDetector _conflictDetector;

        public Handler(
            IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
            IReadRepository<ClassSection> sectionReadRepository,
            IClassSectionSubjectOfferingRepository offeringRepository,
            IPublisher publisher,
            ILogger<Handler> logger,
            ScheduleConflictDetector conflictDetector)
        {
            _offeringReadRepository = offeringReadRepository;
            _sectionReadRepository = sectionReadRepository;
            _offeringRepository = offeringRepository;
            _publisher = publisher;
            _logger = logger;
            _conflictDetector = conflictDetector;
        }

        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
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
            
            // Detect conflicts after successful save (Phase 1 requirement)
            var conflicts = await DetectConflictsForOffering(offering, cancellationToken);
            
            return Result.Success(new Response(addedIds, conflicts));
        }
        
        /// <summary>
        /// Detects conflicts for the given offering after schedules have been added.
        /// This runs after SaveChanges, so the data is committed to the database.
        /// Conflicts are computed on-demand and returned to the frontend for display.
        /// </summary>
        private async Task<List<ConflictResultDto>> DetectConflictsForOffering(
            ClassSectionSubjectOffering offering,
            CancellationToken cancellationToken)
        {
            // Load the offering's parent section to get AcademicTermId
            var section = await _sectionReadRepository.GetByIdAsync(offering.ClassSectionId, cancellationToken);
            if (section == null)
            {
                _logger.LogWarning("Section {SectionId} not found when detecting conflicts for offering {OfferingId}", 
                    offering.ClassSectionId.Value, offering.Id.Value);
                return new List<ConflictResultDto>();
            }

            var teacherIds = offering.TeacherId.HasValue 
                ? new List<int> { (int)offering.TeacherId.Value } 
                : new List<int>();
            var roomIds = offering.RoomId.HasValue 
                ? new List<int> { (int)offering.RoomId.Value } 
                : new List<int>();

            // If no teacher or room assigned, no cross-offering conflicts to check
            // (DI-05 duplicate subject check will still run in DetectConflicts)
            if (!teacherIds.Any() && !roomIds.Any())
            {
                // Still check for section-level conflicts (HC-03)
                var sectionOnlySchedules = await _offeringRepository.GetSectionSchedulesForConflictDetectionAsync(
                    (int)offering.ClassSectionId, cancellationToken);
                var sectionOnlyConflicts = _conflictDetector.DetectConflicts(sectionOnlySchedules, (int)offering.ClassSectionId);
                return MapConflictsToDtos(sectionOnlyConflicts);
            }

            // Load this section's schedules
            var thisSectionSchedules = await _offeringRepository.GetSectionSchedulesForConflictDetectionAsync(
                (int)offering.ClassSectionId, cancellationToken);

            // Load related schedules for same teacher/room in same term (excluding this section)
            var relatedSchedules = await _offeringRepository.GetRelatedSchedulesForConflictDetectionAsync(
                teacherIds, roomIds, (int)section.AcademicTermId, (int)offering.ClassSectionId, cancellationToken);

            // Combine and detect conflicts
            var allSchedules = thisSectionSchedules.Concat(relatedSchedules).ToList();
            var domainConflicts = _conflictDetector.DetectConflicts(allSchedules, (int)offering.ClassSectionId);

            return MapConflictsToDtos(domainConflicts);
        }
        
        private List<ConflictResultDto> MapConflictsToDtos(List<ConflictResult> conflicts)
        {
            return conflicts.Select(c => new ConflictResultDto
            {
                Type = MapConflictType(c.Type),
                Severity = MapConflictSeverity(c.Severity),
                Message = c.Message,
                Day = c.Day,
                StartTime = c.StartTime?.ToString("HH:mm:ss"),
                EndTime = c.EndTime?.ToString("HH:mm:ss"),
                AffectedOfferings = c.AffectedOfferings?.Select(a => new AffectedOfferingDto
                {
                    Id = a.Id,
                    Subject = new SubjectSummaryDto(a.Subject.Code, a.Subject.Title),
                    Section = new SectionSummaryDto(a.Section.Id, a.Section.Name),
                    Room = a.Room != null 
                        ? new RoomSummaryDto(a.Room.RoomNumber, a.Room.Building) 
                        : null
                }).ToList()
            }).ToList();
        }

        private ConflictTypeEnum MapConflictType(ConflictType type) => type switch
        {
            ConflictType.TeacherDoubleBooked => ConflictTypeEnum.TEACHER_DOUBLE_BOOKED,
            ConflictType.RoomDoubleBooked => ConflictTypeEnum.ROOM_DOUBLE_BOOKED,
            ConflictType.SectionOverlap => ConflictTypeEnum.SECTION_OVERLAP,
            ConflictType.DuplicateSubjectInSection => ConflictTypeEnum.DUPLICATE_SUBJECT_IN_SECTION,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown conflict type: {type}")
        };

        private ConflictSeverityEnum MapConflictSeverity(ConflictSeverity severity) => severity switch
        {
            ConflictSeverity.Info => ConflictSeverityEnum.Info,
            ConflictSeverity.Warning => ConflictSeverityEnum.Warning,
            ConflictSeverity.Error => ConflictSeverityEnum.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(severity), $"Unknown severity: {severity}")
        };
    }
}
