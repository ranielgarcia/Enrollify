import type {
  AuthorizationEvaluationContext,
  AuthorizationScope,
} from "../AuthorizationEvaluationContext";
import type { IAuthorizationRequirement } from "./IAuthorizationRequirement";

export class PermissionRequirement implements IAuthorizationRequirement {
  type = "Permission";

  public readonly permission: string;
  public readonly scope?: AuthorizationScope;

  constructor(permission: string, scope?: AuthorizationScope) {
    this.permission = permission;
    this.scope = scope;
  }

  evaluate(context: AuthorizationEvaluationContext): boolean {
    const effectiveScope = this.scope || context.scope;

    if (!context.user?.roles) return false;

    // Iterate through all roles to find the permission
    for (const role of context.user.roles) {
      if (!role.permissionScopes) continue;

      for (const permissionScope of role.permissionScopes) {
        // If scope is specified, check if it matches
        if (effectiveScope) {
          const scopeMatches =
            permissionScope.permissionScope?.name === effectiveScope.type &&
            (!effectiveScope.id ||
              permissionScope.permissionScope?.value === effectiveScope.id);

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
