using Ardalis.Result;
using Enrollify.Application.Features.ClassSchedules.Models;
using Enrollify.Application.Features.ClassSectionSubjectOfferings.DTOs;
using Enrollify.Application.Features.ClassSectionSubjectOfferings.Specifications;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Services.ScheduleConflictDetection;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.ClassSectionSubjectOfferings.Queries;

public record GetOfferingsByClassSectionIdQuery(ClassSectionId ClassSectionId)
    : IRequest<Result<IReadOnlyList<ClassSectionSubjectOfferingDto>>>;

public class GetOfferingsByClassSectionIdQueryHandler
    : IRequestHandler<GetOfferingsByClassSectionIdQuery, Result<IReadOnlyList<ClassSectionSubjectOfferingDto>>>
{
    private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
    private readonly IReadRepository<ClassSection> _sectionReadRepository;
    private readonly IClassSectionSubjectOfferingRepository _offeringRepository;
    private readonly ScheduleConflictDetector _conflictDetector;

    public GetOfferingsByClassSectionIdQueryHandler(
        IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
        IReadRepository<ClassSection> sectionReadRepository,
        IClassSectionSubjectOfferingRepository offeringRepository,
        ScheduleConflictDetector conflictDetector)
    {
        _offeringReadRepository = offeringReadRepository;
        _sectionReadRepository = sectionReadRepository;
        _offeringRepository = offeringRepository;
        _conflictDetector = conflictDetector;
    }

    public async Task<Result<IReadOnlyList<ClassSectionSubjectOfferingDto>>> Handle(
        GetOfferingsByClassSectionIdQuery request,
        CancellationToken cancellationToken)
    {
        List<ClassSectionSubjectOffering> offerings = await _offeringReadRepository.ListAsync(
            new GetClassSectionSubjectOfferingsWithFullDetailsByClassSectionIdSpec(request.ClassSectionId),
            cancellationToken);

        if (!offerings.Any())
        {
            return Result.Success<IReadOnlyList<ClassSectionSubjectOfferingDto>>(
                new List<ClassSectionSubjectOfferingDto>().AsReadOnly());
        }

        // Convert to DTOs
        var dtos = offerings
            .Select(ClassSectionSubjectOfferingDto.FromEntity)
            .ToList();

        // Detect and embed conflicts (Phase 1 requirement)
        await DetectAndEmbedConflicts(dtos, request.ClassSectionId, cancellationToken);

        return Result.Success<IReadOnlyList<ClassSectionSubjectOfferingDto>>(dtos.AsReadOnly());
    }

    /// <summary>
    /// Detects conflicts for all offerings in the section and embeds them in the DTOs.
    /// This runs on every section detail page load to show current conflict state.
    /// </summary>
    private async Task DetectAndEmbedConflicts(
        List<ClassSectionSubjectOfferingDto> offeringDtos,
        ClassSectionId sectionId,
        CancellationToken cancellationToken)
    {
        // Load parent section to get AcademicTermId
        var section = await _sectionReadRepository.GetByIdAsync(sectionId, cancellationToken);
        if (section == null) return;

        // Collect teacher and room IDs from all offerings
        var teacherIds = offeringDtos
            .Where(o => o.Teacher != null)
            .Select(o => o.Teacher!.Id)
            .Distinct()
            .ToList();

        var roomIds = offeringDtos
            .Where(o => o.Room != null)
            .Select(o => o.Room!.Id)
            .Distinct()
            .ToList();

        // Load this section's schedules
        var thisSectionSchedules = await _offeringRepository.GetSectionSchedulesForConflictDetectionAsync(
            sectionId.Value, cancellationToken);

        // Load related schedules for same teacher/room in same term (excluding this section)
        var relatedSchedules = new List<Core.Services.ScheduleConflictDetection.ScheduleConflictDto>();
        if (teacherIds.Any() || roomIds.Any())
        {
            relatedSchedules = await _offeringRepository.GetRelatedSchedulesForConflictDetectionAsync(
                teacherIds, roomIds, section.AcademicTermId.Value, sectionId.Value, cancellationToken);
        }

        // Detect conflicts
        var allSchedules = thisSectionSchedules.Concat(relatedSchedules).ToList();
        var domainConflicts = _conflictDetector.DetectConflicts(allSchedules, sectionId.Value);

        // Group conflicts by offering ID
        var conflictsByOffering = domainConflicts
            .SelectMany(c => c.AffectedOfferings ?? new List<AffectedOffering>(),
                (conflict, affected) => new { conflict, affected })
            .GroupBy(x => x.affected.Id)
            .ToDictionary(g => g.Key, g => g.Select(x => x.conflict).Distinct().ToList());

        // Embed conflicts in each offering DTO
        foreach (var dto in offeringDtos)
        {
            if (conflictsByOffering.TryGetValue(dto.Id, out var conflicts))
            {
                dto.Conflicts = MapConflictsToDtos(conflicts);
            }
            else
            {
                dto.Conflicts = new List<ConflictResultDto>();
            }
        }
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
