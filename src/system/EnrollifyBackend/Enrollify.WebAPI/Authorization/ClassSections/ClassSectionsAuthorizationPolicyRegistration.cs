namespace Enrollify.WebAPI.Authorization.ClassSections;

public static class ClassSectionsAuthorizationPolicyRegistration
{
    public static IServiceCollection AddClassSectionsAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasCreateClassSectionPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasUpdateClassSectionPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasDeleteClassSectionPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasViewClassSectionPermissionHandler>();

        return services;
    }

    public static void AddClassSectionsAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasCreateClassSectionPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasCreateClassSectionPermission()));
        options.AddPolicy(PolicyName.HasUpdateClassSectionPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasUpdateClassSectionPermission()));
        options.AddPolicy(PolicyName.HasDeleteClassSectionPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasDeleteClassSectionPermission()));
        options.AddPolicy(PolicyName.HasViewClassSectionPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasViewClassSectionsPermission()));
    }
}
