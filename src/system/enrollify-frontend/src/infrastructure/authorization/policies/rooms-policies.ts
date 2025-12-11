import { createScope } from "../models/AuthorizationScope";
import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerRoomPolicies = (registry: PolicyRegistry) => {
  // Department-scoped policy
  registry.register(
    new PolicyBuilder("AdminOnly")
      .requireRole(createRole("Admin"))
      .requireScope(createScope("None"))
      .requirePermission(createPermission("Full"))
      .build()
  );
};
