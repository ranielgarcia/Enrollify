using Ardalis.Specification;
using Enrollify.Core.Aggregates.AcademicYearAggregate;

namespace Enrollify.Application.Features.AcademicYearAndTerm.Specifications;

public class BulkGetAcademicYearsByIdsSpec : Specification<AcademicYear>
{
    public BulkGetAcademicYearsByIdsSpec(List<AcademicYearId> academicYearIds)
    {
        var distinctIds = academicYearIds.Distinct().ToList();
        Query.Include(ay => ay.AcademicTerms.Where(at => at.IsActive))
            .Where(ay => distinctIds.Contains(ay.Id));
    }
}
