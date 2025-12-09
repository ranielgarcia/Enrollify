import { describe, it, expect } from "vitest";
import {
  Scopes,
  createScope,
  AuthorizationScope,
} from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/AuthorizationScope";

describe("AuthorizationScope", () => {
  describe("Scopes constants", () => {
    it("should define correct scope values", () => {
      expect(Scopes.None).toBe(0);
      expect(Scopes.Users).toBe(1);
      expect(Scopes.Roles).toBe(2);
      expect(Scopes.Rooms).toBe(3);
      expect(Scopes.RoomTypes).toBe(4);
    });
  });

  describe("createScope", () => {
    it("should return correct scope value for valid names", () => {
      expect(createScope("None")).toBe(0);
      expect(createScope("Users")).toBe(1);
      expect(createScope("Roles")).toBe(2);
      expect(createScope("Rooms")).toBe(3);
      expect(createScope("RoomTypes")).toBe(4);
    });
  });

  describe("AuthorizationScope interface", () => {
    it("should create valid authorization scope object", () => {
      const scope: AuthorizationScope = {
        scopeId: Scopes.Users,
        scopeName: "Users",
      };

      expect(scope.scopeId).toBe(1);
      expect(scope.scopeName).toBe("Users");
    });

    it("should support optional properties", () => {
      const scope: AuthorizationScope = {
        scopeId: Scopes.Rooms,
        scopeName: "Rooms",
      };

      expect(scope).toBeDefined();
    });
  });
});
