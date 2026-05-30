using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.DomainExceptions;
using Enrollify.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands;

public static class RemoveScheduleFromOffering
{
    public sealed record Command(
        ClassSectionSubjectOfferingId OfferingId,
        ClassScheduleId ScheduleId) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
        private readonly IClassSectionSubjectOfferingRepository _offeringRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
            IClassSectionSubjectOfferingRepository offeringRepository,
            ILogger<Handler> logger)
        {
            _offeringReadRepository = offeringReadRepository;
            _offeringRepository = offeringRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            ClassSectionSubjectOffering? offering = await _offeringReadRepository.FirstOrDefaultAsync(
                new GetClassSectionSubjectOfferingWithSchedulesByIdSpec(command.OfferingId), cancellationToken);

            if (offering is null)
            {
                _logger.LogWarning("Offering {OfferingId} not found when removing schedule {ScheduleId}",
                    command.OfferingId.Value, command.ScheduleId.Value);
                return Result.NotFound($"Subject offering with ID {command.OfferingId.Value} was not found.");
            }

            try
            {
                offering.RemoveClassSchedule(command.ScheduleId);
            }
            catch (InvalidClassScheduleException ex)
            {
                _logger.LogWarning("Domain rule violation removing schedule {ScheduleId} from offering {OfferingId}: {Message}",
                    command.ScheduleId.Value, command.OfferingId.Value, ex.Message);
                return Result.NotFound(ex.Message);
            }

            Result<ClassSectionSubjectOfferingId> saveResult = await _offeringRepository.Update(offering, cancellationToken);

            if (!saveResult.IsSuccess)
            {
                _logger.LogError("Failed to persist schedule removal for offering {OfferingId}: {Errors}",
                    command.OfferingId.Value, string.Join(", ", saveResult.Errors));
                return Result.Error("Unable to remove schedule from offering.");
            }

            _logger.LogInformation("Removed schedule {ScheduleId} from offering {OfferingId}",
                command.ScheduleId.Value, command.OfferingId.Value);
            return Result.Success();
        }
    }
}
