using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;
using Enrollify.Core.Constants;

namespace Enrollify.Application.Features.Curriculums.Specifications;

public class GetCurriculumsApplicableToAcademicYearStartYearSpec : Specification<Curriculum>
{
    public GetCurriculumsApplicableToAcademicYearStartYearSpec(AcademicYearStartDate academicYearStartDate)
    {
        var academicYearStartYear = academicYearStartDate.Value.Year;

        Query
            .Where(c => c.StatusId == CurriculumStatusEnum.Active)
            .Where(c => c.EffectiveYear.Value <= academicYearStartYear);
    }
}
