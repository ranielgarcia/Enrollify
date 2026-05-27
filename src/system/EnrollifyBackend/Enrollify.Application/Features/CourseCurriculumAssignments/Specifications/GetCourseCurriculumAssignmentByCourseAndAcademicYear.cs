using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;

namespace Enrollify.Application.Features.CourseCurriculumAssignments.Specifications;

public class GetCourseCurriculumAssignmentByCourseAndAcademicYear : Specification<CourseCurriculumAssignment>
{
    public GetCourseCurriculumAssignmentByCourseAndAcademicYear(CourseId courseId, AcademicYearId academicYearId)
    {
        Query
            .Include(a => a.Course)
            .Include(a => a.Curriculum)
                .ThenInclude(x => x.CurriculumSubjects.Where(cs => cs.IsActive))
                .ThenInclude(cs => cs.Subject)
            .Where(a => a.EntryAcademicYearId == academicYearId && a.CourseId == courseId);
    }
}
