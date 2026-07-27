using System.Collections.Concurrent;
using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Models;
using Enrollify.Core.Services.ClassSectionDataIntegrityValidation;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionValidationIssues;

public class ComputeAndGetDataQualityValidationIssuesForClassSections
{
  public sealed record Command(
    List<ClassSectionId> ClassSectionIds) : IRequest<Result<List<ClassSectionValidationIssueDto>>>;

  public sealed class Handler : IRequestHandler<Command, Result<List<ClassSectionValidationIssueDto>>>
  {
    private readonly IReadRepository<ClassSectionSubjectOffering> _offeringReadRepository;
    private readonly ClassSectionDataIntegrityValidator _dataIntegrityValidator;
    private readonly IReadRepository<ClassSection> _classSectionReadRepository;
    private readonly IClassSectionValidationIssueRepository _validationIssueRepository;
    private readonly IPublisher _publisher;
    private readonly ILogger<Handler> _logger;

    public Handler(
      IReadRepository<ClassSectionSubjectOffering> offeringReadRepository,
      ClassSectionDataIntegrityValidator dataIntegrityValidator,
      IReadRepository<ClassSection> classSectionReadRepository,
      IClassSectionValidationIssueRepository validationIssueRepository,
      IPublisher publisher,
      ILogger<Handler> logger)
    {
      _offeringReadRepository = offeringReadRepository;
      _dataIntegrityValidator = dataIntegrityValidator;
      _classSectionReadRepository = classSectionReadRepository;
      _validationIssueRepository = validationIssueRepository;
      _publisher = publisher;
      _logger = logger;
    }

    public async Task<Result<List<ClassSectionValidationIssueDto>>> Handle(Command request,
      CancellationToken cancellationToken)
    {
      List<ClassSection> sections =
        await _classSectionReadRepository.ListAsync(new GetClassSectionFullDetailsByIdSpec(request.ClassSectionIds),
          cancellationToken);
      if (!sections.Any())
      {
        _logger.LogWarning(
          "ComputeAndGetDataIntegrityValidationIssuesForClassSections: No class sections found for IDs {ClassSectionIds} — skipping",
          string.Join(", ", request.ClassSectionIds.Select(id => id.Value)));
        return Result.NotFound("No class sections found for the provided IDs.");
      }

      List<ClassSectionSubjectOffering> offerings = await _offeringReadRepository.ListAsync(
        new GetClassSectionSubjectOfferingsByClassSectionIdSpec(request.ClassSectionIds), cancellationToken);

      Dictionary<ClassSectionId, List<ClassSectionSubjectOffering>> offeringsBySectionId =
        offerings.GroupBy(o => o.ClassSectionId)
          .ToDictionary(g => g.Key, g => g.ToList());

      ConcurrentBag<ClassSectionValidationIssue> allDataIntegrityValidationIssues = new();

      Parallel.ForEach(sections, section =>
      {
        offeringsBySectionId.TryGetValue(section.Id, out List<ClassSectionSubjectOffering>? offeringsForSection);

        List<ClassSectionDataIntegrityResult> results =
          _dataIntegrityValidator.Validate(section, offeringsForSection ?? []);

        foreach (ClassSectionDataIntegrityResult result in results)
        {
          allDataIntegrityValidationIssues.Add(
            new ClassSectionValidationIssue(
              section.CourseId,
              section.AcademicTermId,
              section.Id,
              result));
        }
      });

      IEnumerable<ClassSectionValidationIssueTypeEnum> dataQualityValidationIssueTypes =
        ClassSectionValidationIssueTypeEnum.List.Where(x =>
          x.Category == ClassSectionValidationIssueCategoryEnum.MISSING_REQUIREMENT ||
          x.Category == ClassSectionValidationIssueCategoryEnum.DATA_INCONSISTENCY ||
          x.Category == ClassSectionValidationIssueCategoryEnum.DEFAULT_VALUE).ToList();

      var validationIssuesPerClassSection = allDataIntegrityValidationIssues.GroupBy(x => x.ClassSectionId)
        .ToDictionary(g => g.Key, g => g.ToList());
      foreach (var section in sections)
      {
        validationIssuesPerClassSection.TryGetValue(section.Id, out List<ClassSectionValidationIssue>? validationIssuesForCurrentSection);
        await _validationIssueRepository.ReplaceAllSpecificIssuesForSectionAsync(section.Id, validationIssuesForCurrentSection ?? [], dataQualityValidationIssueTypes, cancellationToken);

        await _publisher.Publish(
          new RefreshClassSectionSchedulingStatsAggregateCountsRequestedEvent(section.AcademicTermId, section.CourseId,
            section.Id), cancellationToken);

        _logger.LogInformation(
          "Computed {DataQualityIssueCount} data quality validation issues for class section {ClassSectionId}",
          validationIssuesForCurrentSection?.Count ?? 0, section.Id.Value);
      }

      return Result.Success(allDataIntegrityValidationIssues.Select(ClassSectionValidationIssueDto.FromEntity).ToList());
    }
  }
}
