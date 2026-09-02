using Enrollify.Application.Features.ClassSectionScheduling.DTOs;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.SchedulingStats;
using Enrollify.Application.Filtering;
using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Queries.ClassSections;

public record FilterClassSectionsPaginatedQuery(
  int Page = 1,
  int PageSize = 10,
  int? AcademicYearId = null,
  IEnumerable<FilterItem>? Filters = null,
  IEnumerable<SortItem>? Sorts = null,
  string? JoinOperator = null
) : IRequest<Result<PagedResult<ClassSectionDto>>>;

public class FilterClassSectionsPaginatedQueryHandler
  : IRequestHandler<FilterClassSectionsPaginatedQuery, Result<PagedResult<ClassSectionDto>>>
{
  private readonly IReadRepository<ClassSection> _readRepository;
  private readonly IReadRepository<AcademicYear> _academicYearRepository;
  private readonly IReadRepository<ClassSectionSchedulingStats> _schedulingStatsReadRepository;

  public FilterClassSectionsPaginatedQueryHandler(
    IReadRepository<ClassSection> readRepository,
    IReadRepository<AcademicYear> academicYearRepository,
    IReadRepository<ClassSectionSchedulingStats> schedulingStatsReadRepository)
  {
    _readRepository = readRepository;
    _academicYearRepository = academicYearRepository;
    _schedulingStatsReadRepository = schedulingStatsReadRepository;
  }

  public async Task<Result<PagedResult<ClassSectionDto>>> Handle(
    FilterClassSectionsPaginatedQuery request,
    CancellationToken cancellationToken)
  {
    JoinOperator joinOperator = Enum.TryParse<JoinOperator>(request.JoinOperator, true, out JoinOperator parsed)
      ? parsed
      : JoinOperator.Or;

    IEnumerable<int>? academicTermIds = null;
    if (request.AcademicYearId.HasValue)
    {
      var academicYearId = AcademicYearId.From(request.AcademicYearId.Value);
      AcademicYear? academicYear = await _academicYearRepository.FirstOrDefaultAsync(
        new GetAcademicYearByIdSpec(academicYearId), cancellationToken);

      if (academicYear is null)
        return Result.NotFound($"Academic year with id {request.AcademicYearId} was not found.");

      academicTermIds = academicYear.AcademicTerms.Select(t => t.Id.Value);
    }

    var spec = new FilterClassSectionsPaginatedSpec(
      request.Page,
      request.PageSize,
      academicTermIds,
      request.Filters,
      request.Sorts,
      joinOperator);

    List<ClassSection> sections = await _readRepository.ListAsync(spec, cancellationToken);
    int totalCount = await _readRepository.CountAsync(spec, cancellationToken);

    var schedulingStatsSpec = new GetClassSchedulingStatsForClassSectionIdsSpec(sections.Select(s => s.Id).ToList());
    List<ClassSectionSchedulingStats> schedulingStats =
      await _schedulingStatsReadRepository.ListAsync(schedulingStatsSpec, cancellationToken);

    var schedulingStatsPerClassSection = new Dictionary<ClassSectionId, List<ClassSectionSchedulingStats>>();
    foreach (ClassSectionSchedulingStats schedulingStat in schedulingStats)
    {
      if (!schedulingStat.ClassSectionId.HasValue)
        continue;

      ClassSectionId classSectionId = schedulingStat.ClassSectionId.Value;
      if (!schedulingStatsPerClassSection.TryGetValue(classSectionId,
            out List<ClassSectionSchedulingStats>? statsForSection))
      {
        statsForSection = [];
        schedulingStatsPerClassSection[classSectionId] = statsForSection;
      }

      statsForSection.Add(schedulingStat);
    }

    var sectionsToReturn = new List<ClassSectionDto>();
    foreach (ClassSection section in sections)
    {
      List<ClassSectionSchedulingStats> sectionSchedulingStats =
        schedulingStatsPerClassSection.TryGetValue(section.Id, out List<ClassSectionSchedulingStats>? stats)
          ? stats
          : new List<ClassSectionSchedulingStats>();

      var sectionDto = ClassSectionDto.FromEntity(section, sectionSchedulingStats);
      sectionsToReturn.Add(sectionDto);
    }

    return new PagedResult<ClassSectionDto>(
      sectionsToReturn.AsReadOnly(),
      request.Page,
      request.PageSize,
      totalCount);
  }
}
