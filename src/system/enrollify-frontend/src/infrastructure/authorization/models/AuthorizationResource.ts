import type { ScopeName } from "./AuthorizationScope";

/**
 * Represents a resource being authorized against.
 *
 * Combines:
 * 1. Feature/module scope (e.g., "Rooms", "Users") - for permission lookup
 * 2. Optional entity ID - for entity-specific authorization
 *
 * @example
 * // Feature-level: "Can user manage Rooms feature?"
 * { scope: "Rooms" }
 *
 * // Entity-level: "Can user edit Room #42?"
 * { scope: "Rooms", entityId: 42 }
 *
 * // With entity type: "Can user view this specific Department?"
 * { scope: "Users", entityType: "Department", entityId: 5 }
 */
export interface AuthorizationResource<TEntityId = number | string> {
  /**
   * The feature/module scope for permission lookup.
   * Maps to user's permissionScopes from the backend.
   */
  scope: ScopeName;

  /**
   * Optional: The specific entity ID being accessed.
   * Used for entity-level authorization checks.
   */
  entityId?: TEntityId;

  /**
   * Optional: The type of entity (if different from scope).
   * E.g., scope="Users" but entityType="Department" for department-specific checks.
   */
  entityType?: string;

  /**
   * Optional: Additional context data for custom authorization logic.
   */
  metadata?: Record<string, unknown>;
}

/**
 * Helper to create a feature-level resource (no specific entity)
 */
export function createResource(scope: ScopeName): AuthorizationResource {
  return { scope };
}

/**
 * Helper to create an entity-level resource
 */
export function createEntityResource<TEntityId = number | string>(
  scope: ScopeName,
  entityId: TEntityId,
  entityType?: string
): AuthorizationResource<TEntityId> {
  return { scope, entityId, entityType };
}

/**
 * Type guard to check if resource has an entity ID
 */
export function isEntityResource(
  resource: AuthorizationResource | undefined
): resource is AuthorizationResource & { entityId: number | string } {
  return resource !== undefined && resource.entityId !== undefined;
}
