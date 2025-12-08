import type { components } from "@/api/generated/api";

export interface AuthorizationEvaluationContext {
  user: components["schemas"]["EnrollifyCoreAuthenticationUserContext"] | null;
  resource?: string;
  scope?: AuthorizationScope;
}

export interface AuthorizationScope {
  type: "college" | "department" | "program" | "semester" | "global";
  id?: number;
  metadata?: Record<string, string>;
}
