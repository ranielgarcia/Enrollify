import type { AuthorizationScope } from "../AuthorizationEvaluationContext";
import type { AuthorizationPolicy } from "../AuthorizationPolicy";
import type { IAuthorizationRequirement } from "../requirements/IAuthorizationRequirement";
import { PermissionRequirement } from "../requirements/PermissionRequirement";
import { RoleRequirement } from "../requirements/RoleRequirement";
import { ScopeRequirement } from "../requirements/ScopeRequirement";

export class PolicyBuilder {
  private policy: AuthorizationPolicy;

  constructor(name: string) {
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

  requireScope(scopeType: string, scopeId?: number): this {
    this.policy.requirements.push(new ScopeRequirement(scopeType, scopeId));
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

  build(): AuthorizationPolicy {
    return this.policy;
  }
}
