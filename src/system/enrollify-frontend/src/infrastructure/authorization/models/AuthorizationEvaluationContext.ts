import type { components } from "@/api/generated/api";
import type { AuthorizationScope } from "./AuthorizationScope";

export interface AuthorizationEvaluationContext {
  user: components["schemas"]["EnrollifyCoreAuthenticationUserContext"] | null;
  resource?: string;
  authorizationScope?: AuthorizationScope;
}
