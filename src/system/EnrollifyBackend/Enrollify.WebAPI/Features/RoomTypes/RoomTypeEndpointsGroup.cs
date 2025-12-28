namespace Enrollify.WebAPI.Features.RoomTypes;

public class RoomTypeEndpointsGroup : Group
{
    public RoomTypeEndpointsGroup()
    {
        Configure("room-types", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
