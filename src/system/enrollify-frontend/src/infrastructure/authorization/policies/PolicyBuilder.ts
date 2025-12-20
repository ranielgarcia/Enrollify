import type { ScopeName } from "../models/AuthorizationScope";
import type { IAuthorizationPolicy } from "./IAuthorizationPolicy";
import type { IAuthorizationRequirement } from "../requirements/IAuthorizationRequirement";
import { PermissionRequirement } from "../requirements/PermissionRequirement";
import { RoleRequirement } from "../requirements/RoleRequirement";
import type { PolicyName } from "../models/PolicyNames";
import type { Permission } from "../models/PermissionsEnum";
import type { Role } from "../models/Roles";

export class PolicyBuilder {
  private policy: IAuthorizationPolicy;

  constructor(name: PolicyName) {
    this.policy = {
      name,
      requirements: [],
      requireAll: true,
    };
  }

  requireRole(...roles: Role[]): this {
    this.policy.requirements.push(new RoleRequirement(roles));
    return this;
  }

  /**
   * Require a specific permission within a scope.
   * @param permission - The permission to require (View, Create, Update, Delete, Full)
   * @param scope - The feature scope to check the permission within (Users, Roles, Rooms, etc.)
   */
  requirePermission(permission: Permission, scope: ScopeName): this {
    this.policy.requirements.push(new PermissionRequirement(permission, scope));
    return this;
  }

  requireAll(): this {
    this.policy.requireAll = true;
    return this;
  }

  requireAny(): this {
    this.policy.requireAll = false;
    return this;
  }

  addCustomRequirement(requirement: IAuthorizationRequirement): this {
    this.policy.requirements.push(requirement);
    return this;
  }

  build(): IAuthorizationPolicy {
    return this.policy;
  }
}
