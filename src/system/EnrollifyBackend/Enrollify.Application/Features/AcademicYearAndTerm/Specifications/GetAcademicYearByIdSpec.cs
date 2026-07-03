namespace Enrollify.Application.Features.AcademicYearAndTerm.Specifications;

public class GetAcademicYearByIdSpec : Specification<AcademicYear>
{
    public GetAcademicYearByIdSpec(AcademicYearId id)
    {
        Query.Include(ay => ay.AcademicTerms.Where(at => at.IsActive))
             .Where(ay => ay.Id == id);
    }
}
