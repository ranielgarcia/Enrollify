import type { AuthorizationScope } from "./AuthorizationScope";
import type { UserContext } from "./UserContext";

export interface AuthorizationEvaluationContext {
  user: UserContext;
  resource?: string;
  authorizationScope?: AuthorizationScope;
}
