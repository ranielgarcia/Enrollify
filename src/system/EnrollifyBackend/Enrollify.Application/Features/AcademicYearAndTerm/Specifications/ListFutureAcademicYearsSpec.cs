using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Specifications;

public class ListFutureAcademicYearsSpec : Specification<AcademicYear>
{
    public ListFutureAcademicYearsSpec(DateTime dateReference)
    {
        // Academic years that haven't started yet
        Query.Include(ay => ay.AcademicTerms.Where(at => at.IsActive))
             .Where(ay => (DateTime)ay.StartDate > dateReference)
             .OrderBy(ay => (DateTime)ay.StartDate);
    }
}
