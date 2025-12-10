export const Permissions = {
  None: 0,
  View: 1 << 0,
  Create: 1 << 1,
  Update: 1 << 2,
  Delete: 1 << 3,
  Full: 1 << 4,
} as const;

export type PermissionName = keyof typeof Permissions;
export type PermissionValue = (typeof Permissions)[PermissionName];

// Type-safe Scope that ensures id and name correspond correctly
export type Permission = {
  [K in PermissionName]: { value: (typeof Permissions)[K]; name: K };
}[PermissionName];

// Helper function to create a Scope from just the name
export const createPermission = <T extends PermissionName>(
  name: T
): Extract<Permission, { name: T }> =>
  ({
    value: Permissions[name],
    name,
  }) as Extract<Permission, { name: T }>;

export class PermissionFlagEnum {
  private readonly permission: Permission;

  constructor(value: Permission) {
    this.permission = value;
  }

  has(flag: PermissionValue) {
    return (this.permission.value & flag) !== 0;
  }

  add(flag: PermissionValue) {
    this.permission.value |= flag;
  }

  remove(flag: PermissionValue) {
    this.permission.value &= ~flag;
  }
}
