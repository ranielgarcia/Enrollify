import { describe, it, expect, beforeEach } from "vitest";
import { AuthorizationService } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/AuthorizationService";
import { PolicyRegistry } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/policies/PolicyRegistry";
import { PolicyBuilder } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/policies/PolicyBuilder";
import { RoleHandler } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/handlers/RoleHandler";
import { PermissionHandler } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/handlers/PermissionHandler";
import { ScopeHandler } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/handlers/ScopeHandler";
import type { IAuthorizationHandler } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/handlers/IAuthorizationHandler";
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

describe("AuthorizationService", () => {
  let service: AuthorizationService;
  let registry: PolicyRegistry;
  let handlers: IAuthorizationHandler[];

  // Helper to create mock user context
  const createMockUser = (
    roles: { id: number; name: string; permissionScopes?: any[] }[] = []
  ) => ({
    id: 1,
    userName: "testuser",
    email: "test@test.com",
    roles,
  });

  beforeEach(() => {
    registry = new PolicyRegistry();
    handlers = [new RoleHandler(), new PermissionHandler(), new ScopeHandler()];
    service = new AuthorizationService(registry, handlers);
  });

  describe("authorize", () => {
    it("should authorize user with correct role", async () => {
      const policy = new PolicyBuilder("AdminOnly")
        .requireRole("Admin", "SystemAdmin")
        .build();

      registry.register(policy);

      const context: AuthorizationEvaluationContext = {
        user: createMockUser([{ id: Roles.Admin, name: "Admin" }]),
      };

      const result = await service.authorize("AdminOnly", context);
      expect(result.succeeded).toBe(true);
    });

    it("should deny user without required role", async () => {
      const policy = new PolicyBuilder("AdminOnly")
        .requireRole("Admin", "SystemAdmin")
        .build();

      registry.register(policy);

      const context: AuthorizationEvaluationContext = {
        user: createMockUser([{ id: Roles.Teacher, name: "Teacher" }]),
      };

      const result = await service.authorize("AdminOnly", context);
      expect(result.succeeded).toBe(false);
    });

    it("should handle policy not found", async () => {
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([{ id: Roles.Admin, name: "Admin" }]),
      };

      const result = await service.authorize(
        "NonExistentPolicy" as any,
        context
      );
      expect(result.succeeded).toBe(false);
      expect(result.failureReasons).toContain(
        "Policy 'NonExistentPolicy' not found"
      );
    });

    it("should support requireAll (AND logic)", async () => {
      const viewPermission = createPermission("View");

      const policy = new PolicyBuilder("AdminOnly")
        .requireRole("Admin")
        .requirePermission(viewPermission)
        .requireAll()
        .build();

      registry.register(policy);

      // User has role but not permission
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([
          {
            id: Roles.Admin,
            name: "Admin",
            permissionScopes: [], // No permissions
          },
        ]),
      };

      const result = await service.authorize("AdminOnly", context);
      expect(result.succeeded).toBe(false);
    });

    it("should support requireAny (OR logic)", async () => {
      const viewPermission = createPermission("View");

      const policy = new PolicyBuilder("AdminOnly")
        .requireRole("Admin")
        .requirePermission(viewPermission)
        .requireAny()
        .build();

      registry.register(policy);

      // User has role but not permission - should succeed with OR
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([
          {
            id: Roles.Admin,
            name: "Admin",
            permissionScopes: [], // No permissions
          },
        ]),
      };

      const result = await service.authorize("AdminOnly", context);
      expect(result.succeeded).toBe(true);
    });
  });

  describe("hasRole", () => {
    it("should return true when user has the role", () => {
      const user = createMockUser([{ id: Roles.Admin, name: "Admin" }]);

      const result = service.hasRole(user, { id: Roles.Admin, name: "Admin" });
      expect(result).toBe(true);
    });

    it("should return false when user does not have the role", () => {
      const user = createMockUser([{ id: Roles.Teacher, name: "Teacher" }]);

      const result = service.hasRole(user, { id: Roles.Admin, name: "Admin" });
      expect(result).toBe(false);
    });

    it("should return true when user has any of the specified roles", () => {
      const user = createMockUser([{ id: Roles.Registrar, name: "Registrar" }]);

      const result = service.hasRole(
        user,
        { id: Roles.Admin, name: "Admin" },
        { id: Roles.Registrar, name: "Registrar" }
      );
      expect(result).toBe(true);
    });

    it("should return false when user is null", () => {
      const result = service.hasRole(null, { id: Roles.Admin, name: "Admin" });
      expect(result).toBe(false);
    });
  });

  describe("hasPermission", () => {
    it("should return true when user has the permission in scope", () => {
      const user = createMockUser([
        {
          id: Roles.Admin,
          name: "Admin",
          permissionScopes: [
            {
              permissionScope: { id: Scopes.Users, name: "Users", value: null },
              permissions: [{ id: 1, name: "View", value: Permissions.View }],
            },
          ],
        },
      ]);

      const result = service.hasPermission(user, createPermission("View"), {
        scope: createScope("Users"),
      });
      expect(result).toBe(true);
    });

    it("should return false when user does not have the permission", () => {
      const user = createMockUser([
        {
          id: Roles.Admin,
          name: "Admin",
          permissionScopes: [
            {
              permissionScope: { id: Scopes.Users, name: "Users", value: null },
              permissions: [{ id: 1, name: "View", value: Permissions.View }],
            },
          ],
        },
      ]);

      const result = service.hasPermission(user, createPermission("Delete"), {
        scope: createScope("Users"),
      });
      expect(result).toBe(false);
    });

    it("should return false when user is null", () => {
      const result = service.hasPermission(null, createPermission("View"), {
        scope: createScope("Users"),
      });
      expect(result).toBe(false);
    });

    it("should return false when user has no roles", () => {
      const user = createMockUser([]);

      const result = service.hasPermission(user, createPermission("View"), {
        scope: createScope("Users"),
      });
      expect(result).toBe(false);
    });
  });

  describe("registerHandler", () => {
    it("should register a new handler at runtime", () => {
      const initialHandlerCount = service.getHandlers().length;

      const customHandler: IAuthorizationHandler = {
        requirementType: "Custom",
        canHandle: (req) => req.type === "Custom",
        handle: () => true,
      };

      service.registerHandler(customHandler);

      expect(service.getHandlers().length).toBe(initialHandlerCount + 1);
    });
  });

  describe("getHandlers", () => {
    it("should return a copy of handlers array", () => {
      const handlers1 = service.getHandlers();
      const handlers2 = service.getHandlers();

      expect(handlers1).not.toBe(handlers2);
      expect(handlers1).toEqual(handlers2);
    });
  });
});
