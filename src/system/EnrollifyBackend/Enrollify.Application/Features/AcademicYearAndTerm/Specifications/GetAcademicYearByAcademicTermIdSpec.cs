using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Specifications;

public class GetAcademicYearByAcademicTermIdSpec : Specification<AcademicYear>
{
    public GetAcademicYearByAcademicTermIdSpec(AcademicTermId academicTermId)
    {
        Query
            .Include(ay => ay.AcademicTerms.Where(at => at.IsActive))
            .Where(ay => ay.AcademicTerms.Any(at => at.Id == academicTermId && at.IsActive));
    }
}
