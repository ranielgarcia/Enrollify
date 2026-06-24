using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSections;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.RoomTypeAggregate;
using Enrollify.Core.Aggregates.SubjectAggregate;
using Enrollify.Core.Constants.AcademicBuiltInData;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.Subjects.Commands;

public static class UpdateSubject
{
  public class Command : IRequest<Result<SubjectId>>
  {
    public SubjectId Id { get; set; }
    public SubjectCode Code { get; set; }
    public string Title { get; set; } = null!;
    public decimal Units { get; set; }
    public string Description { get; set; } = null!;
    public RoomTypeId PreferRoomTypeId { get; set; }
  }

  public sealed class Handler : IRequestHandler<Command, Result<SubjectId>>
  {
    private readonly IReadRepository<Subject> _subjectReadRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly IReadRepository<Course> _courseReadRepository;
    private readonly IReadRepository<RoomType> _roomTypeReadRepository;
    private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
    private readonly IReadRepository<ClassSection> _classSectionReadRepository;

    public Handler(IReadRepository<Subject> subjectReadRepository,
      ISubjectRepository subjectRepository,
      IReadRepository<Course> courseReadRepository,
      IReadRepository<RoomType> roomTypeReadRepository,
      IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
      IReadRepository<ClassSection> classSectionReadRepository)
    {
      _subjectReadRepository = subjectReadRepository;
      _subjectRepository = subjectRepository;
      _courseReadRepository = courseReadRepository;
      _roomTypeReadRepository = roomTypeReadRepository;
      _offeringReadRepository = offeringReadRepository;
      _classSectionReadRepository = classSectionReadRepository;
    }

    public async Task<Result<SubjectId>> Handle(Command command, CancellationToken cancellationToken)
    {
      if (command.Code == BuiltInSubjectsEnum.ElectivePlaceholder.Code)
        return Result.Invalid(new ValidationError
        {
          ErrorMessage =
            $"The subject code '{BuiltInSubjectsEnum.ElectivePlaceholder.Code}' is reserved and cannot be used."
        });

      Subject? existing = await _subjectReadRepository.GetByIdAsync(command.Id, cancellationToken);
      if (existing == null) return Result.NotFound($"Subject with an ID of {command.Id} not found.");

      // Block updates when the subject is referenced in any Open, Locked, Active, or Completed section
      List<ClassSectionSubjectOffering> offerings = await _offeringReadRepository.ListAsync(
        new GetClassSectionSubjectOfferingsBySubjectIdSpec(command.Id), cancellationToken);
      if (offerings.Count > 0)
      {
        var sectionIds = offerings.Select(o => o.ClassSectionId).Distinct().ToList();
        List<ClassSection> activeSections = await _classSectionReadRepository.ListAsync(
          new GetClassSectionsByIdsInOpenOrHigherStatusSpec(sectionIds), cancellationToken);
        if (activeSections.Count > 0)
          return Result.Forbidden(
            $"Cannot update subject — it is referenced in {activeSections.Count} open or active class section(s). " +
            "Cancel or complete those sections before modifying the subject.");
      }

      RoomType? preferRoomType =
        await _roomTypeReadRepository.GetByIdAsync(command.PreferRoomTypeId, cancellationToken);
      if (preferRoomType == null)
        return Result.Invalid(new ValidationError
          { ErrorMessage = $"Room type with an ID of {command.PreferRoomTypeId} not found." });

      existing
        .UpdateCode(command.Code)
        .UpdateTitle(command.Title)
        .UpdateDescription(command.Description)
        .UpdateUnits(command.Units)
        .UpdatePreferRoomType(command.PreferRoomTypeId);

      await _subjectRepository.Update(existing, cancellationToken);

      return Result.Success(existing.Id);
    }
  }
}
