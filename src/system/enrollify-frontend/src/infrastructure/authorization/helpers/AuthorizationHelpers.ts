import type { UserContext } from "../../../api/api-dtos/UserContext";
import type { ScopeName } from "../models/AuthorizationScope";
import { Scopes } from "../models/AuthorizationScope";
import type { AuthorizationResource } from "../models/AuthorizationResource";
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
 * Check if a user's permission scope matches the target scope name
 */
export function scopeMatches(
  userScope: { name?: string | null; value?: number } | null | undefined,
  targetScopeName: ScopeName
): boolean {
  if (!userScope) return false;

  // Check name matches
  if (userScope.name !== targetScopeName) return false;

  // Check value matches the expected scope ID
  const expectedScopeId = Scopes[targetScopeName];
  if (userScope.value !== expectedScopeId) return false;

  return true;
}

/**
 * Check if user has the specified permission within the given resource scope.
 *
 * @param user - The user context
 * @param permission - The permission to check
 * @param resource - Optional resource (if provided, checks permission within that scope)
 *
 * @example
 * Check if user has View permission on Rooms feature
 * userHasPermission(user, createPermission("View"), { scope: "Rooms" })
 *
 * Check if user has Update permission (any scope)
 * userHasPermission(user, createPermission("Update"))
 */
export function userHasPermission(
  user: UserContext | null | undefined,
  permission: Permission,
  resource?: AuthorizationResource
): boolean {
  const validation = validateUserContext(user);
  if (!validation.isValid) return false;

  const permissionFlag = new PermissionFlagEnum(permission);

  for (const role of user!.roles!) {
    if (!role.permissionScopes) continue;

    for (const permissionScope of role.permissionScopes) {
      // If resource scope is specified, check if it matches
      if (resource?.scope) {
        const matches = scopeMatches(
          permissionScope.permissionScope,
          resource.scope
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
