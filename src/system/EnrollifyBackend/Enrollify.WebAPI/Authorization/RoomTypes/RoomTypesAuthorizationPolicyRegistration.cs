using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.RoomTypes;

public static class RoomTypesAuthorizationPolicyRegistration
{
    public static IServiceCollection AddRoomTypesAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasCreateRoomTypePermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasViewRoomTypesPermissionHandler>();

        return services;
    }

    public static void AddRoomTypesAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasCreateRoomTypePermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasCreateRoomTypePermission()));

        options.AddPolicy(PolicyName.HasViewRoomTypesPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasViewRoomTypesPermission()));
    }
}
