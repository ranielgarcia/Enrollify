using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.RoomTypes;

public static class RoomTypesAuthorizationPolicyRegistration
{
    public static IServiceCollection AddRoomTypesAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasCreateRoomTypePermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasViewRoomTypesPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasUpdateRoomTypePermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasDeleteRoomTypePermissionHandler>();

        return services;
    }

    public static void AddRoomTypesAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasCreateRoomTypePermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasCreateRoomTypePermission()));
        options.AddPolicy(PolicyName.HasUpdateRoomTypesPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasUpdateRoomTypePermission()));
        options.AddPolicy(PolicyName.HasDeleteRoomTypesPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasDeleteRoomTypePermission()));
        options.AddPolicy(PolicyName.HasViewRoomTypesPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasViewRoomTypePermission()));
    }
}
