using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;

namespace Enrollify.Application.Features.ClassSectionScheduling.Repositories;

public interface IClassSectionSchedulingStatsRepository
{
  Task CalculateDraftSectionsForCourse(CourseId courseId, CancellationToken cancellationToken);

  Task CalculateOpenSectionsForCourse(CourseId courseId, CancellationToken cancellationToken);

  Task CalculateCancelledSectionsForCourse(CourseId courseId, CancellationToken cancellationToken);

  Task CalculateHardConflictIssuesForClassSection

}
