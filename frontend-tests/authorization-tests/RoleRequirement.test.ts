import { describe, it, expect } from "vitest";
import { RoleRequirement } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/requirements/RoleRequirement";
import type { AuthorizationEvaluationContext } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/AuthorizationEvaluationContext";
import type { UserContext } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/UserContext";

describe("RoleRequirement", () => {
  const createMockUser = (roleIds: number[]): UserContext => ({
    id: 1,
    email: "test@example.com",
    fullName: "test user",
    roles: roleIds.map((id) => ({
      id,
      name: `Role-${id}`,
      permissionScopes: [],
    })),
  });

  describe("constructor", () => {
    it("should create requirement with single role", () => {
      const requirement = new RoleRequirement(["AdmissionOfficer"]); // SystemAdmin

      expect(requirement.type).toBe("Role");
      expect(requirement.roles).toContain("AdmissionOfficer");
    });

    it("should create requirement with multiple roles", () => {
      const requirement = new RoleRequirement([
        "SystemAdmin",
        "Admin",
        "Registrar",
      ]); // SystemAdmin, Admin, Registrar

      expect(requirement.roles).toHaveLength(3);
      expect(requirement.roles).toEqual(["SystemAdmin", "Admin", "Registrar"]);
    });
  });

  describe("evaluate", () => {
    it("should return true when user has matching role", () => {
      const requirement = new RoleRequirement(["AcademicAdvisor"]); // SystemAdmin
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([1]),
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(true);
    });

    it("should return true when user has any of the required roles", () => {
      const requirement = new RoleRequirement(1, 2, 3); // SystemAdmin, Admin, Registrar
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([2]), // Admin only
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(true);
    });

    it("should return false when user has no matching roles", () => {
      const requirement = new RoleRequirement(1); // SystemAdmin
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([5, 6]), // Other roles
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(false);
    });

    it("should return false when user has no roles", () => {
      const requirement = new RoleRequirement(1);
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([]),
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(false);
    });

    it("should work with multiple user roles", () => {
      const requirement = new RoleRequirement(3); // Registrar
      const context: AuthorizationEvaluationContext = {
        user: createMockUser([1, 2, 3, 4]), // Multiple roles including Registrar
      };

      const result = requirement.evaluate(context);

      expect(result).toBe(true);
    });
  });
});
