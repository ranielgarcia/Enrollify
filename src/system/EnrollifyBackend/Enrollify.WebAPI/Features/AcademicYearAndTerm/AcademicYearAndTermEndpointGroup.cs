namespace Enrollify.WebAPI.Features.AcademicYearAndTerm;

public class AcademicYearAndTermEndpointGroup : Group
{
    public AcademicYearAndTermEndpointGroup()
    {
        Configure("academic-years", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
