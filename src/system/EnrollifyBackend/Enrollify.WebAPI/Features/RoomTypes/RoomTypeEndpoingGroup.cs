using FastEndpoints;

namespace Enrollify.WebAPI.Features.RoomTypes;

public class RoomTypeEndpoingGroup : Group
{
    public RoomTypeEndpoingGroup()
    {
        Configure("room-types", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
