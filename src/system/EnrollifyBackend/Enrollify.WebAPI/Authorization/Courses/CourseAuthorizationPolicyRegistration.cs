namespace Enrollify.WebAPI.Authorization.Courses;

public static class CourseAuthorizationPolicyRegistration
{
    public static IServiceCollection AddCoursesAuthorizationPolicyHandlers(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasCreateCoursePermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasUpdateCoursePermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasDeleteCoursePermissionHandler>();
        services.AddScoped<IAuthorizationHandler, HasViewCoursesPermissionHandler>();

        return services;
    }

    public static void AddCoursesAuthorizationPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(PolicyName.HasCreateCoursePermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasCreateCoursePermission()));
        options.AddPolicy(PolicyName.HasUpdateCoursePermission, policyBuilder => 
            policyBuilder.AddRequirements(new HasUpdateCoursePermission()));
        options.AddPolicy(PolicyName.HasDeleteCoursePermission, policyBuilder =>
            policyBuilder.AddRequirements(new HasDeleteCoursePermission()));
        options.AddPolicy(PolicyName.HasViewCoursesPermission, policyBuilder => 
            policyBuilder.AddRequirements(new HasViewCoursesPermission()));
    }
}
