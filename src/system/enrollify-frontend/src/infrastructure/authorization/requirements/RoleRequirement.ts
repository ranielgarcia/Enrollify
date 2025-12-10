import type { AuthorizationEvaluationContext } from "../models/AuthorizationEvaluationContext";
import type { Role } from "../models/Roles";
import type { IAuthorizationRequirement } from "./IAuthorizationRequirement";

export class RoleRequirement implements IAuthorizationRequirement {
  type = "Role";

  public readonly roles: Role[];

  constructor(roles: Role[]) {
    this.roles = roles;
  }

  evaluate(context: AuthorizationEvaluationContext): boolean {
    return this.roles.some((role) =>
      context?.user?.roles?.map((r) => r.id)?.includes(role.id)
    );
  }
}
