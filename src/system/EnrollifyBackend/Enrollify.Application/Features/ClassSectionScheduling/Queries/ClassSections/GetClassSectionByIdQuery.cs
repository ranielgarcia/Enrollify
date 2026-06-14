using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Application.Features.ClassSectionScheduling.Services;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSections;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSubjectOfferingAggregate;
using Enrollify.Core.Aggregates.RoomAggregate;
using Enrollify.Core.Aggregates.TeacherAggregate;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.ClassSectionScheduling.Queries.ClassSections;

public record GetClassSectionByIdQuery(ClassSectionId Id) : IRequest<Result<ClassSectionDetailDto>>;

public class GetClassSectionByIdQueryHandler
  : IRequestHandler<GetClassSectionByIdQuery, Result<ClassSectionDetailDto>>
{
  private readonly IReadRepository<ClassSection> _classSectionReadRepository;
  private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
  private readonly IReadRepository<ClassSectionEnrollmentEligibilityValidationMessage> _validationMessageRepository;
  private readonly ConflictDetectionHelper _conflictDetectionHelper;

  public GetClassSectionByIdQueryHandler(
    IReadRepository<ClassSection> classSectionReadRepository,
    IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
    IReadRepository<ClassSectionEnrollmentEligibilityValidationMessage> validationMessageRepository,
    ConflictDetectionHelper conflictDetectionHelper)
  {
    _classSectionReadRepository = classSectionReadRepository;
    _offeringReadRepository = offeringReadRepository;
    _validationMessageRepository = validationMessageRepository;
    _conflictDetectionHelper = conflictDetectionHelper;
  }

  public async Task<Result<ClassSectionDetailDto>> Handle(
    GetClassSectionByIdQuery request,
    CancellationToken cancellationToken)
  {
    ClassSection? section = await _classSectionReadRepository.FirstOrDefaultAsync(
      new GetClassSectionFullDetailsByIdSpec(request.Id), cancellationToken);

    if (section is null)
      return Result.NotFound($"Class section with id {request.Id.Value} was not found.");

    List<ClassSectionSubjectOffering> offerings = await _offeringReadRepository.ListAsync(
      new GetClassSectionSubjectOfferingsWithFullDetailsByClassSectionIdSpec(request.Id), cancellationToken);

    List<ClassSectionEnrollmentEligibilityValidationMessage> allValidationMessagesForSection =
      await _validationMessageRepository
        .ListAsync(new GetClassSectionEnrollmentEligibilityValidationMessagesSpec(section.Id), cancellationToken);

    // Convert offerings to DTOs
    var offeringDtos = offerings.Select(ClassSectionSubjectOfferingDto.FromEntity).ToList();

    // Detect and embed conflicts in offering DTOs
    await DetectAndEmbedConflicts(offeringDtos, section, cancellationToken);

    return Result.Success(ClassSectionDetailDto.FromEntities(section, allValidationMessagesForSection, offeringDtos));
  }

  /// <summary>
  /// Detects conflicts for all offerings in the section and embeds them in the DTOs.
  /// This provides the single source of truth for conflict information on the section detail page.
  /// </summary>
  private async Task DetectAndEmbedConflicts(
    List<ClassSectionSubjectOfferingDto> offeringDtos,
    ClassSection section,
    CancellationToken cancellationToken)
  {
    if (!offeringDtos.Any()) return;

    // Collect teacher and room IDs from all offerings
    var teacherIds = offeringDtos.Where(o => o.Teacher != null).Select(o => TeacherId.From(o.Teacher!.Id)).ToList();
    var roomIds = offeringDtos.Where(o => o.Room != null).Select(o => RoomId.From(o.Room!.Id)).ToList();

    // Use helper to detect conflicts
    Dictionary<ClassSectionSubjectOfferingId, List<ConflictResultDto>> conflictsByOffering =
      await _conflictDetectionHelper.DetectConflictsForSectionAsync(
        section, teacherIds, roomIds, cancellationToken);

    // Embed conflicts in each offering DTO
    foreach (ClassSectionSubjectOfferingDto dto in offeringDtos)
      dto.Conflicts = conflictsByOffering.TryGetValue(ClassSectionSubjectOfferingId.From(dto.Id), out List<ConflictResultDto>? conflicts)
        ? conflicts
        : new List<ConflictResultDto>();
  }
}
