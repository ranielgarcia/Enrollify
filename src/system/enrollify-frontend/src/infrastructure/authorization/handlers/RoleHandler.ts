import type { AuthorizationEvaluationContext } from "../AuthorizationEvaluationContext";
import type { IAuthorizationRequirement } from "../requirements/IAuthorizationRequirement";
import type { RoleRequirement } from "../requirements/RoleRequirement";
import type { IAuthorizationHandler } from "./IAuthorizationHandler";

export class RoleHandler implements IAuthorizationHandler {
  readonly requirementType = "Role";

  canHandle(requirement: IAuthorizationRequirement): boolean {
    return requirement.type === this.requirementType;
  }

  handle(
    requirement: IAuthorizationRequirement,
    context: AuthorizationEvaluationContext
  ): boolean {
    if (!this.canHandle(requirement)) {
      throw new Error(
        `RoleHandler cannot handle requirement of type: ${requirement.type}`
      );
    }

    const roleRequirement = requirement as RoleRequirement;

    if (
      !context.user ||
      !context.user.roles ||
      context.user.roles.length === 0
    ) {
      return false;
    }

    // User must have at least one of the required roles
    return roleRequirement.roles.some((role) =>
      context.user?.roles?.map((r) => r.name).includes(role)
    );
  }
}
