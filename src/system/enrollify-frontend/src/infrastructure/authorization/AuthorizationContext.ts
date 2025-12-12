import { createContext } from "react";
import type { AuthorizationResult } from "./AuthorizationResult";
import type { AuthorizationService } from "./AuthorizationService";
import type { AuthorizationResource } from "./models/AuthorizationResource";
import type { PolicyName } from "./models/PolicyNames";
import type { Permission } from "./models/PermissionsEnum";
import type { Role } from "./models/Roles";

// React Context for Authorization
export interface IAuthorizationContextValue {
  /**
   * Authorize against a named policy.
   * @param policyName - The policy to evaluate
   * @param resource - Optional resource for scoped/entity-level authorization
   */
  authorize: (
    policyName: PolicyName,
    resource?: AuthorizationResource
  ) => Promise<AuthorizationResult>;

  checkPolicy: (
    policyName: PolicyName,
    resource?: AuthorizationResource
  ) => Promise<boolean>;

  /**
   * Check if the current user has any of the specified roles.
   */
  currentUserHasRole: (...roles: Role[]) => boolean;

  /**
   * Check if the current user has a permission, optionally within a resource scope.
   */
  currentUserHasPermission: (
    permission: Permission,
    resource?: AuthorizationResource
  ) => boolean;

  authService: AuthorizationService | null;
}

export const AuthorizationContext =
  createContext<IAuthorizationContextValue | null>(null);
