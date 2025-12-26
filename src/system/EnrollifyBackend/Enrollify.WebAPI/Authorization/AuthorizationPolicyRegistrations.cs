using Enrollify.WebAPI.Authorization.Buildings;
using Enrollify.WebAPI.Authorization.Colleges;
using Enrollify.WebAPI.Authorization.Departments;
using Enrollify.WebAPI.Authorization.Roles;
using Enrollify.WebAPI.Authorization.Rooms;
using Enrollify.WebAPI.Authorization.RoomTypes;
using Enrollify.WebAPI.Authorization.Shared;

namespace Enrollify.WebAPI.Authorization;

public static class AuthorizationPolicyRegistrations
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, HasAnyValidRoleHandler>();
        services.AddRolesAuthorizationPolicyHandlers();
        services.AddRoomTypesAuthorizationPolicyHandlers();
        services.AddRoomAuthorizationPolicyHandlers();
        services.AddCollegeAuthorizationPolicyHandlers();
        services.AddBuildingsAuthorizationPolicyHandlers();
        services.AddDepartmentAuthorizationPolicyHandlers();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(PolicyName.HasAnyValidRoleAndPermission, policyBuilder =>
                policyBuilder.AddRequirements(new HasAnyValidRole()));

            options.AddRolesAuthorizationPolicies();
            options.AddRoomTypesAuthorizationPolicies();
            options.AddRoomsAuthorizationPolicies();
            options.AddCollegeAuthorizationPolicies();
            options.AddBuildingsAuthorizationPolicies();
            options.AddDepartmentAuthorizationPolicies();
        });

        return services;
    }
}
