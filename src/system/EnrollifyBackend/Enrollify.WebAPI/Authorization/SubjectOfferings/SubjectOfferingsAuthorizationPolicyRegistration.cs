namespace Enrollify.WebAPI.Authorization.SubjectOfferings;

public static class SubjectOfferingsAuthorizationPolicyRegistration
{
    public static IServiceCollection AddSubjectOfferingsAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasCreateSubjectOfferingPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasUpdateSubjectOfferingPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasDeleteSubjectOfferingPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasViewSubjectOfferingsPermissionHandler>();

        return services;
    }

    public static void AddSubjectOfferingsAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasCreateSubjectOfferingPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasCreateSubjectOfferingPermission()));
        options.AddPolicy(PolicyName.HasUpdateSubjectOfferingPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasUpdateSubjectOfferingPermission()));
        options.AddPolicy(PolicyName.HasDeleteSubjectOfferingPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasDeleteSubjectOfferingPermission()));
        options.AddPolicy(PolicyName.HasViewSubjectOfferingsPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasViewSubjectOfferingsPermission()));
    }
}
