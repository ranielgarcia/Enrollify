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

public static class AddScheduleToOffering
{
    public sealed record Command(
        ClassSectionSubjectOfferingId OfferingId,
        string DayOfWeek,
        TimeOnly StartTime,
        TimeOnly EndTime) : IRequest<Result<ClassScheduleId>>;

    public sealed class Handler : IRequestHandler<Command, Result<ClassScheduleId>>
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

        public async Task<Result<ClassScheduleId>> Handle(Command command, CancellationToken cancellationToken)
        {
            ClassSectionSubjectOffering? offering = await _offeringReadRepository.FirstOrDefaultAsync(
                new GetClassSectionSubjectOfferingWithSchedulesByIdSpec(command.OfferingId), cancellationToken);

            if (offering is null)
            {
                _logger.LogWarning("Offering {OfferingId} not found when adding schedule", command.OfferingId.Value);
                return Result.NotFound($"Subject offering with ID {command.OfferingId.Value} was not found.");
            }

            if (!DayOfWeekEnum.TryFromValue(command.DayOfWeek, out DayOfWeekEnum? dayOfWeek))
            {
                return Result.Invalid(new ValidationError(nameof(command.DayOfWeek),
                    $"'{command.DayOfWeek}' is not a valid day of week. Valid values: {string.Join(", ", DayOfWeekEnum.List.Select(d => d.Value))}",
                    string.Empty, ValidationSeverity.Error));
            }

            var newSchedule = new ClassSchedule(command.OfferingId, dayOfWeek!, command.StartTime, command.EndTime);

            try
            {
                offering.AddClassSchedule(newSchedule);
            }
            catch (InvalidClassScheduleException ex)
            {
                _logger.LogWarning("Domain rule violation adding schedule to offering {OfferingId}: {Message}",
                    command.OfferingId.Value, ex.Message);
                return Result.Invalid(new ValidationError(string.Empty, ex.Message, string.Empty, ValidationSeverity.Error));
            }

            Result<ClassSectionSubjectOfferingId> saveResult = await _offeringRepository.Update(offering, cancellationToken);

            if (!saveResult.IsSuccess)
            {
                _logger.LogError("Failed to persist new schedule for offering {OfferingId}: {Errors}",
                    command.OfferingId.Value, string.Join(", ", saveResult.Errors));
                return Result.Error("Unable to add schedule to offering.");
            }

            ClassScheduleId? addedId = offering.ClassSchedules
                .FirstOrDefault(s => s.DayOfWeek == dayOfWeek && s.StartTime == command.StartTime && s.EndTime == command.EndTime)
                ?.Id;

            if (addedId is null)
                return Result.Error("Schedule was added but ID could not be retrieved.");

            await _publisher.Publish(new ClassSectionEligibilityRecomputeRequestedEvent(offering.ClassSectionId), cancellationToken);

            _logger.LogInformation("Added schedule {ScheduleId} to offering {OfferingId}", addedId.Value.Value, command.OfferingId.Value);
            return Result.Success(addedId.Value);
        }
    }
}
