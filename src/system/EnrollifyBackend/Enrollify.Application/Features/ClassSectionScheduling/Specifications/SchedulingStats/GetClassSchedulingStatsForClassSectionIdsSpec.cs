using Ardalis.Specification;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.ClassSectionSchedulingStatsAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Specifications.SchedulingStats;

public class GetClassSchedulingStatsForClassSectionIdsSpec : Specification<ClassSectionSchedulingStats>
{
  public GetClassSchedulingStatsForClassSectionIdsSpec(List<ClassSectionId> classSectionIds)
  {
    Query
      .Where(x => x.ClassSectionId != null && classSectionIds.Contains(x.ClassSectionId.Value));
  }
}
