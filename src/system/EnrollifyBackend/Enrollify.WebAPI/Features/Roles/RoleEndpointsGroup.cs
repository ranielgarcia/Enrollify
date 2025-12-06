using FastEndpoints;

namespace Enrollify.WebAPI.Features.Roles;

public class RoleEndpointsGroup : Group
{
    public RoleEndpointsGroup()
    {
        Configure("roles", ep =>
        {
            ep.Description(x => x.Produces(401));
        });
    }
}
