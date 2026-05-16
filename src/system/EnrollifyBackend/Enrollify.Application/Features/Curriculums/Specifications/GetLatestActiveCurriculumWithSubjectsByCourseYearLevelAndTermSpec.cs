using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CourseAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Constants;
using Enrollify.Core.ValueObjects;

namespace Enrollify.Application.Features.Curriculums.Specifications;

public class GetLatestActiveCurriculumWithSubjectsByCourseYearLevelAndTermSpec : Specification<Curriculum>
{
    // We can have multiple active curriculums for a course, but we only want the latest one based on the effective year.
    // This is to ensure that if there are multiple active curriculums for a course, we always get the most recent one.
    public GetLatestActiveCurriculumWithSubjectsByCourseYearLevelAndTermSpec(CourseId course, YearLevel yearLevel, TermNumber termNumber)
    {
        Query
            .Include(c => c.CurriculumSubjects.Where(cs => cs.IsActive && cs.YearLevel == yearLevel && cs.TermNumber == termNumber))
            .Where(c => c.CourseId == course && c.IsActive && c.StatusId == CurriculumStatusEnum.Active)
            .OrderByDescending(c => c.EffectiveYear)
            .Take(1);
    }
}
