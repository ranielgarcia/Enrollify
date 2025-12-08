import type { AuthorizationScope, Scope } from "../models/AuthorizationScope";
import type { IAuthorizationPolicy } from "./IAuthorizationPolicy";
import type { IAuthorizationRequirement } from "../requirements/IAuthorizationRequirement";
import { PermissionRequirement } from "../requirements/PermissionRequirement";
import { RoleRequirement } from "../requirements/RoleRequirement";
import { ScopeRequirement } from "../requirements/ScopeRequirement";
import type { PolicyName } from "../models/PolicyNames";

export class PolicyBuilder {
  private policy: IAuthorizationPolicy;

  constructor(name: PolicyName) {
    this.policy = {
      name,
      requirements: [],
      requireAll: true,
    };
  }

  requireRole(...roles: string[]): this {
    this.policy.requirements.push(new RoleRequirement(roles));
    return this;
  }

  requirePermission(permission: string, scope?: AuthorizationScope): this {
    this.policy.requirements.push(new PermissionRequirement(permission, scope));
    return this;
  }

  requireScope(scope: Scope): this {
    this.policy.requirements.push(new ScopeRequirement(scope));
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
