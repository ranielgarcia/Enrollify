using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSections;
using Enrollify.Application.Features.ClassSectionScheduling.Repositories;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.Core.Constants;
using Enrollify.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionSubjectOfferings;

public static class UpdateClassSectionSubjectOffering
{
  public sealed record Command(
    ClassSectionSubjectOfferingId Id,
    TeacherId? TeacherId,
    RoomId? RoomId,
    int DaysPerWeek,
    decimal HoursPerDay,
    int? MaxNumberOfStudents) : IRequest<Result<ClassSectionSubjectOfferingId>>;

  public sealed class Handler : IRequestHandler<Command, Result<ClassSectionSubjectOfferingId>>
  {
    private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
    private readonly IClassSectionSubjectOfferingRepository _offeringRepository;
    private readonly IReadRepository<ClassSection> _classSectionReadRepository;
    private readonly IPublisher _publisher;
    private readonly ILogger<Handler> _logger;

    public Handler(
      IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
      IClassSectionSubjectOfferingRepository offeringRepository,
      IReadRepository<ClassSection> classSectionReadRepository,
      IPublisher publisher,
      ILogger<Handler> logger)
    {
      _offeringReadRepository = offeringReadRepository;
      _offeringRepository = offeringRepository;
      _classSectionReadRepository = classSectionReadRepository;
      _publisher = publisher;
      _logger = logger;
    }

    public async Task<Result<ClassSectionSubjectOfferingId>> Handle(Command command,
      CancellationToken cancellationToken)
    {
      ClassSectionSubjectOffering? offering = await _offeringReadRepository.FirstOrDefaultAsync(
        new GetClassSectionSubjectOfferingWithSchedulesByIdSpec(command.Id), cancellationToken);

      if (offering is null)
      {
        _logger.LogWarning("Subject offering with ID {OfferingId} not found for update", command.Id.Value);
        return Result.NotFound($"Subject offering with ID {command.Id.Value} was not found.");
      }

      ClassSection? section =
        await _classSectionReadRepository.FirstOrDefaultAsync(new GetClassSectionByIdSpec(offering.ClassSectionId),
          cancellationToken);

      if (section is not null && section.StatusId != ClassSectionStatusEnum.Draft)
      {
        _logger.LogWarning(
          "Attempting to update a subject offering {OfferingId} for a class section that is not in Draft status anymore.",
          offering.Id.Value);
        return Result.Forbidden(
          "Cannot update an offering because its class section is not in Draft status.");
      }

      if (command.TeacherId is not null)
        offering.UpdateTeacher(command.TeacherId.Value);

      if (command.RoomId is not null)
        offering.UpdateRoom(command.RoomId.Value);

      offering.UpdateSchedule(command.DaysPerWeek, command.HoursPerDay);
      offering.UpdateMaxNumberOfStudents(command.MaxNumberOfStudents);

      Result<ClassSectionSubjectOfferingId> result = await _offeringRepository.Update(offering, cancellationToken);

      if (!result.IsSuccess)
      {
        _logger.LogError("Failed to update offering {OfferingId}: {Errors}",
          command.Id.Value, string.Join(", ", result.Errors));
        return Result.Error("Unable to update the subject offering.");
      }

      await _publisher.Publish(new ClassSectionEligibilityRecomputeRequestedEvent(offering.ClassSectionId),
        cancellationToken);

      _logger.LogInformation("Updated subject offering {OfferingId}", command.Id.Value);
      return result;
    }
  }
}
