import { createScope } from "../models/AuthorizationScope";
import { createPermission } from "../models/PermissionsEnum";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerDefaultPolicies = (registry: PolicyRegistry) => {
  // Department-scoped policy
  registry.register(
    new PolicyBuilder("AdminOnly")
      .requireRole("DepartmentHead")
      .requireScope([createScope("None")])
      .requirePermission(createPermission("Full"))
      .build()
  );
};
