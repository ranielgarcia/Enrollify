import {
  RequirementTypes,
  type RequirementType,
} from "../models/RequirementTypes";
import type { IAuthorizationRequirement } from "../requirements/IAuthorizationRequirement";
import type { RoleRequirement } from "../requirements/RoleRequirement";
import type { IAuthorizationHandler } from "./IAuthorizationHandler";
import type { AuthorizationEvaluationContext } from "../models/AuthorizationEvaluationContext";
import { userHasAnyRole } from "../helpers/AuthorizationHelpers";

export class RoleHandler implements IAuthorizationHandler {
  readonly requirementType = RequirementTypes.Role as RequirementType;

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
    return userHasAnyRole(context.user, roleRequirement.roles);
  }
}
