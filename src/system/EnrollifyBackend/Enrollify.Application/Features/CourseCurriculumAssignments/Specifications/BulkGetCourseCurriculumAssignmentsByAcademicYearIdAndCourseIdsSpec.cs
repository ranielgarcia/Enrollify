using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;

namespace Enrollify.Application.Features.CourseCurriculumAssignments.Specifications;

public class BulkGetCourseCurriculumAssignmentsByAcademicYearIdAndCourseIdsSpec : Specification<CourseCurriculumAssignment>
{
    public BulkGetCourseCurriculumAssignmentsByAcademicYearIdAndCourseIdsSpec(
        AcademicYearId academicYearId,
        List<CourseId> courseIds)
    {
        var distinctIds = courseIds.Distinct().ToList();
        Query.Where(a => a.EntryAcademicYearId == academicYearId && distinctIds.Contains(a.CourseId));
    }
}
