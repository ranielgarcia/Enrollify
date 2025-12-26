using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.Rooms;

public static class RoomAuthorizationPolicyRegistration
{
    public static IServiceCollection AddRoomAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasViewRoomsPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasCreateRoomPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasUpdateRoomPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasDeleteRoomPermissionHandler>();

        return services; 
    }

    public static void AddRoomsAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasViewRoomsPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasViewRoomsPermission()));

        options.AddPolicy(PolicyName.HasCreateRoomPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasCreateRoomPermission()));

        options.AddPolicy(PolicyName.HasUpdateRoomPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasUpdateRoomPermission()));

        options.AddPolicy(PolicyName.HasDeleteRoomPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasDeleteRoomPermission()));
    }
}
