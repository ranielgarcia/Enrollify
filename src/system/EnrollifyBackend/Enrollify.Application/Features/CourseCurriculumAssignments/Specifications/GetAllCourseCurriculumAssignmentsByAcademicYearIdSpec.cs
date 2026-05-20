using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseCurriculumAssignmentAggregate;

namespace Enrollify.Application.Features.CourseCurriculumAssignments.Specifications;

public class GetAllCourseCurriculumAssignmentsByAcademicYearIdSpec : Specification<CourseCurriculumAssignment>
{
    public GetAllCourseCurriculumAssignmentsByAcademicYearIdSpec(AcademicYearId academicYearId)
    {
        Query
            .Include(a => a.Course)
            .Include(a => a.Curriculum).ThenInclude(a => a.CurriculumSubjects.Where(x => x.IsActive))
            .AsSplitQuery()
            .Where(a => a.EntryAcademicYearId == academicYearId);
    }
}
