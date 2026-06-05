using Ardalis.Result;
using Enrollify.Application.Features.Curriculums.Specifications;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate.Models;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.SharedKernel;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Commands;

public static class CreateClassSectionSubjectOffering
{
  public sealed record Command(
    ClassSectionId ClassSectionId,
    CurriculumSubjectId CurriculumSubjectId,
    TeacherId? TeacherId,
    RoomId? RoomId,
    int DaysPerWeek,
    decimal HoursPerDay,
    int? MaxNumberOfStudents) : IRequest<Result<ClassSectionSubjectOfferingId>>;

  public sealed class Handler : IRequestHandler<Command, Result<ClassSectionSubjectOfferingId>>
  {
    private readonly IReadRepository<ClassSection> _classSectionReadRepository;
    private readonly IReadRepository<Curriculum> _curriculumReadRepository;
    private readonly IClassSectionSubjectOfferingRepository _offeringRepository;
    private readonly ILogger<Handler> _logger;

    public Handler(
      IReadRepository<ClassSection> classSectionReadRepository,
      IReadRepository<Curriculum> curriculumReadRepository,
      IClassSectionSubjectOfferingRepository offeringRepository,
      ILogger<Handler> logger)
    {
      _classSectionReadRepository = classSectionReadRepository;
      _curriculumReadRepository = curriculumReadRepository;
      _offeringRepository = offeringRepository;
      _logger = logger;
    }

    public async Task<Result<ClassSectionSubjectOfferingId>> Handle(Command command,
      CancellationToken cancellationToken)
    {
      ClassSection? classSection = await _classSectionReadRepository
        .GetByIdAsync(command.ClassSectionId, cancellationToken);

      if (classSection is null)
      {
        _logger.LogWarning("Class section with ID {ClassSectionId} not found", command.ClassSectionId.Value);
        return Result.NotFound($"Class section with ID {command.ClassSectionId.Value} was not found.");
      }

      Curriculum? curriculum = await _curriculumReadRepository.FirstOrDefaultAsync(
        new GetCurriculumWithSubjectsByIdSpec(classSection.CurriculumId), cancellationToken);

      if (curriculum is null)
      {
        _logger.LogWarning("Curriculum with ID {CurriculumId} not found for class section {ClassSectionId}",
          classSection.CurriculumId.Value, command.ClassSectionId.Value);
        return Result.NotFound($"Curriculum with ID {classSection.CurriculumId.Value} was not found.");
      }

      CurriculumSubject? curriculumSubject = curriculum.GetCurriculumSubjectById(command.CurriculumSubjectId);
      if (curriculumSubject is null)
      {
        _logger.LogWarning("Curriculum subject with ID {CurriculumSubjectId} not found in curriculum {CurriculumId}",
          command.CurriculumSubjectId.Value, curriculum.Id.Value);
        return Result.NotFound(
          $"Curriculum subject with ID {command.CurriculumSubjectId.Value} was not found in curriculum {curriculum.Id.Value}.");
      }

      if (curriculumSubject.Subject is null)
      {
        _logger.LogError("Subject navigation property not loaded for curriculum subject {CurriculumSubjectId}",
          command.CurriculumSubjectId.Value);
        return Result.Error("Subject data could not be loaded for this curriculum subject.");
      }

      decimal snapshotUnits = curriculumSubject.SubjectUnitsOverride ?? curriculumSubject.Subject.Units;

      var newOffering = new ClassSectionSubjectOffering(new ClassSectionSubjectOfferingForCreation
      {
        ClassSectionId = command.ClassSectionId,
        SubjectId = curriculumSubject.SubjectId,
        CurriculumSubjectId = command.CurriculumSubjectId,
        TeacherId = command.TeacherId,
        RoomId = command.RoomId,
        DaysPerWeek = command.DaysPerWeek,
        HoursPerDay = command.HoursPerDay,
        MaxNumberOfStudents = command.MaxNumberOfStudents,
        SnapshotSubjectCode = curriculumSubject.Subject.Code,
        SnapshotSubjectTitle = curriculumSubject.Subject.Title,
        SnapshotUnits = snapshotUnits,
        SnapshotIsElective = curriculumSubject.IsElective,
        SnapshotElectiveGroupName = curriculumSubject.ElectiveGroupName
      });

      Result<ClassSectionSubjectOfferingId> result = await _offeringRepository.Create(newOffering, cancellationToken);

      if (!result.IsSuccess)
      {
        _logger.LogError("Failed to create subject offering for class section {ClassSectionId}: {Errors}",
          command.ClassSectionId.Value, string.Join(", ", result.Errors));
        return Result.Error("Unable to create the subject offering.");
      }

      _logger.LogInformation("Created subject offering {OfferingId} for class section {ClassSectionId}",
        result.Value.Value, command.ClassSectionId.Value);

      return result;
    }
  }
}
