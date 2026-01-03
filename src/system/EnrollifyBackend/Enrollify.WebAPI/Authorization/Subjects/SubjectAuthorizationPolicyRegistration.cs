namespace Enrollify.WebAPI.Authorization.Subjects;

public static class SubjectAuthorizationPolicyRegistration
{
    public static IServiceCollection AddSubjectAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasCreateSubjectPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasUpdateSubjectPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasDeleteSubjectPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasViewSubjectsPermissionHandler>();

        return services;
    }

    public static void AddSubjectAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasCreateSubjectPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasCreateSubjectPermission()));
        options.AddPolicy(PolicyName.HasUpdateSubjectPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasUpdateSubjectPermission()));
        options.AddPolicy(PolicyName.HasDeleteSubjectPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasDeleteSubjectPermission()));
        options.AddPolicy(PolicyName.HasViewSubjectsPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasViewSubjectsPermission()));
    }
}
