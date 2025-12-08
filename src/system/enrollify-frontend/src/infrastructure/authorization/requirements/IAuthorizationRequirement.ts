import type { AuthorizationEvaluationContext } from "../AuthorizationEvaluationContext";

export interface IAuthorizationRequirement {
  type: string;
  evaluate(context: AuthorizationEvaluationContext): boolean | Promise<boolean>;
}
