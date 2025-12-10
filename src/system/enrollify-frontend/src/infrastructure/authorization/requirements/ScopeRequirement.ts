import type { AuthorizationEvaluationContext } from "../models/AuthorizationEvaluationContext";
import type { Scope } from "../models/AuthorizationScope";
import type { IAuthorizationRequirement } from "./IAuthorizationRequirement";

export class ScopeRequirement implements IAuthorizationRequirement {
  type = "Scope";

  public readonly scope: Scope;

  constructor(scope: Scope) {
    this.scope = scope;
  }

  evaluate(context: AuthorizationEvaluationContext): boolean {
    if (!context.authorizationScope) return false;

    if (this.scope.name !== context.authorizationScope?.scope.name)
      return false;

    if (context.authorizationScope?.scope.id !== this.scope.id) return false;

    return true;
  }
}
