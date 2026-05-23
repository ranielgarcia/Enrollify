using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.Aggregates.CurriculumAggregate;

namespace Enrollify.Core.Services;

public interface IApplicableCurriculumQueryService
{
    Task<IReadOnlyList<Curriculum>> GetApplicableCurriculumsForAcademicYearStartDateAsync
        (AcademicYearStartDate academicYearStartDate, CancellationToken cancellationToken);
}
