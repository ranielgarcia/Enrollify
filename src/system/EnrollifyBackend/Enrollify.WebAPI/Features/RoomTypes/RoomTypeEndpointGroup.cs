using FastEndpoints;

namespace Enrollify.WebAPI.Features.RoomTypes;

public class RoomTypeEndpointGroup : Group
{
    public RoomTypeEndpointGroup()
    {
        Configure("room-types", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
