import { createContext } from "react";
import type { AuthorizationResult } from "./AuthorizationResult";
import type { AuthorizationService } from "./AuthorizationService";
import type { AuthorizationScope } from "./models/AuthorizationScope";
import type { PolicyName } from "./models/PolicyNames";
import type { Permission } from "./models/Permissions";
import type { Role } from "./models/Roles";

// React Context for Authorization
export interface AuthorizationContextValue {
  authorize: (
    policyName: PolicyName,
    resource?: string,
    scope?: AuthorizationScope
  ) => Promise<AuthorizationResult>;
  hasRole: (...roles: Role[]) => boolean;
  hasPermission: (
    permission: Permission,
    scope?: AuthorizationScope
  ) => boolean;
  authService: AuthorizationService | null;
}

export const AuthorizationContext =
  createContext<AuthorizationContextValue | null>(null);
