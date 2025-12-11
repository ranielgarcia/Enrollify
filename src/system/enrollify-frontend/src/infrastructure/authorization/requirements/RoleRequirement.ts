import type { AuthorizationEvaluationContext } from "../models/AuthorizationEvaluationContext";
import type { Role } from "../models/Roles";
import type { IAuthorizationRequirement } from "./IAuthorizationRequirement";
import { userHasAnyRole } from "../helpers/AuthorizationHelpers";

export class RoleRequirement implements IAuthorizationRequirement {
  type = "Role";

  public readonly roles: Role[];

  constructor(roles: Role[]) {
    this.roles = roles;
  }

  evaluate(context: AuthorizationEvaluationContext): boolean {
    return userHasAnyRole(context.user, this.roles);
  }
}
