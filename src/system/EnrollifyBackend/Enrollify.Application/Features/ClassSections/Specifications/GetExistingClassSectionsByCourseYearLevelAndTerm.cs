using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.ClassSectionAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.ValueObjects;

namespace Enrollify.Application.Features.ClassSections.Specifications;

public class GetExistingClassSectionsByCourseYearLevelAndTerm : Specification<ClassSection>
{
    public GetExistingClassSectionsByCourseYearLevelAndTerm(YearLevel yearLevel, CourseId courseId, AcademicTermId academicTermId)
    {
        Query.Where(cs => cs.YearLevel == yearLevel && cs.CourseId == courseId && cs.AcademicTermId == academicTermId)
            .OrderBy(cs => cs.SectionCode);
    }
}
