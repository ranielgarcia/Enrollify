import { describe, it, expect, beforeEach } from "vitest";
import { RoleHandler } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/handlers/RoleHandler";
import { RoleRequirement } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/requirements/RoleRequirement";
import type { AuthorizationEvaluationContext } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/AuthorizationEvaluationContext";
import { Roles } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/Roles";

describe("RoleHandler", () => {
  let handler: RoleHandler;

  // Helper to create mock user context
  const createMockUser = (roles: { id: number; name: string }[] = []) => ({
    id: 1,
    userName: "testuser",
    email: "test@test.com",
    roles,
  });

  beforeEach(() => {
    handler = new RoleHandler();
  });

  describe("canHandle", () => {
    it("should return true for Role requirements", () => {
      const requirement = new RoleRequirement(["Admin"]);
      expect(handler.canHandle(requirement)).toBe(true);
    });

    it("should return false for non-Role requirements", () => {
      const requirement = { type: "Permission", evaluate: () => true };
      expect(handler.canHandle(requirement)).toBe(false);
    });
  });

  describe("handle", () => {
    it("should authorize user with required role", () => {
      const requirement = new RoleRequirement(["Admin", "SystemAdmin"]);
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([{ id: Roles.Admin, name: "Admin" }]),
      };

      const result = handler.handle(requirement, context);
      expect(result).toBe(true);
    });

    it("should deny user without required role", () => {
      const requirement = new RoleRequirement(["Admin"]);
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([{ id: Roles.Teacher, name: "Teacher" }]),
      };

      const result = handler.handle(requirement, context);
      expect(result).toBe(false);
    });

    it("should deny user when user is null", () => {
      const requirement = new RoleRequirement(["Admin"]);
      const context: AuthorizationEvaluationContext = {
        user: null as any,
      };

      const result = handler.handle(requirement, context);
      expect(result).toBe(false);
    });

    it("should deny user when roles array is empty", () => {
      const requirement = new RoleRequirement(["Admin"]);
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([]),
      };

      const result = handler.handle(requirement, context);
      expect(result).toBe(false);
    });

    it("should authorize when user has any of the required roles", () => {
      const requirement = new RoleRequirement([
        "Admin",
        "Registrar",
        "SystemAdmin",
      ]);
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([{ id: Roles.Registrar, name: "Registrar" }]),
      };

      const result = handler.handle(requirement, context);
      expect(result).toBe(true);
    });

    it("should throw error for invalid requirement type", () => {
      const invalidRequirement = { type: "Permission", evaluate: () => true };
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([{ id: Roles.Admin, name: "Admin" }]),
      };

      expect(() => handler.handle(invalidRequirement, context)).toThrow(
        "RoleHandler cannot handle requirement of type: Permission"
      );
    });
  });
});
