using Enrollify.Core.Authentication;
using Enrollify.Core.Constants.Authorization;

namespace Enrollify.WebAPI.Authorization;

public static class MyRoleCheck
{
    public static bool HasAnyValidRole(this UserContext userContext)
    {
        return userContext.Roles.Count > 0 && 
               userContext.Roles.All(r => RolesEnum.TryFromValue(r.Id.Value, out _));
    }
    
    public static IEnumerable<UserRoleContext> GetValidRoles(this UserContext userContext)
    {
        return userContext.Roles.Where(r => RolesEnum.TryFromValue(r.Id.Value, out _));
    }

    public static bool HasPermissionToTheScope(this UserContext userContext, PermissionScopeEnum scope, PermissionEnum permission)
    {
        var userRoleScopes = userContext.Roles
            .SelectMany(r => r.PermissionScopes)
            .Where(ps => ps.PermissionScopeId.Value == scope.Value);

        return userRoleScopes.Any(ps => (ps.BitmaskPermission & permission.Value) != 0);
    }
}
