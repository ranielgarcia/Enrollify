import { describe, it, expect, beforeEach } from "vitest";
import { PolicyBuilder } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/policies/PolicyBuilder";
import { createPermission } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/PermissionsEnum";
import { createScope } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/AuthorizationScope";

describe("PolicyBuilder", () => {
  describe("requireRole", () => {
    it("should add role requirement to policy", () => {
      const policy = new PolicyBuilder("AdminOnly")
        .requireRole("Admin", "SystemAdmin")
        .build();

      expect(policy.name).toBe("AdminOnly");
      expect(policy.requirements).toHaveLength(1);
      expect(policy.requirements[0].type).toBe("Role");
    });

    it("should support method chaining", () => {
      const builder = new PolicyBuilder("AdminOnly");
      const result = builder.requireRole("Admin");

      expect(result).toBe(builder);
    });
  });

  describe("requirePermission", () => {
    it("should add permission requirement to policy", () => {
      const policy = new PolicyBuilder("CanManageStudents")
        .requirePermission(createPermission("View"))
        .build();

      expect(policy.requirements).toHaveLength(1);
      expect(policy.requirements[0].type).toBe("Permission");
    });

    it("should add permission requirement with scope", () => {
      const policy = new PolicyBuilder("CanManageStudents")
        .requirePermission(createPermission("View"), {
          scope: createScope("Users"),
        })
        .build();

      expect(policy.requirements).toHaveLength(1);
      expect(policy.requirements[0].type).toBe("Permission");
    });
  });

  describe("requireScope", () => {
    it("should add scope requirement to policy", () => {
      const policy = new PolicyBuilder("DepartmentAdmin")
        .requireScope(createScope("Users"))
        .build();

      expect(policy.requirements).toHaveLength(1);
      expect(policy.requirements[0].type).toBe("Scope");
    });
  });

  describe("requireAll", () => {
    it("should set requireAll to true", () => {
      const policy = new PolicyBuilder("AdminOnly")
        .requireRole("Admin")
        .requirePermission(createPermission("View"))
        .requireAll()
        .build();

      expect(policy.requireAll).toBe(true);
    });
  });

  describe("requireAny", () => {
    it("should set requireAll to false", () => {
      const policy = new PolicyBuilder("CanViewReports")
        .requireRole("Admin", "Registrar")
        .requireAny()
        .build();

      expect(policy.requireAll).toBe(false);
    });
  });

  describe("addCustomRequirement", () => {
    it("should add custom requirement to policy", () => {
      const customRequirement = {
        type: "Custom",
        evaluate: () => true,
      };

      const policy = new PolicyBuilder("AdminOnly")
        .addCustomRequirement(customRequirement)
        .build();

      expect(policy.requirements).toHaveLength(1);
      expect(policy.requirements[0].type).toBe("Custom");
    });
  });

  describe("build", () => {
    it("should return complete policy with defaults", () => {
      const policy = new PolicyBuilder("AdminOnly").build();

      expect(policy.name).toBe("AdminOnly");
      expect(policy.requirements).toEqual([]);
      expect(policy.requireAll).toBe(true);
    });

    it("should build complex policy with multiple requirements", () => {
      const policy = new PolicyBuilder("EnrollmentManager")
        .requireRole("Registrar", "Admin")
        .requirePermission(createPermission("Create"))
        .requireScope(createScope("Users"))
        .requireAll()
        .build();

      expect(policy.name).toBe("EnrollmentManager");
      expect(policy.requirements).toHaveLength(3);
      expect(policy.requireAll).toBe(true);
    });
  });
});
