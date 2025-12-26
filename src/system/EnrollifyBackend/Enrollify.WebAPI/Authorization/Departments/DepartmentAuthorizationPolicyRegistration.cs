namespace Enrollify.WebAPI.Authorization.Departments;

public static class DepartmentAuthorizationPolicyRegistration
{
    public static IServiceCollection AddDepartmentAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasCreateDepartmentPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasUpdateDepartmentPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasDeleteDepartmentPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasViewDepartmentPermissionHandler>();

        return services;
    }

    public static void AddDepartmentAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasCreateDepartmentPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasCreateDepartmentPermission()));
        options.AddPolicy(PolicyName.HasUpdateDepartmentPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasUpdateDepartmentPermission()));
        options.AddPolicy(PolicyName.HasDeleteDepartmentPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasDeleteDepartmentPermission()));
        options.AddPolicy(PolicyName.HasViewDepartmentPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasViewDepartmentPermission()));
    }
}
