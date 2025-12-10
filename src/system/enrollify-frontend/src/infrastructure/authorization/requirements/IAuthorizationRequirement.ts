import type { AuthorizationEvaluationContext } from "../models/AuthorizationEvaluationContext";

export interface IAuthorizationRequirement {
  type: string;
  evaluate(context: AuthorizationEvaluationContext): boolean | Promise<boolean>;
}
