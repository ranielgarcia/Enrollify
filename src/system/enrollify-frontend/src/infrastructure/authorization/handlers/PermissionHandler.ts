import type { AuthorizationEvaluationContext } from "../models/AuthorizationEvaluationContext";
import type { IAuthorizationRequirement } from "../requirements/IAuthorizationRequirement";
import type { PermissionRequirement } from "../requirements/PermissionRequirement";
import type { IAuthorizationHandler } from "./IAuthorizationHandler";

export class PermissionHandler implements IAuthorizationHandler {
  readonly requirementType = "Permission";

  canHandle(requirement: IAuthorizationRequirement): boolean {
    return requirement.type === this.requirementType;
  }

  handle(
    requirement: IAuthorizationRequirement,
    context: AuthorizationEvaluationContext
  ): boolean {
    if (!this.canHandle(requirement)) {
      throw new Error(
        `PermissionHandler cannot handle requirement of type: ${requirement.type}`
      );
    }

    const permissionRequirement = requirement as PermissionRequirement;

    if (!context.user?.roles) {
      return false;
    }

    const effectiveScope =
      permissionRequirement.authorizationScope?.scope ||
      context.authorizationScope?.scope;

    // Iterate through all roles to find the permission
    for (const role of context.user.roles) {
      if (!role.permissionScopes) continue;

      for (const permissionScope of role.permissionScopes) {
        // If scope is specified, check if it matches
        if (effectiveScope) {
          const scopeMatches =
            permissionScope.permissionScope?.name === effectiveScope.name &&
            (!effectiveScope.id ||
              permissionScope.permissionScope?.value === effectiveScope.id);

          if (!scopeMatches) continue;
        }

        // Check if the permission exists in this scope
        const hasPermission = permissionScope.permissions?.some(
          (p) => p.name === permissionRequirement.permission
        );

        if (hasPermission) return true;
      }
    }

    return false;
  }
}
