import { createContext } from "react";
import type { AuthorizationScope } from "./AuthorizationEvaluationContext";
import type { AuthorizationResult } from "./AuthorizationResult";
import type { AuthorizationService } from "./AuthorizationService";

// React Context for Authorization
export interface AuthorizationContextValue {
  authorize: (
    policyName: string,
    resource?: string,
    scope?: AuthorizationScope
  ) => Promise<AuthorizationResult>;
  hasRole: (...roles: string[]) => boolean;
  hasPermission: (permission: string, scope?: AuthorizationScope) => boolean;
  authService: AuthorizationService | null;
}

export const AuthorizationContext =
  createContext<AuthorizationContextValue | null>(null);
