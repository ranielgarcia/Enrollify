import type { UserContext } from "../../../api/models/UserContext";
import type { AuthorizationScope, Scope } from "../models/AuthorizationScope";
import {
  PermissionFlagEnum,
  type Permission,
  type PermissionValue,
} from "../models/PermissionsEnum";
import type { Role } from "../models/Roles";

/**
 * Result of user context validation
 */
export interface UserValidationResult {
  isValid: boolean;
  hasRoles: boolean;
  hasPermissionScopes: boolean;
  failureReason?: string;
}

/**
 * Validates user context for authorization checks.
 * Mirrors the validation logic in UserAuthorizationHandler.cs in the backend
 */
export function validateUserContext(
  user: UserContext | null | undefined
): UserValidationResult {
  if (!user) {
    return {
      isValid: false,
      hasRoles: false,
      hasPermissionScopes: false,
      failureReason:
        "Cannot process authorization requirement without an authenticated user",
    };
  }

  const hasRoles = Boolean(user.roles && user.roles.length > 0);
  if (!hasRoles) {
    return {
      isValid: false,
      hasRoles: false,
      hasPermissionScopes: false,
      failureReason:
        "Cannot process authorization requirement on a user without a role or any valid role",
    };
  }

  const hasPermissionScopes = user.roles!.some(
    (r) => r.permissionScopes && r.permissionScopes.length > 0
  );
  if (!hasPermissionScopes) {
    return {
      isValid: false,
      hasRoles: true,
      hasPermissionScopes: false,
      failureReason:
        "Cannot process authorization requirement on a user without any permissions",
    };
  }

  return {
    isValid: true,
    hasRoles: true,
    hasPermissionScopes: true,
  };
}

/**
 * Check if the user has any of the specified roles
 */
export function userHasAnyRole(
  user: UserContext | null | undefined,
  roles: Role[]
): boolean {
  if (!user?.roles || user.roles.length === 0) return false;

  const userRoleIds = user.roles.map((r) => r.id);
  return roles.some((role) => userRoleIds.includes(role.id));
}

/**
 * Check if scope matches the target scope
 */
export function scopeMatches(
  userScope: { name?: string | null; value?: number } | null | undefined,
  targetScope: Scope
): boolean {
  if (!userScope) return false;

  // Check name matches
  if (userScope.name !== targetScope.name) return false;

  // If target has specific ID, check it matches
  if (targetScope.id !== 0 && userScope.value !== targetScope.id) return false;

  return true;
}

/**
 * Check if user has the specified permission within the given scope
 */
export function userHasPermission(
  user: UserContext | null | undefined,
  permission: Permission,
  authorizationScope?: AuthorizationScope
): boolean {
  const validation = validateUserContext(user);
  if (!validation.isValid) return false;

  const permissionFlag = new PermissionFlagEnum(permission);

  for (const role of user!.roles!) {
    if (!role.permissionScopes) continue;

    for (const permissionScope of role.permissionScopes) {
      // If scope is specified, check if it matches
      if (authorizationScope) {
        const matches = scopeMatches(
          permissionScope.permissionScope,
          authorizationScope.scope
        );
        if (!matches) continue;
      }

      // Check if the permission exists in this scope
      const hasPermission = checkPermissionInScope(
        permissionScope.permissions,
        permissionFlag
      );
      if (hasPermission) return true;
    }
  }

  return false;
}

/**
 * Check if any permission in the array satisfies the required permission flag
 */
export function checkPermissionInScope(
  permissions:
    | Array<{ value?: number | null; name?: string | null }>
    | null
    | undefined,
  requiredPermission: PermissionFlagEnum
): boolean {
  if (!permissions) return false;

  return permissions.some(
    (p) =>
      p.value !== undefined &&
      p.value !== null &&
      requiredPermission.has(p.value as PermissionValue)
  );
}

/**
 * Get the effective scope from requirement and context
 */
export function getEffectiveScope(
  requirementScope: AuthorizationScope | undefined,
  contextScope: AuthorizationScope | undefined
): Scope | undefined {
  return requirementScope?.scope || contextScope?.scope;
}
