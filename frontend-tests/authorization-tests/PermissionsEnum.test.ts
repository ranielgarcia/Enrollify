import { describe, it, expect } from "vitest";
import {
  Permissions,
  PermissionFlagEnum,
  createPermission,
} from "../../src/system/enrollify-frontend/src/infrastructure/authorization/models/PermissionsEnum";

describe("PermissionsEnum", () => {
  describe("Permissions constants", () => {
    it("should define correct permission values", () => {
      expect(Permissions.None).toBe(0);
      expect(Permissions.View).toBe(1);
      expect(Permissions.Create).toBe(2);
      expect(Permissions.Update).toBe(4);
      expect(Permissions.Delete).toBe(8);
      expect(Permissions.Full).toBe(16);
    });

    it("should use powers of 2 for bitwise operations", () => {
      expect(Permissions.View).toBe(1 << 0);
      expect(Permissions.Create).toBe(1 << 1);
      expect(Permissions.Update).toBe(1 << 2);
      expect(Permissions.Delete).toBe(1 << 3);
      expect(Permissions.Full).toBe(1 << 4);
    });
  });

  describe("createPermission", () => {
    it("should return correct permission value for valid names", () => {
      expect(createPermission("None")).toBe(0);
      expect(createPermission("View")).toBe(1);
      expect(createPermission("Create")).toBe(2);
      expect(createPermission("Update")).toBe(4);
      expect(createPermission("Delete")).toBe(8);
      expect(createPermission("Full")).toBe(16);
    });
  });

  describe("PermissionFlagEnum", () => {
    describe("has", () => {
      it("should return true when flag is set", () => {
        const combined = Permissions.View | Permissions.Create;

        expect(PermissionFlagEnum.has(combined, Permissions.View)).toBe(true);
        expect(PermissionFlagEnum.has(combined, Permissions.Create)).toBe(true);
      });

      it("should return false when flag is not set", () => {
        const combined = Permissions.View | Permissions.Create;

        expect(PermissionFlagEnum.has(combined, Permissions.Update)).toBe(
          false
        );
        expect(PermissionFlagEnum.has(combined, Permissions.Delete)).toBe(
          false
        );
      });

      it("should handle Full permission", () => {
        expect(PermissionFlagEnum.has(Permissions.Full, Permissions.View)).toBe(
          true
        );
        expect(
          PermissionFlagEnum.has(Permissions.Full, Permissions.Create)
        ).toBe(true);
        expect(
          PermissionFlagEnum.has(Permissions.Full, Permissions.Update)
        ).toBe(true);
        expect(
          PermissionFlagEnum.has(Permissions.Full, Permissions.Delete)
        ).toBe(true);
      });

      it("should return false for None", () => {
        expect(PermissionFlagEnum.has(Permissions.None, Permissions.View)).toBe(
          false
        );
      });
    });

    describe("combine", () => {
      it("should combine multiple permissions", () => {
        const combined = PermissionFlagEnum.combine(
          Permissions.View,
          Permissions.Create,
          Permissions.Update
        );

        expect(combined).toBe(
          Permissions.View | Permissions.Create | Permissions.Update
        );
        expect(PermissionFlagEnum.has(combined, Permissions.View)).toBe(true);
        expect(PermissionFlagEnum.has(combined, Permissions.Create)).toBe(true);
        expect(PermissionFlagEnum.has(combined, Permissions.Update)).toBe(true);
        expect(PermissionFlagEnum.has(combined, Permissions.Delete)).toBe(
          false
        );
      });

      it("should handle single permission", () => {
        const combined = PermissionFlagEnum.combine(Permissions.View);

        expect(combined).toBe(Permissions.View);
      });

      it("should handle empty input", () => {
        const combined = PermissionFlagEnum.combine();

        expect(combined).toBe(0);
      });
    });

    describe("remove", () => {
      it("should remove flag from combined value", () => {
        const combined = Permissions.View | Permissions.Create;
        const result = PermissionFlagEnum.remove(combined, Permissions.Create);

        expect(result).toBe(Permissions.View);
        expect(PermissionFlagEnum.has(result, Permissions.View)).toBe(true);
        expect(PermissionFlagEnum.has(result, Permissions.Create)).toBe(false);
      });

      it("should do nothing if flag not present", () => {
        const combined = Permissions.View;
        const result = PermissionFlagEnum.remove(combined, Permissions.Create);

        expect(result).toBe(Permissions.View);
      });
    });

    describe("toArray", () => {
      it("should convert combined permissions to array", () => {
        const combined = Permissions.View | Permissions.Create;
        const result = PermissionFlagEnum.toArray(combined);

        expect(result).toContain(Permissions.View);
        expect(result).toContain(Permissions.Create);
        expect(result).not.toContain(Permissions.Update);
      });

      it("should return empty array for None", () => {
        const result = PermissionFlagEnum.toArray(Permissions.None);

        expect(result).toEqual([]);
      });
    });
  });
});
