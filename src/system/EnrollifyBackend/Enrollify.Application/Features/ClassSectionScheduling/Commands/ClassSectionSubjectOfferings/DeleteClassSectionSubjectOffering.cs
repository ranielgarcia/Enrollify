using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionSubjectOfferings;

public static class DeleteClassSectionSubjectOffering
{
  public sealed record Command(ClassSectionSubjectOfferingId Id) : IRequest<Result>;

  public sealed class Handler : IRequestHandler<Command, Result>
  {
    private readonly IClassSectionSubjectOfferingRepository _offeringRepository;
    private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
    private readonly IReadRepository<ClassSection> _classSectionReadRepository;
    private readonly ILogger<Handler> _logger;

    public Handler(
      IClassSectionSubjectOfferingRepository offeringRepository,
      IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
      IReadRepository<ClassSection> classSectionReadRepository,
      ILogger<Handler> logger)
    {
      _offeringRepository = offeringRepository;
      _offeringReadRepository = offeringReadRepository;
      _classSectionReadRepository = classSectionReadRepository;
      _logger = logger;
    }

    public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
    {
      ClassSectionSubjectOffering? offering =
        await _offeringReadRepository.FirstOrDefaultAsync(new GetClassSectionSubjectOfferingByIdSpec(command.Id),
          cancellationToken);

      if (offering is null)
      {
        _logger.LogWarning("Subject offering with ID {OfferingId} not found for deletion", command.Id.Value);
        return Result.NotFound($"Subject offering with ID {command.Id.Value} was not found.");
      }

      ClassSection? section =
        await _classSectionReadRepository.FirstOrDefaultAsync(new GetClassSectionByIdSpec(offering.ClassSectionId),
          cancellationToken);

      if (section is not null && section.StatusId != ClassSectionStatusEnum.Draft)
      {
        _logger.LogWarning(
          "Attempting to delete subject offering {OfferingId} for a class section that is not in Draft status anymore.",
          offering.Id.Value);
        return Result.Forbidden("Cannot delete offering because its class section is not in Draft status.");
      }

      Result result = await _offeringRepository.Delete(offering, cancellationToken);

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
