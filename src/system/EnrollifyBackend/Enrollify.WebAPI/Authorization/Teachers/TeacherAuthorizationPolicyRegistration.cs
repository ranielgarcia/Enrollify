using Enrollify.WebAPI.Authorization.Subjects;

namespace Enrollify.WebAPI.Authorization.Teachers;

public static class TeacherAuthorizationPolicyRegistration
{
    public static IServiceCollection AddTeacherAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasCreateTeacherPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasUpdateTeacherPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasDeleteTeacherPermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasViewTeacherPermissionHandler>();

        return services;
    }

    public static void AddTeacherAuthorizationPolicies (this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasCreateTeacherPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasCreateTeacherPermission()));

        options.AddPolicy(PolicyName.HasUpdateTeacherPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasUpdateTeacherPermission()));

        options.AddPolicy(PolicyName.HasDeleteTeacherPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasDeleteTeacherPermission()));

        options.AddPolicy(PolicyName.HasViewTeacherPermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasViewTeacherPermission()));
    }
}
