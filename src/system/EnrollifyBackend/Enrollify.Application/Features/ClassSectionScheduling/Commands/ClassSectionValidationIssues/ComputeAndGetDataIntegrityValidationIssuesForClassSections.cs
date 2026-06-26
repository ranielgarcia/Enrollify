using System.Collections.Concurrent;
using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.ClassSectionSubjectOfferings;
using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate.Events;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate;
using Enrollify.Core.Aggregates.ClassSectionValidationIssueAggregate.Models;
using Enrollify.Core.Services.ClassSectionDataIntegrityValidation;

namespace Enrollify.Application.Features.ClassSectionScheduling.Commands.ClassSectionValidationIssues;

public class ComputeAndGetDataIntegrityValidationIssuesForClassSections
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

      ConcurrentBag<ClassSectionValidationIssue> allDataIntegrityValidationIssues = new();

      Parallel.ForEach(sections, section =>
      {
        List<ClassSectionDataIntegrityResult> results =
          _dataIntegrityValidator.Validate(section, offerings);

        foreach (var result in results)
        {
          allDataIntegrityValidationIssues.Add(
            new ClassSectionValidationIssue(
              section.CourseId,
              section.AcademicTermId,
              section.Id,
              result));
        }
      });

      var dataIntegrityValidationIssueTypes =
        ClassSectionValidationIssueTypeEnum.List.Where(x =>
          x.Tier == ClassSectionValidationIssueTierEnum.DATA_INTEGRITY);

      var validationIssuesPerClassSection = allDataIntegrityValidationIssues.GroupBy(x => x.ClassSectionId)
        .ToDictionary(g => g.Key, g => g.ToList());
      foreach (var section in sections)
      {
        validationIssuesPerClassSection.TryGetValue(section.Id, out List<ClassSectionValidationIssue> validationIssuesForCurrentSection);
        await _validationIssueRepository.ReplaceAllSpecificIssuesForSectionAsync(section.Id, validationIssuesForCurrentSection, dataIntegrityValidationIssueTypes, cancellationToken);

        await _publisher.Publish(
          new RefreshClassSectionSchedulingStatsAggregateCountsRequestedEvent(section.AcademicTermId, section.CourseId,
            section.Id), cancellationToken);

        _logger.LogInformation(
          "Computed {DataIntegrityIssueCount} data integrity validation issues for class section {ClassSectionId}",
          validationIssuesForCurrentSection.Count, section.Id.Value);
      }

      return Result.Success(allDataIntegrityValidationIssues.Select(ClassSectionValidationIssueDto.FromEntity).ToList());
    }
  }
}
