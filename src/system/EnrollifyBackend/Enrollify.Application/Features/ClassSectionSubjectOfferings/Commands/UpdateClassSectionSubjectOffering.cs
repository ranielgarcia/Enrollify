using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands;

public static class UpdateClassSectionSubjectOffering
{
    public sealed record Command(
        ClassSectionSubjectOfferingId Id,
        TeacherId? TeacherId,
        RoomId? RoomId,
        int DaysPerWeek,
        double HoursPerDay,
        int? MaxNumberOfStudents,
        decimal? SubjectUnitsOverride) : IRequest<Result<ClassSectionSubjectOfferingId>>;

    public sealed class Handler : IRequestHandler<Command, Result<ClassSectionSubjectOfferingId>>
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

        public async Task<Result<ClassSectionSubjectOfferingId>> Handle(Command command, CancellationToken cancellationToken)
        {
            ClassSectionSubjectOffering? offering = await _offeringReadRepository.FirstOrDefaultAsync(
                new GetClassSectionSubjectOfferingWithSchedulesByIdSpec(command.Id), cancellationToken);

            if (offering is null)
            {
                _logger.LogWarning("Subject offering with ID {OfferingId} not found for update", command.Id.Value);
                return Result.NotFound($"Subject offering with ID {command.Id.Value} was not found.");
            }

            if (command.TeacherId is not null)
                offering.UpdateTeacher(command.TeacherId.Value);

            if (command.RoomId is not null)
                offering.UpdateRoom(command.RoomId.Value);

            offering.UpdateSchedule(command.DaysPerWeek, command.HoursPerDay);
            offering.UpdateMaxNumberOfStudents(command.MaxNumberOfStudents);
            offering.UpdateSubjectUnitsOverride(command.SubjectUnitsOverride);

            Result<ClassSectionSubjectOfferingId> result = await _offeringRepository.Update(offering, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to update offering {OfferingId}: {Errors}",
                    command.Id.Value, string.Join(", ", result.Errors));
                return Result.Error("Unable to update the subject offering.");
            }

            await _publisher.Publish(new ClassSectionEligibilityRecomputeRequestedEvent(offering.ClassSectionId), cancellationToken);

            _logger.LogInformation("Updated subject offering {OfferingId}", command.Id.Value);
            return result;
        }
    }
}
