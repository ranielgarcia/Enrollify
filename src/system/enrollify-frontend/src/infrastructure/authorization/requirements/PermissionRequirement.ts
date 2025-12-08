import type { AuthorizationEvaluationContext } from "../models/AuthorizationEvaluationContext";
import type { AuthorizationScope } from "../models/AuthorizationScope";
import type { IAuthorizationRequirement } from "./IAuthorizationRequirement";

export class PermissionRequirement implements IAuthorizationRequirement {
  type = "Permission";

  public readonly permission: string;
  public readonly authorizationScope?: AuthorizationScope;

  constructor(permission: string, authorizationScope?: AuthorizationScope) {
    this.permission = permission;
    this.authorizationScope = authorizationScope;
  }

  evaluate(context: AuthorizationEvaluationContext): boolean {
    const effectiveScope =
      this.authorizationScope || context.authorizationScope;

    if (!context.user?.roles) return false;

    // Iterate through all roles to find the permission
    for (const role of context.user.roles) {
      if (!role.permissionScopes) continue;

      for (const permissionScope of role.permissionScopes) {
        // If scope is specified, check if it matches
        if (effectiveScope) {
          const scopeMatches =
            permissionScope.permissionScope?.name ===
              effectiveScope.scope.name &&
            (!effectiveScope.scope.id ||
              permissionScope.permissionScope?.value ===
                effectiveScope.scope.id);

          if (!scopeMatches) continue;
        }

        // Check if the permission exists in this scope
        const hasPermission = permissionScope.permissions?.some(
          (p) => p.name === this.permission
        );

        if (hasPermission) return true;
      }
    }

    return false;
  }
}
