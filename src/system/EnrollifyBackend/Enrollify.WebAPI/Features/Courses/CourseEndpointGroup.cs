namespace Enrollify.WebAPI.Features.Courses;

public class CourseEndpointGroup : Group
{
    public CourseEndpointGroup()
    {
        Configure("courses", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
