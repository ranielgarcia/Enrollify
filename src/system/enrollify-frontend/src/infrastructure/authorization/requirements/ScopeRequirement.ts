import type { AuthorizationEvaluationContext } from "../AuthorizationEvaluationContext";
import type { IAuthorizationRequirement } from "./IAuthorizationRequirement";

export class ScopeRequirement implements IAuthorizationRequirement {
  type = "Scope";

  public readonly scopeType: string;
  public readonly scopeId?: number;

  constructor(scopeType: string, scopeId?: number) {
    this.scopeType = scopeType;
    this.scopeId = scopeId;
  }

  evaluate(context: AuthorizationEvaluationContext): boolean {
    if (!context.scope) return false;

    if (context.scope.type !== this.scopeType) return false;

    if (this.scopeId && context.scope.id !== this.scopeId) return false;

    return true;
  }
}
