import type { AuthorizationScope } from "./AuthorizationScope";
import type { UserContext } from "../../../api/models/UserContext";

export interface AuthorizationEvaluationContext {
  user: UserContext;
  resource?: string;
  authorizationScope?: AuthorizationScope;
}
