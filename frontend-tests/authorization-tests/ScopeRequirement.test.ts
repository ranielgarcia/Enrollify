import { describe, it, expect } from "vitest";
import { ScopeRequirement } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/requirements/ScopeRequirement";
import type { AuthorizationEvaluationContext } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/AuthorizationEvaluationContext";
import {
  createScope,
  Scopes,
} from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/AuthorizationScope";
import type { UserContext } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/UserContext";

describe("ScopeRequirement", () => {
  const createMockUser = (scopeIds: number[]): UserContext => ({
    id: 1,
    email: "test@example.com",
    fullName: "test user",
    roles: [
      {
        id: 1,
        name: "TestRole",
        permissionScopes: scopeIds.map((scopeId) => ({
          scopeId,
          scopeName: `Scope-${scopeId}`,
          permissions: [],
        })),
      },
    ],
  });

  describe("constructor", () => {
    it("should create requirement with single scope", () => {
      const requirement = new ScopeRequirement([createScope("Users")]);

      expect(requirement.type).toBe("Scope");
      expect(requirement.scopes).toContain(Scopes.Users);
    });

    // Temporarily commented out, we will implement this
    // feature in the future if required
    // it("should create requirement with multiple scopes", () => {
    //   const requirement = new ScopeRequirement(
    //     createScope("Users"),
    //     createScope("Roles"),
    //     createScope("Rooms")
    //   );

    //   expect(requirement.allowedScopes).toHaveLength(3);
    // });
  });

  describe("evaluate", () => {
    it("should return true when user has access to required scope", () => {
      const requirement = new ScopeRequirement([createScope("Users")]);
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([Scopes.Users]),
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(true);
    });

    it("should return true when user has any of the required scopes", () => {
      const requirement = new ScopeRequirement([
        createScope("Users"),
        createScope("Roles"),
      ]);
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([Scopes.Roles]), // Has Roles only
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(true);
    });

    it("should return false when user lacks required scope", () => {
      const requirement = new ScopeRequirement([createScope("Users")]);
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([Scopes.Rooms, Scopes.RoomTypes]),
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(false);
    });

    it("should return false when user has no permission scopes", () => {
      const requirement = new ScopeRequirement([createScope("Users")]);
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([]),
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(false);
    });

    it("should check authorizationScope from context if provided", () => {
      const requirement = new ScopeRequirement([createScope("Users")]);
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([]),
        authorizationScope: { scope: { id: Scopes.Users, name: "Users" } },
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(true);
    });

    it("should work with multiple user scopes", () => {
      const requirement = new ScopeRequirement([createScope("Rooms")]);
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([Scopes.Users, Scopes.Roles, Scopes.Rooms]),
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(true);
    });
  });
});
