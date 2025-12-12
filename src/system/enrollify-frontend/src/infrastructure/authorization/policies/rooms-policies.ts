import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerRoomPolicies = (registry: PolicyRegistry) => {
  // Admin-only policy with full permissions on Rooms scope
  registry.register(
    new PolicyBuilder("canViewRooms")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("View"), "Rooms")
      .requireAll()
      .build()
  );
};
