import type { AuthorizationEvaluationContext } from "../models/AuthorizationEvaluationContext";
import type { AuthorizationScope } from "../models/AuthorizationScope";
import type { Permission } from "../models/PermissionsEnum";
import type { IAuthorizationRequirement } from "./IAuthorizationRequirement";
import { userHasPermission } from "../helpers/AuthorizationHelpers";

export class PermissionRequirement implements IAuthorizationRequirement {
  type = "Permission";

  public readonly permission: Permission;
  public readonly authorizationScope?: AuthorizationScope;

  constructor(permission: Permission, authorizationScope?: AuthorizationScope) {
    this.permission = permission;
    this.authorizationScope = authorizationScope;
  }

  evaluate(context: AuthorizationEvaluationContext): boolean {
    const effectiveAuthorizationScope =
      this.authorizationScope || context.authorizationScope;

    return userHasPermission(
      context.user,
      this.permission,
      effectiveAuthorizationScope
    );
  }
}
