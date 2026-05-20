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

    public GetExistingClassSectionsByCourseYearLevelAndTerm(List<YearLevel> yearLevels, List<CourseId> courseIds, List<AcademicTermId> academicTermIds)
    {
        Query.Where(cs => yearLevels.Contains(cs.YearLevel) && courseIds.Contains(cs.CourseId) && academicTermIds.Contains(cs.AcademicTermId));
    }

    public GetExistingClassSectionsByCourseYearLevelAndTerm(YearLevel yearLevel, AcademicTermId academicTermId, List<CourseId> courseIds)
    {
        Query.Where(cs => cs.YearLevel == yearLevel && courseIds.Contains(cs.CourseId) && cs.AcademicTermId == academicTermId);
    }
}
