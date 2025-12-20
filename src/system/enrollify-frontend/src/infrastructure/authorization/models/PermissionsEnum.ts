// This must stay in sync with PermissionEnum.cs in the backend
export const Permissions = {
  None: 0,
  View: 1 << 0, // 1
  Create: 1 << 1, // 2
  Update: 1 << 2, // 4
  Delete: 1 << 3, // 8
  Full: (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3), // 15 (View | Create | Update | Delete)
} as const;

export type PermissionName = keyof typeof Permissions;
export type PermissionValue = (typeof Permissions)[PermissionName];

// Type-safe Permission that ensures value and name correspond correctly
export type Permission = {
  [K in PermissionName]: { value: (typeof Permissions)[K]; name: K };
}[PermissionName];

// Helper function to create a Permission from just the name
export const createPermission = <T extends PermissionName>(
  name: T
): Extract<Permission, { name: T }> =>
  ({
    value: Permissions[name],
    name,
  }) as Extract<Permission, { name: T }>;

/**
 * Utility class for working with bitwise permission flags.
 * Mirrors the SmartFlagEnum pattern from the backend.
 *
 * Note: This class is immutable - methods return new Permission objects
 * rather than mutating the original.
 */
export class PermissionFlagEnum {
  private readonly value: PermissionValue;

  constructor(permission: Permission | PermissionValue) {
    this.value = typeof permission === "number" ? permission : permission.value;
  }

  /**
   * Check if this permission includes the specified flag
   */
  has(flag: PermissionValue): boolean {
    // Full permission includes all flags
    if (this.value === Permissions.Full) {
      return flag !== Permissions.None;
    }
    return (this.value & flag) !== 0;
  }

  /**
   * Check if this permission includes ALL of the specified flags
   */
  hasAll(...flags: PermissionValue[]): boolean {
    return flags.every((flag) => this.has(flag));
  }

  /**
   * Check if this permission includes ANY of the specified flags
   */
  hasAny(...flags: PermissionValue[]): boolean {
    return flags.some((flag) => this.has(flag));
  }

  /**
   * Combine this permission with additional flags (immutable)
   */
  add(...flags: PermissionValue[]): PermissionFlagEnum {
    const combined = flags.reduce((acc, flag) => acc | flag, this.value);
    return new PermissionFlagEnum(combined as PermissionValue);
  }

  /**
   * Remove flags from this permission (immutable)
   */
  remove(...flags: PermissionValue[]): PermissionFlagEnum {
    const result = flags.reduce((acc, flag) => acc & ~flag, this.value);
    return new PermissionFlagEnum(result as PermissionValue);
  }

  /**
   * Get the raw numeric value
   */
  getValue(): PermissionValue {
    return this.value;
  }

  /**
   * Get an array of individual permission flags that are set
   */
  toArray(): PermissionValue[] {
    const flags: PermissionValue[] = [];
    if (this.has(Permissions.View)) flags.push(Permissions.View);
    if (this.has(Permissions.Create)) flags.push(Permissions.Create);
    if (this.has(Permissions.Update)) flags.push(Permissions.Update);
    if (this.has(Permissions.Delete)) flags.push(Permissions.Delete);
    return flags;
  }

  /**
   * Get an array of permission names that are set
   */
  toNames(): PermissionName[] {
    const names: PermissionName[] = [];
    if (this.has(Permissions.View)) names.push("View");
    if (this.has(Permissions.Create)) names.push("Create");
    if (this.has(Permissions.Update)) names.push("Update");
    if (this.has(Permissions.Delete)) names.push("Delete");
    return names;
  }

  /**
   * Check if this permission is empty (None)
   */
  isEmpty(): boolean {
    return this.value === Permissions.None;
  }

  /**
   * Check if this permission has full access
   */
  isFull(): boolean {
    return (
      this.value === Permissions.Full ||
      this.hasAll(
        Permissions.View,
        Permissions.Create,
        Permissions.Update,
        Permissions.Delete
      )
    );
  }

  /**
   * Create a PermissionFlagEnum from multiple flags
   */
  static combine(...flags: PermissionValue[]): PermissionFlagEnum {
    const combined = flags.reduce((acc, flag) => acc | flag, 0);
    return new PermissionFlagEnum(combined as PermissionValue);
  }

  /**
   * Create a PermissionFlagEnum with all permissions
   */
  static full(): PermissionFlagEnum {
    return new PermissionFlagEnum(Permissions.Full);
  }

  /**
   * Create an empty PermissionFlagEnum
   */
  static none(): PermissionFlagEnum {
    return new PermissionFlagEnum(Permissions.None);
  }
}

// Sample usage
// Check permissions
// const perms = new PermissionFlagEnum(createPermission("Full"));
// perms.has(Permissions.View);      // true
// perms.hasAll(Permissions.View, Permissions.Create);  // true
// perms.toNames();  // ["View", "Create", "Update", "Delete"]

// Combine permissions (immutable)
// const readWrite = PermissionFlagEnum.combine(Permissions.View, Permissions.Update);
// const withDelete = readWrite.add(Permissions.Delete);  // Returns new instance
