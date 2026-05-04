using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Specifications;

public class ListPreviousAcademicYearsSpec : Specification<AcademicYear>
{
    public ListPreviousAcademicYearsSpec()
    {
        var today = DateTime.UtcNow.Date;
        Query.Include(ay => ay.AcademicTerms.Where(at => at.IsActive))
             .Where(ay => ay.EndDate.Value < today)
             .OrderByDescending(ay => ay.StartDate.Value);
    }
}
