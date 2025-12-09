import type { AuthorizationEvaluationContext } from "../models/AuthorizationEvaluationContext";
import type { RoleName } from "../models/Roles";
import type { IAuthorizationRequirement } from "./IAuthorizationRequirement";

export class RoleRequirement implements IAuthorizationRequirement {
  type = "Role";

  public readonly roles: RoleName[];

  constructor(roles: RoleName[]) {
    this.roles = roles;
  }

  evaluate(context: AuthorizationEvaluationContext): boolean {
    return this.roles.some((role) =>
      context?.user?.roles?.map((r) => r.name)?.includes(role)
    );
  }
}
