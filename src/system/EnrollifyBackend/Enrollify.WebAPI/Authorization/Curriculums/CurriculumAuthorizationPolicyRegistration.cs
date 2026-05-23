namespace Enrollify.WebAPI.Authorization.Curriculums;

public static class CurriculumAuthorizationPolicyRegistration
{
    public static IServiceCollection AddCurriculumAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasCreateCurriculumPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasUpdateCurriculumPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasDeleteCurriculumPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasViewCurriculumsPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, CanApproveCurriculumPermissionHandler>();

        return services;
    }

    public static void AddCurriculumAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasCreateCurriculumPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasCreateCurriculumPermission()));
        options.AddPolicy(PolicyName.HasUpdateCurriculumPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasUpdateCurriculumPermission()));
        options.AddPolicy(PolicyName.HasDeleteCurriculumPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasDeleteCurriculumPermission()));
        options.AddPolicy(PolicyName.HasViewCurriculumsPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasViewCurriculumsPermission()));
        options.AddPolicy(PolicyName.CanApproveCurriculumPermission, policyBuilder =>
            policyBuilder.AddRequirements(new CanApproveCurriculumPermission()));
    }
}
