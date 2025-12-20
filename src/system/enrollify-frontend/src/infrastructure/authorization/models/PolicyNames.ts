export const PolicyNames = {
  canViewRooms: "canViewRooms",
} as const;

export type PolicyName = keyof typeof PolicyNames;
