
using FastEndpoints;

namespace Enrollify.WebAPI.Features.Users;

public class UserEndpointsGroup : Group
{
    public UserEndpointsGroup()
    {
        Configure("users", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
