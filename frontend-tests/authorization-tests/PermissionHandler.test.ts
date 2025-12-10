import { describe, it, expect, beforeEach } from "vitest";
import { PermissionHandler } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/handlers/PermissionHandler";
import { PermissionRequirement } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/requirements/PermissionRequirement";
import type { AuthorizationEvaluationContext } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/AuthorizationEvaluationContext";
import {
  createPermission,
  Permissions,
} from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/PermissionsEnum";
import {
  createScope,
  Scopes,
} from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/AuthorizationScope";
import { Roles } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/Roles";

describe("PermissionHandler", () => {
  let handler: PermissionHandler;

  // Helper to create mock user context with permissions
  const createMockUserWithPermissions = (
    permissionScopes: {
      scopeName: string;
      scopeId: number;
      scopeValue?: number | null;
      permissions: { id: number; name: string; value: number }[];
    }[]
  ) => ({
    id: 1,
    userName: "testuser",
    email: "test@test.com",
    roles: [
      {
        id: Roles.Admin,
        name: "Admin",
        permissionScopes: permissionScopes.map((ps) => ({
          permissionScope: {
            id: ps.scopeId,
            name: ps.scopeName,
            value: ps.scopeValue ?? null,
          },
          permissions: ps.permissions,
        })),
      },
    ],
  });

  beforeEach(() => {
    handler = new PermissionHandler();
  });

  describe("canHandle", () => {
    it("should return true for Permission requirements", () => {
      const requirement = new PermissionRequirement(createPermission("View"));
      expect(handler.canHandle(requirement)).toBe(true);
    });

    it("should return false for non-Permission requirements", () => {
      const requirement = { type: "Role", evaluate: () => true };
      expect(handler.canHandle(requirement)).toBe(false);
    });
  });

  describe("handle", () => {
    it("should authorize user with matching permission in scope", () => {
      const requirement = new PermissionRequirement(createPermission("View"), {
        scope: createScope("Users"),
      });
      const context: AuthorizationEvaluationContext = {
        user: createMockUserWithPermissions([
          {
            scopeName: "Users",
            scopeId: Scopes.Users,
            permissions: [{ id: 1, name: "View", value: Permissions.View }],
          },
        ]),
      };

      const result = handler.handle(requirement, context);
      expect(result).toBe(true);
    });

    it("should deny user without matching permission", () => {
      const requirement = new PermissionRequirement(
        createPermission("Delete"),
        { scope: createScope("Users") }
      );
      const context: AuthorizationEvaluationContext = {
        user: createMockUserWithPermissions([
          {
            scopeName: "Users",
            scopeId: Scopes.Users,
            permissions: [{ id: 1, name: "View", value: Permissions.View }],
          },
        ]),
      };

      const result = handler.handle(requirement, context);
      expect(result).toBe(false);
    });

    it("should deny user when scope does not match", () => {
      const requirement = new PermissionRequirement(createPermission("View"), {
        scope: createScope("Roles"),
      });
      const context: AuthorizationEvaluationContext = {
        user: createMockUserWithPermissions([
          {
            scopeName: "Users",
            scopeId: Scopes.Users,
            permissions: [{ id: 1, name: "View", value: Permissions.View }],
          },
        ]),
      };

      const result = handler.handle(requirement, context);
      expect(result).toBe(false);
    });

    it("should deny user when user is null", () => {
      const requirement = new PermissionRequirement(createPermission("View"));
      const context: AuthorizationEvaluationContext = {
        user: null as any,
      };

      const result = handler.handle(requirement, context);
      expect(result).toBe(false);
    });

    it("should deny user when user has no roles", () => {
      const requirement = new PermissionRequirement(createPermission("View"));
      const context: AuthorizationEvaluationContext = {
        user: {
          id: 1,
          userName: "testuser",
          email: "test@test.com",
          roles: [],
        },
      };

      const result = handler.handle(requirement, context);
      expect(result).toBe(false);
    });

    it("should use context scope when requirement scope is not specified", () => {
      const requirement = new PermissionRequirement(createPermission("View"));
      const context: AuthorizationEvaluationContext = {
        user: createMockUserWithPermissions([
          {
            scopeName: "Users",
            scopeId: Scopes.Users,
            permissions: [{ id: 1, name: "View", value: Permissions.View }],
          },
        ]),
        authorizationScope: { scope: createScope("Users") },
      };

      const result = handler.handle(requirement, context);
      expect(result).toBe(true);
    });

    it("should throw error for invalid requirement type", () => {
      const invalidRequirement = { type: "Role", evaluate: () => true };
      const context: AuthorizationEvaluationContext = {
        user: createMockUserWithPermissions([]),
      };

      expect(() => handler.handle(invalidRequirement, context)).toThrow(
        "PermissionHandler cannot handle requirement of type: Role"
      );
    });
  });
});
