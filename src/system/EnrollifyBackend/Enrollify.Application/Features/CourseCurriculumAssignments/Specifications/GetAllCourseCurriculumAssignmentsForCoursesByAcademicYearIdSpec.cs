using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;

namespace Enrollify.Application.Features.CourseCurriculumAssignments.Specifications;

public class GetAllCourseCurriculumAssignmentsForCoursesByAcademicYearIdSpec : Specification<CourseCurriculumAssignment>
{
  public GetAllCourseCurriculumAssignmentsForCoursesByAcademicYearIdSpec(List<CourseId> courseIds,
    AcademicYearId academicYearId)
  {
    // NOTE: Next you encounter an issue related to adding AsSplitQuery, check all related query entities EF IEntityTypeConfiguration,
    // They are not configured properly, e.g. a nullable column is marked as required in the configuration
    Query
      .Include(a => a.Course)
      .Include(a => a.Curriculum)
      .ThenInclude(a => a.CurriculumSubjects.Where(x => x.IsActive))
      .ThenInclude(cs => cs.Subject)
      .AsSplitQuery()
      .Where(a => courseIds.Contains(a.CourseId) && a.EntryAcademicYearId == academicYearId);
  }
}
