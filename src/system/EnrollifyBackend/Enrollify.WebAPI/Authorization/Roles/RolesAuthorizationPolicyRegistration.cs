using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.Roles;

public static class RolesAuthorizationPolicyRegistration
{
    public static IServiceCollection AddRolesAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, View.HasViewRolesPermissionHandler>();
        return services;
    }

    public static void AddRolesAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasViewRolesPermission, policyBuilder =>
            policyBuilder.AddRequirements(new View.HasViewRolesPermission()));
    }
}
