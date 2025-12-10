import { describe, it, expect, beforeEach } from "vitest";
import { ScopeHandler } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/handlers/ScopeHandler";
import { ScopeRequirement } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/requirements/ScopeRequirement";
import type { AuthorizationEvaluationContext } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/AuthorizationEvaluationContext";
import {
  createScope,
  Scopes,
} from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/AuthorizationScope";
import { Roles } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/Roles";

describe("ScopeHandler", () => {
  let handler: ScopeHandler;

  // Helper to create mock user context
  const createMockUser = () => ({
    id: 1,
    userName: "testuser",
    email: "test@test.com",
    roles: [{ id: Roles.Admin, name: "Admin" }],
  });

  beforeEach(() => {
    handler = new ScopeHandler();
  });

  describe("canHandle", () => {
    it("should return true for Scope requirements", () => {
      const requirement = new ScopeRequirement([createScope("Users")]);
      expect(handler.canHandle(requirement)).toBe(true);
    });

    it("should return false for non-Scope requirements", () => {
      const requirement = { type: "Role", evaluate: () => true };
      expect(handler.canHandle(requirement)).toBe(false);
    });
  });

  describe("handle", () => {
    it("should authorize when scope type matches", () => {
      const requirement = new ScopeRequirement([createScope("Users")]);
      const context: AuthorizationEvaluationContext = {
        user: createMockUser(),
        authorizationScope: { scope: createScope("Users") },
      };

      const result = handler.handle(requirement, context);
      expect(result).toBe(true);
    });

    it("should authorize when scope type and id match", () => {
      const scopeWithId = { id: Scopes.Users, name: "Users" as const };
      const requirement = new ScopeRequirement([scopeWithId]);
      const context: AuthorizationEvaluationContext = {
        user: createMockUser(),
        authorizationScope: { scope: scopeWithId },
      };

      const result = handler.handle(requirement, context);
      expect(result).toBe(true);
    });

    it("should deny when scope type does not match", () => {
      const requirement = new ScopeRequirement([createScope("Roles")]);
      const context: AuthorizationEvaluationContext = {
        user: createMockUser(),
        authorizationScope: { scope: createScope("Users") },
      };

      const result = handler.handle(requirement, context);
      expect(result).toBe(false);
    });

    it("should deny when context has no scope", () => {
      const requirement = new ScopeRequirement([createScope("Users")]);
      const context: AuthorizationEvaluationContext = {
        user: createMockUser(),
      };

      const result = handler.handle(requirement, context);
      expect(result).toBe(false);
    });

    it("should throw error for invalid requirement type", () => {
      const invalidRequirement = { type: "Role", evaluate: () => true };
      const context: AuthorizationEvaluationContext = {
        user: createMockUser(),
        authorizationScope: { scope: createScope("Users") },
      };

      expect(() => handler.handle(invalidRequirement, context)).toThrow(
        "ScopeHandler cannot handle requirement of type: Role"
      );
    });
  });
});
