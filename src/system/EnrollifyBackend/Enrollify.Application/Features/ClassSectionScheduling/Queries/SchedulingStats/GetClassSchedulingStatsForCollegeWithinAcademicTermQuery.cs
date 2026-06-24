using Ardalis.Result;
using Enrollify.Application.Features.ClassSectionScheduling.Specifications.SchedulingStats;
using Enrollify.Application.Features.Courses.Specifications;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate;
using Enrollify.Core.Aggregates.CollegeAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Constants;
using Enrollify.SharedKernel;
using MediatR;

namespace Enrollify.Application.Features.ClassSectionScheduling.Queries.SchedulingStats;

public record GetClassSchedulingStatsForCollegeWithinAcademicTermQuery(CollegeId collegeId, AcademicTermId TermId)
  : IRequest<Result<Dictionary<string, int>>>;

public class GetSchedulingStatsForCollegeInAcademicTermQueryHandler
  : IRequestHandler<GetClassSchedulingStatsForCollegeWithinAcademicTermQuery, Result<Dictionary<string, int>>>
{
  private readonly IReadRepository<Course> _courseReadRepository;
  private readonly IReadRepository<ClassSectionSchedulingStats> _schedulingStatsReadRepository;

  public GetSchedulingStatsForCollegeInAcademicTermQueryHandler(
    IReadRepository<Course> courseReadRepository,
    IReadRepository<ClassSectionSchedulingStats> schedulingStatsReadRepository)
  {
    _courseReadRepository = courseReadRepository;
    _schedulingStatsReadRepository = schedulingStatsReadRepository;
  }

  public async Task<Result<Dictionary<string, int>>> Handle(
    GetClassSchedulingStatsForCollegeWithinAcademicTermQuery request,
    CancellationToken cancellationToken)
  {
    List<Course> coursesInCollege =
      await _courseReadRepository.ListAsync(new GetCoursesByCollegeIdSpec(request.collegeId), cancellationToken);

    if (!coursesInCollege.Any())
      return Result.NotFound($"No courses found for college with ID {request.collegeId.Value}.");

    var spec = new GetSpecificTypesClassSchedulingStatsForCollegeWithinAcademicTermSpec(
      coursesInCollege.Select(x => x.Id).ToList(),
      request.TermId,
      [
        ClassSectionSchedulingStatsAggregateTypeEnum.DRAFT_SECTIONS,
        ClassSectionSchedulingStatsAggregateTypeEnum.OPEN_SECTIONS,
        ClassSectionSchedulingStatsAggregateTypeEnum.CANCELLED_SECTIONS,
        ClassSectionSchedulingStatsAggregateTypeEnum.HARD_CONFLICTS,
        ClassSectionSchedulingStatsAggregateTypeEnum.SOFT_CONFLICTS,
        ClassSectionSchedulingStatsAggregateTypeEnum.DATA_INTEGRITY,
        ClassSectionSchedulingStatsAggregateTypeEnum.INFORMATIONAL
      ]);

    List<ClassSectionSchedulingStats> allStats =
      await _schedulingStatsReadRepository.ListAsync(spec, cancellationToken);

    if (!allStats.Any())
      return Result.NotFound(
        $"No scheduling stats found for college {request.collegeId.Value} in term {request.TermId.Value}.");

    var stats = allStats
      .GroupBy(x => x.AggregateType)
      .ToDictionary(g => g.Key.Name, g => g.Sum(x => x.AggregateCount));

    return Result.Success(stats);
  }
}
