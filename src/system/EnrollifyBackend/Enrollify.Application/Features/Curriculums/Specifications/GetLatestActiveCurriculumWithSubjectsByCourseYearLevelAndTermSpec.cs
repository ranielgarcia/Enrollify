using Enrollify.Core.ValueObjects;

namespace Enrollify.Application.Features.Curriculums.Specifications;

public class GetLatestActiveCurriculumWithSubjectsByCourseYearLevelAndTermSpec : Specification<Curriculum>
{
    // We can have multiple active curriculums for a course, but we only want the latest one based on the effective year.
    // This is to ensure that if there are multiple active curriculums for a course, we always get the most recent one.
    public GetLatestActiveCurriculumWithSubjectsByCourseYearLevelAndTermSpec(CourseId course, YearLevel yearLevel, TermNumber termNumber, AcademicYearStartDate academicYearStartDate)
    {
        var academicYearStartYear = academicYearStartDate.Value.Year;
        Query
            .Include(c => c.CurriculumSubjects.Where(cs => cs.IsActive && cs.YearLevel == yearLevel && cs.TermNumber == termNumber))
            .Where(c => c.CourseId == course && c.IsActive && c.StatusId == CurriculumStatusEnum.Active)
            .Where(c => c.EffectiveYear.Value <= academicYearStartYear)
            .OrderByDescending(c => c.EffectiveYear)
            .Take(1);
    }
}
