import type { AuthorizationEvaluationContext } from "../models/AuthorizationEvaluationContext";
import {
  RequirementTypes,
  type RequirementType,
} from "../models/RequirementTypes";
import type { IAuthorizationRequirement } from "../requirements/IAuthorizationRequirement";
import type { ScopeRequirement } from "../requirements/ScopeRequirement";
import type { IAuthorizationHandler } from "./IAuthorizationHandler";

export class ScopeHandler implements IAuthorizationHandler {
  readonly requirementType = RequirementTypes.Scope as RequirementType;

  canHandle(requirement: IAuthorizationRequirement): boolean {
    return requirement.type === this.requirementType;
  }

  handle(
    requirement: IAuthorizationRequirement,
    context: AuthorizationEvaluationContext
  ): boolean {
    if (!this.canHandle(requirement)) {
      throw new Error(
        `ScopeHandler cannot handle requirement of type: ${requirement.type}`
      );
    }

    const scopeRequirement = requirement as ScopeRequirement;

    if (!context.authorizationScope) {
      return false;
    }

    // Check if scope type matches
    if (scopeRequirement.scope.name !== context.authorizationScope.scope.name)
      return false;

    // If specific scope ID is required, check it
    if (scopeRequirement.scope.id !== context.authorizationScope.scope.id) {
      return false;
    }

    return true;
  }
}
