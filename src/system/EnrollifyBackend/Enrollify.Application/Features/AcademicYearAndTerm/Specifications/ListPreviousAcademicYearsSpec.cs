namespace Enrollify.Application.Features.AcademicYearAndTerm.Specifications;

public class ListPreviousAcademicYearsSpec : Specification<AcademicYear>
{
    public ListPreviousAcademicYearsSpec(int numberOfPreviousYears)
    {
        var today = DateTime.UtcNow.Date;
        Query.Include(ay => ay.AcademicTerms.Where(at => at.IsActive))
             .Where(ay => (DateTime)ay.EndDate < today)
             .OrderByDescending(ay => (DateTime)ay.StartDate)
             .Take(numberOfPreviousYears);
    }
}
