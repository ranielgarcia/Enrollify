using Ardalis.Result;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands;

public static class UpdateClassSectionSubjectOfferingMaxStudents
{
    /// <summary>
    /// Updates the MaxNumberOfStudents cap on a class section subject offering.
    /// Pass null to remove the cap (unlimited).
    /// </summary>
    public sealed record Command(
        ClassSectionSubjectOfferingId Id,
        int? MaxNumberOfStudents) : IRequest<Result<ClassSectionSubjectOfferingId>>;

    public sealed class Handler : IRequestHandler<Command, Result<ClassSectionSubjectOfferingId>>
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

        public async Task<Result<ClassSectionSubjectOfferingId>> Handle(Command command, CancellationToken cancellationToken)
        {
            var offering = await _offeringReadRepository.GetByIdAsync(command.Id, cancellationToken);
            if (offering is null)
            {
                _logger.LogWarning("Subject offering with ID {OfferingId} not found for update", command.Id.Value);
                return Result.NotFound($"Subject offering with ID {command.Id.Value} not found.");
            }

            offering.UpdateMaxNumberOfStudents(command.MaxNumberOfStudents);

            var updateResult = await _offeringRepository.Update(offering, cancellationToken);
            if (!updateResult.IsSuccess)
            {
                _logger.LogError("Failed to update offering {OfferingId}: {Errors}",
                    command.Id.Value, string.Join(", ", updateResult.Errors));
                return Result.Error("Unable to update the subject offering.");
            }

            _logger.LogInformation("Successfully updated MaxNumberOfStudents for offering {OfferingId}", command.Id.Value);
            return Result.Success(offering.Id);
        }
    }
}
