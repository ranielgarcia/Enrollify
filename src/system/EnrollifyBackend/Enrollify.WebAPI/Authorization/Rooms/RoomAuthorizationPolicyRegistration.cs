using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.Rooms;

public static class RoomAuthorizationPolicyRegistration
{
    public static IServiceCollection AddRoomAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasViewRoomsPermissionHandler>();

        return services; 
    }

    public static void AddRoomsAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasViewRoomsPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasViewRoomsPermission()));
    }
}
