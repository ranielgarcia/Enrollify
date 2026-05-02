namespace Enrollify.WebAPI.Authorization.AcademicYearsAndTerms;

public static class AcademicYearsAndTermsAuthorizationPolicyRegistration
{
    public static IServiceCollection AddAcademicYearsAndTermsAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasCreateAcademicYearAndTermPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasUpdateAcademicYearAndTermPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasDeleteAcademicYearAndTermPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasViewAcademicYearAndTermPermissionHandler>();
        return services;
    }

    public static void AddAcademicYearsAndTermsAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasCreateAcademicYearAndTermPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasCreateAcademicYearAndTermPermission()));
        options.AddPolicy(PolicyName.HasUpdateAcademicYearAndTermPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasUpdateAcademicYearAndTermPermission()));
        options.AddPolicy(PolicyName.HasDeleteAcademicYearAndTermPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasDeleteAcademicYearAndTermPermission()));
        options.AddPolicy(PolicyName.HasViewAcademicYearAndTermPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasViewAcademicYearAndTermPermission()));
    }
}
