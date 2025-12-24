import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerBuildingsPolicies = (registry: PolicyRegistry) => {
  registry.register(
    new PolicyBuilder("canViewBuildings")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("View"), "Buildings")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canCreateBuilding")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Create"), "Buildings")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canUpdateBuilding")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Update"), "Buildings")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canDeleteBuilding")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Delete"), "Buildings")
      .requireAll()
      .build()
  );
};
