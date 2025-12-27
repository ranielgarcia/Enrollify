namespace Enrollify.WebAPI.Features.Colleges;

public class CollegeEndpointsGroup : Group
{
    public CollegeEndpointsGroup()
    {
        Configure("colleges", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
