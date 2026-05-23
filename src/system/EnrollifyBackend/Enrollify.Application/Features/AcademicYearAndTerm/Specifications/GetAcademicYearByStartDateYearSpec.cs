using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;
using Enrollify.Core.ValueObjects;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Specifications;

public class GetAcademicYearByStartDateYearSpec : Specification<AcademicYear>
{
    public GetAcademicYearByStartDateYearSpec(Year year)
    {
        Query
            .Include(x => x.AcademicTerms.Where(t => t.IsActive))
            .Where(x => ((DateTime)x.StartDate).Year <= ((int)year.Value))
            .OrderByDescending(x => x.StartYear)
            .Take(1);
    }
}
