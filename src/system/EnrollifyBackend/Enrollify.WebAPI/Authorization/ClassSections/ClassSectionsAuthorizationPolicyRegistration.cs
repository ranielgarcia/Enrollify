namespace Enrollify.WebAPI.Authorization.ClassSections;

public static class ClassSectionsAuthorizationPolicyRegistration
{
    public static IServiceCollection AddClassSectionsAuthorizationPolicyHandlers(this IServiceCollection services)
    {

        return services;
    }

    public static void AddClassSectionsAuthorizationPolicies(this AuthorizationOptions options)
    {

    }
}
