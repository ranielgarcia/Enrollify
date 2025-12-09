import type { AuthorizationEvaluationContext } from "../models/AuthorizationEvaluationContext";
import type { IAuthorizationRequirement } from "../requirements/IAuthorizationRequirement";
import type { ScopeRequirement } from "../requirements/ScopeRequirement";
import type { IAuthorizationHandler } from "./IAuthorizationHandler";

export class ScopeHandler implements IAuthorizationHandler {
  readonly requirementType = "Scope";

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
    if (
      scopeRequirement.scopes
        .map((s) => s.name)
        .indexOf(context.authorizationScope.scope.name) === -1
    ) {
      return false;
    }

    // If specific scope ID is required, check it
    if (
      scopeRequirement.scopes
        .map((s) => s.id)
        .indexOf(context.authorizationScope.scope.id) === -1
    ) {
      return false;
    }

    return true;
  }
}
