import { describe, it, expect, beforeEach } from "vitest";
import { PolicyRegistry } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/policies/PolicyRegistry";
import { PolicyBuilder } from "../../src/system/enrollify-frontend/src/infrastructure/authorization/policies/PolicyBuilder";

describe("PolicyRegistry", () => {
  let registry: PolicyRegistry;

  beforeEach(() => {
    registry = new PolicyRegistry();
  });

  describe("register", () => {
    it("should register a policy", () => {
      const policy = new PolicyBuilder("AdminOnly")
        .requireRole("Admin")
        .build();

      registry.register(policy);

      const result = registry.get("AdminOnly");
      expect(result).toBe(policy);
    });

    it("should overwrite policy with same name", () => {
      const policy1 = new PolicyBuilder("AdminOnly")
        .requireRole("Admin")
        .build();

      const policy2 = new PolicyBuilder("AdminOnly")
        .requireRole("SystemAdmin")
        .build();

      registry.register(policy1);
      registry.register(policy2);

      const result = registry.get("AdminOnly");
      expect(result).toBe(policy2);
    });
  });

  describe("get", () => {
    it("should return undefined for non-existent policy", () => {
      const result = registry.get("NonExistent");

      expect(result).toBeUndefined();
    });

    it("should return registered policy", () => {
      const policy = new PolicyBuilder("AdminOnly")
        .requireRole("Admin")
        .build();

      registry.register(policy);

      const result = registry.get("AdminOnly");
      expect(result).toBeDefined();
      expect(result?.name).toBe("AdminOnly");
    });
  });

  describe("has", () => {
    it("should return false for non-existent policy", () => {
      const result = registry.has("NonExistent");

      expect(result).toBe(false);
    });

    it("should return true for registered policy", () => {
      const policy = new PolicyBuilder("AdminOnly")
        .requireRole("Admin")
        .build();

      registry.register(policy);

      const result = registry.has("AdminOnly");
      expect(result).toBe(true);
    });
  });

  describe("remove", () => {
    it("should remove registered policy", () => {
      const policy = new PolicyBuilder("AdminOnly")
        .requireRole("Admin")
        .build();

      registry.register(policy);
      registry.remove("AdminOnly");

      expect(registry.has("AdminOnly")).toBe(false);
    });

    it("should do nothing for non-existent policy", () => {
      expect(() => registry.remove("NonExistent")).not.toThrow();
    });
  });

  describe("getAll", () => {
    it("should return empty array when no policies registered", () => {
      const result = registry.getAll();

      expect(result).toEqual([]);
    });

    it("should return all registered policies", () => {
      const policy1 = new PolicyBuilder("AdminOnly")
        .requireRole("Admin")
        .build();

      const policy2 = new PolicyBuilder("RegistrarOnly")
        .requireRole("Registrar")
        .build();

      registry.register(policy1);
      registry.register(policy2);

      const result = registry.getAll();
      expect(result).toHaveLength(2);
      expect(result).toContain(policy1);
      expect(result).toContain(policy2);
    });
  });

  describe("clear", () => {
    it("should remove all registered policies", () => {
      const policy1 = new PolicyBuilder("AdminOnly")
        .requireRole("Admin")
        .build();

      const policy2 = new PolicyBuilder("RegistrarOnly")
        .requireRole("Registrar")
        .build();

      registry.register(policy1);
      registry.register(policy2);
      registry.clear();

      expect(registry.getAll()).toHaveLength(0);
    });
  });
});
