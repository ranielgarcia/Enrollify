namespace Enrollify.Application.Features.AcademicYearAndTerm.Specifications;

public class GetOverlappingAcademicYearByDateRangeSpec : Specification<AcademicYear>
{
    public GetOverlappingAcademicYearByDateRangeSpec(AcademicYearStartDate startDate, AcademicYearEndDate endDate, AcademicYearId? excludeYearId = null)
    {
        Query
            .Where(ay =>
                    (DateTime)ay.StartDate < (DateTime)endDate
                    && (DateTime)ay.EndDate > (DateTime)startDate);

        if (excludeYearId.HasValue)
        {
            Query.Where(ay => ay.Id != excludeYearId.Value);
        }
    }
}
