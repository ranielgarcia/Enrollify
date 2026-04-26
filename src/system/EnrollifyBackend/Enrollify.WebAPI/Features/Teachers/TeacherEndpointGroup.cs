namespace Enrollify.WebAPI.Features.Teachers;

public class TeacherEndpointGroup : Group
{
    public TeacherEndpointGroup()
    {
        Configure("teachers", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
