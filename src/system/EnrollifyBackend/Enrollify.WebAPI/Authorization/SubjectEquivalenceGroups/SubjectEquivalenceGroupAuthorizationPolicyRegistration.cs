namespace Enrollify.WebAPI.Authorization.SubjectEquivalenceGroups;

public static class SubjectEquivalenceGroupAuthorizationPolicyRegistration
{
    public static IServiceCollection AddSubjectEquivalenceGroupAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasCreateSubjectEquivalenceGroupPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasUpdateSubjectEquivalenceGroupPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasDeleteSubjectEquivalenceGroupPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasViewSubjectEquivalenceGroupsPermissionHandler>();
        return services;
    }

    public static void AddSubjectEquivalenceGroupAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasCreateSubjectEquivalenceGroupPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasCreateSubjectEquivalenceGroupPermission()));
        options.AddPolicy(PolicyName.HasUpdateSubjectEquivalenceGroupPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasUpdateSubjectEquivalenceGroupPermission()));
        options.AddPolicy(PolicyName.HasDeleteSubjectEquivalenceGroupPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasDeleteSubjectEquivalenceGroupPermission()));
        options.AddPolicy(PolicyName.HasViewSubjectEquivalenceGroupsPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasViewSubjectEquivalenceGroupsPermission()));
    }
}
