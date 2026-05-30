using Ardalis.Result;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands;

public static class DeleteClassSectionSubjectOffering
{
    public sealed record Command(ClassSectionSubjectOfferingId Id) : IRequest<Result>;

    public sealed class Handler : IRequestHandler<Command, Result>
    {
        private readonly IClassSectionSubjectOfferingRepository _offeringRepository;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IClassSectionSubjectOfferingRepository offeringRepository,
            ILogger<Handler> logger)
        {
            _offeringRepository = offeringRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            Result result = await _offeringRepository.Delete(command.Id, cancellationToken);

            if (result.Status == ResultStatus.NotFound)
            {
                _logger.LogWarning("Subject offering with ID {OfferingId} not found for deletion", command.Id.Value);
                return result;
            }

            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to delete offering {OfferingId}: {Errors}",
                    command.Id.Value, string.Join(", ", result.Errors));
                return Result.Error("Unable to delete the subject offering.");
            }

            _logger.LogInformation("Deleted subject offering {OfferingId}", command.Id.Value);
            return Result.Success();
        }
    }
}
