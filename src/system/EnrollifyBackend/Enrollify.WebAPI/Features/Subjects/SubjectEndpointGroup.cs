namespace Enrollify.WebAPI.Features.Subjects;

public class SubjectEndpointGroup : Group
{
    public SubjectEndpointGroup()
    {
        Configure("subjects", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
