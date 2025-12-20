import type { AuthorizationEvaluationContext } from "../models/AuthorizationEvaluationContext";
import type { ScopeName } from "../models/AuthorizationScope";
import type { Permission } from "../models/PermissionsEnum";
import type { IAuthorizationRequirement } from "./IAuthorizationRequirement";
import { userHasPermission } from "../helpers/AuthorizationHelpers";

export class PermissionRequirement implements IAuthorizationRequirement {
  type = "Permission";

  public readonly permission: Permission;
  public readonly scope: ScopeName;

  /**
   * Create a permission requirement.
   * @param permission - The permission to check (View, Create, Update, Delete, Full)
   * @param scope - The feature scope to check the permission within (Users, Roles, Rooms, etc.)
   */
  constructor(permission: Permission, scope: ScopeName) {
    this.permission = permission;
    this.scope = scope;
  }

  evaluate(context: AuthorizationEvaluationContext): boolean {
    return userHasPermission(context.user, this.permission, {
      scope: this.scope,
    });
  }
}
