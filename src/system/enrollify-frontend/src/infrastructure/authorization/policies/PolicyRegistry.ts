import type { AuthorizationPolicy } from "../AuthorizationPolicy";

export class PolicyRegistry {
  private policies: Map<string, AuthorizationPolicy> = new Map();

  register(policy: AuthorizationPolicy): void {
    this.policies.set(policy.name, policy);
  }

  get(name: string): AuthorizationPolicy | undefined {
    return this.policies.get(name);
  }

  has(name: string): boolean {
    return this.policies.has(name);
  }

  remove(name: string): boolean {
    return this.policies.delete(name);
  }

  getAll(): AuthorizationPolicy[] {
    return Array.from(this.policies.values());
  }
}
