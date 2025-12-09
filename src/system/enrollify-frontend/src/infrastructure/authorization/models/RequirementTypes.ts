export const RequirementTypes = {
  Scope: "Scope",
  Role: "Role",
  Permission: "Permission",
};

export type RequirementType = keyof typeof RequirementTypes;
