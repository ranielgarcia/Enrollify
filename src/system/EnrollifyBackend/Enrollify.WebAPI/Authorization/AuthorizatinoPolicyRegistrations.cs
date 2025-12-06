using Enrollify.WebAPI.Authorization.Roles;
using Enrollify.WebAPI.Authorization.Shared;
using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization;

public static class AuthorizatinoPolicyRegistrations
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasAnyValidRoleHandler>();
        services.AddRolesAuthorizationPolicyHandlers();

        services.AddAuthorization(options =>
        {
            // shared policies
            options.AddPolicy(PolicyName.HasAnyValidRoleAndPermission, policyBuilder =>
                policyBuilder.AddRequirements(new HasAnyValidRole()));

            options.AddRolesAuthorizationPolicies();
        });

        return services;
    }
}
