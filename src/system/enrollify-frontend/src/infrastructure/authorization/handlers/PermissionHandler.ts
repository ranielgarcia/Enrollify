import {
  RequirementTypes,
  type RequirementType,
} from "../models/RequirementTypes";
import type { IAuthorizationRequirement } from "../requirements/IAuthorizationRequirement";
import type { PermissionRequirement } from "../requirements/PermissionRequirement";
import type { IAuthorizationHandler } from "./IAuthorizationHandler";
import type { AuthorizationEvaluationContext } from "../models/AuthorizationEvaluationContext";
import type { AuthorizationResource } from "../models/AuthorizationResource";
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

    // Use requirement's scope, fallback to context's resource scope
    const effectiveScope =
      permissionRequirement.scope ?? context.resource?.scope;

    const resource: AuthorizationResource | undefined = effectiveScope
      ? { scope: effectiveScope }
      : undefined;

    return userHasPermission(
      context.user,
      permissionRequirement.permission,
      resource
    );
  }
}
