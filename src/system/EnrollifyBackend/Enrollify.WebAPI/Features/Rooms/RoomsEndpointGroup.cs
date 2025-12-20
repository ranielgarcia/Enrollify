using FastEndpoints;

namespace Enrollify.WebAPI.Features.Rooms;

public class RoomsEndpointGroup : Group
{
    public RoomsEndpointGroup()
    {
        Configure("rooms", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
