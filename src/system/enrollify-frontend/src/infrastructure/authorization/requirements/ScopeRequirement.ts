import type { AuthorizationEvaluationContext } from "../models/AuthorizationEvaluationContext";
import type { Scope } from "../models/AuthorizationScope";
import type { IAuthorizationRequirement } from "./IAuthorizationRequirement";

export class ScopeRequirement implements IAuthorizationRequirement {
  type = "Scope";

  public readonly scopes: Scope[];

  constructor(scopes: Scope[]) {
    this.scopes = scopes;
  }

  evaluate(context: AuthorizationEvaluationContext): boolean {
    if (!context.authorizationScope) return false;

    if (
      !this.scopes.some(
        (scope) => scope.name === context.authorizationScope?.scope.name
      )
    )
      return false;

    if (
      this.scopes.some(
        (scope) => scope.id && context.authorizationScope?.scope.id !== scope.id
      )
    )
      return false;

    return true;
  }
}
