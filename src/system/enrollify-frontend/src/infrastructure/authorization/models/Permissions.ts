export const Permissions = {
  None: 0,
  View: 1 << 0,
  Create: 1 << 1,
  Update: 1 << 2,
  Delete: 1 << 3,
  Full: 1 << 4,
} as const;

export type PermissionName = keyof typeof Permissions;
export type PermissionId = (typeof Permissions)[PermissionName];

export interface Permission {
  name: PermissionName;
  value: PermissionId;
}
