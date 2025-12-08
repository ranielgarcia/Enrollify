import type { IAuthorizationPolicy } from "./IAuthorizationPolicy";
import type { PolicyName } from "../models/PolicyNames";

export class PolicyRegistry {
  private policies: Map<PolicyName, IAuthorizationPolicy> = new Map();

  register(policy: IAuthorizationPolicy): void {
    this.policies.set(policy.name, policy);
  }

  get(name: PolicyName): IAuthorizationPolicy | undefined {
    return this.policies.get(name);
  }

  has(name: PolicyName): boolean {
    return this.policies.has(name);
  }

  remove(name: PolicyName): boolean {
    return this.policies.delete(name);
  }

  getAll(): IAuthorizationPolicy[] {
    return Array.from(this.policies.values());
  }
}
