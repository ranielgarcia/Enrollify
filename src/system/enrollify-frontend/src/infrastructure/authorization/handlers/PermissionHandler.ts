import {
  RequirementTypes,
  type RequirementType,
} from "../models/RequirementTypes";
import type { IAuthorizationRequirement } from "../requirements/IAuthorizationRequirement";
import type { PermissionRequirement } from "../requirements/PermissionRequirement";
import type { IAuthorizationHandler } from "./IAuthorizationHandler";
import type { AuthorizationEvaluationContext } from "../models/AuthorizationEvaluationContext";
import { userHasPermission } from "../helpers/AuthorizationHelpers";

export class PermissionHandler implements IAuthorizationHandler {
  readonly requirementType = RequirementTypes.Permission as RequirementType;

  canHandle(requirement: IAuthorizationRequirement): boolean {
    return requirement.type === this.requirementType;
  }

  handle(
    requirement: IAuthorizationRequirement,
    context: AuthorizationEvaluationContext
  ): boolean {
    if (!this.canHandle(requirement)) {
      throw new Error(
        `PermissionHandler cannot handle requirement of type: ${requirement.type}`
      );
    }

    const permissionRequirement = requirement as PermissionRequirement;

    // Use context's authorizationScope if requirement doesn't specify one
    const effectiveAuthorizationScope =
      permissionRequirement.authorizationScope || context.authorizationScope;

    return userHasPermission(
      context.user,
      permissionRequirement.permission,
      effectiveAuthorizationScope
    );
  }
}
