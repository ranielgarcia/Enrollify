namespace Enrollify.WebAPI.Features.Buildings;

public class BuildingEndpointGroup : Group
{
    public BuildingEndpointGroup()
    {
        Configure("buildings", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
