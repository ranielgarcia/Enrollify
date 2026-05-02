using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Ardalis.Specification;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Specifications;

public class GetOverlappingAcademicYearByDateRangeSpec : Specification<AcademicYear>
{
    public GetOverlappingAcademicYearByDateRangeSpec(AcademicYearStartDate startDate, AcademicYearEndDate endDate, AcademicYearId? excludeYearId = null)
    {
        Query.Where(ay =>
                    ay.StartDate.Value < endDate.Value
                    && ay.EndDate.Value > startDate.Value);

        if (excludeYearId.HasValue)
        {
            Query.Where(ay => ay.Id != excludeYearId.Value);
        }
    }
}
