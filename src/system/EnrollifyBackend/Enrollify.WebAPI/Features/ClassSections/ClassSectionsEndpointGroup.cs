namespace Enrollify.WebAPI.Features.ClassSections;

public class ClassSectionsEndpointGroup : Group
{
    public ClassSectionsEndpointGroup()
    {
        Configure("class-sections", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
