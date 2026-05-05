using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Specifications;

public class GetActiveAcademicYearSpec : Specification<AcademicYear>
{
    public GetActiveAcademicYearSpec()
    {
        var today = DateTime.UtcNow.Date;
        Query
            .Include(ay => ay.AcademicTerms.Where(at => at.IsActive))
            .Where(ay => (DateTime)ay.StartDate <= today && (DateTime)ay.EndDate >= today);
    }
}
