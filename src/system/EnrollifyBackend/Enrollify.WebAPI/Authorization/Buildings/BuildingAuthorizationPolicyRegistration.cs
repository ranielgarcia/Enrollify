using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.Buildings;

public static class BuildingAuthorizationPolicyRegistration
{
    public static IServiceCollection AddBuildingsAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped< IAuthorizationHandler, HasCreateBuildingPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasUpdateBuildingPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasDeleteBuildingPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasViewBuildingPermissionHandler>();

        return services;
    }

    public static void AddBuildingsAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasCreateBuildingPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasCreateBuildingPermission()));
        options.AddPolicy(PolicyName.HasUpdateBuildingPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasUpdateBuildingPermission()));
        options.AddPolicy(PolicyName.HasDeleteBuildingPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasDeleteBuildingPermission()));
        options.AddPolicy(PolicyName.HasViewBuildingPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasViewBuildingPermission()));
    }
}
