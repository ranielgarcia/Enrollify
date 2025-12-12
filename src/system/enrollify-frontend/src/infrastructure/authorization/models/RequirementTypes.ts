export const RequirementTypes = {
  Role: "Role",
  Permission: "Permission",
};

export type RequirementType = keyof typeof RequirementTypes;
