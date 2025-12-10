using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.RoomTypes;

public static class RoomTypesAuthorizationPolicyRegistration
{
    public static IServiceCollection AddRoomTypesAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, Create.HasCreateRoomTypePermissionHandler>();

        return services;
    }

    public static void AddRoomTypesAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasCreateRoomTypePermission, policyBuilder =>
            policyBuilder.AddRequirements(new Create.HasCreateRoomTypePermission()));
    }
}
