import { describe, it, expect } from "vitest";
import { PermissionRequirement } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/requirements/PermissionRequirement";
import { AuthorizationEvaluationContext } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/AuthorizationEvaluationContext";
import {
  createPermission,
  Permissions,
} from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/PermissionsEnum";
import {
  createScope,
  Scopes,
} from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/AuthorizationScope";
import { UserContext } from "@/api/schema";

describe("PermissionRequirement", () => {
  const createMockUser = (
    permissions: number[],
    scopeId: number = Scopes.None
  ): UserContext => ({
    userId: "user-1",
    email: "test@example.com",
    firstName: "Test",
    lastName: "User",
    roles: [
      {
        id: 1,
        name: "TestRole",
        permissionScopes: [
          {
            scopeId,
            scopeName: `Scope-${scopeId}`,
            permissions: permissions.map((p, i) => ({
              id: i + 1,
              value: p,
              name: `Permission-${i}`,
            })),
          },
        ],
      },
    ],
  });

  describe("constructor", () => {
    it("should create requirement with permission only", () => {
      const requirement = new PermissionRequirement(createPermission("View"));

      expect(requirement.type).toBe("Permission");
      expect(requirement.requiredPermission).toBe(Permissions.View);
    });

    it("should create requirement with permission and scope", () => {
      const requirement = new PermissionRequirement(createPermission("View"), {
        scope: createScope("Users"),
      });

      expect(requirement.requiredPermission).toBe(Permissions.View);
      expect(requirement.requiredScope).toBe(Scopes.Users);
    });
  });

  describe("evaluate", () => {
    it("should return true when user has required permission", () => {
      const requirement = new PermissionRequirement(createPermission("View"));
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([Permissions.View]),
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(true);
    });

    it("should return true when user has Full permission (includes all)", () => {
      const requirement = new PermissionRequirement(createPermission("View"));
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([Permissions.Full]),
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(true);
    });

    it("should return true when user has combined permissions (bitwise)", () => {
      const requirement = new PermissionRequirement(createPermission("View"));
      // User has View | Create | Update combined
      const combinedPermission =
        Permissions.View | Permissions.Create | Permissions.Update;
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([combinedPermission]),
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(true);
    });

    it("should return false when user lacks required permission", () => {
      const requirement = new PermissionRequirement(createPermission("Delete"));
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([Permissions.View, Permissions.Create]),
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(false);
    });

    it("should check scope when required", () => {
      const requirement = new PermissionRequirement(createPermission("View"), {
        scope: createScope("Users"),
      });
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([Permissions.View], Scopes.Users),
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(true);
    });

    it("should return false when permission exists but scope does not match", () => {
      const requirement = new PermissionRequirement(createPermission("View"), {
        scope: createScope("Users"),
      });
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([Permissions.View], Scopes.Rooms), // Different scope
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(false);
    });

    it("should return false when user has no roles", () => {
      const requirement = new PermissionRequirement(createPermission("View"));
      const context: AuthorizationEvaluationContext = {
        user: {
          userId: "user-1",
          email: "test@example.com",
          firstName: "Test",
          lastName: "User",
          roles: [],
        },
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(false);
    });
  });
});
