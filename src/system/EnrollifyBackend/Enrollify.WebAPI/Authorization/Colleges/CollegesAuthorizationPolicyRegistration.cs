using Microsoft.AspNetCore.Authorization;

namespace Enrollify.WebAPI.Authorization.Colleges;

public static class CollegesAuthorizationPolicyRegistration
{
    public static IServiceCollection AddCollegeAuthorizationPolicyHandlers (this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasCreateCollegePermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasUpdateCollegePermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasDeleteCollegePermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasViewCollegePermissionHandler>();

        return services;
    }

    public static void AddCollegeAuthorizationPolicies (this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasCreateCollegePermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasCreateCollegePermission()));
        options.AddPolicy(PolicyName.HasUpdateCollegePermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasUpdateCollegePermission()));
        options.AddPolicy(PolicyName.HasDeleteCollegePermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasDeleteCollegePermission()));
        options.AddPolicy(PolicyName.HasViewCollegePermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasViewCollegePermission()));
    }
}
