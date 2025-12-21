import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerCollegesPolicies = (registry: PolicyRegistry) => {
  registry.register(
    new PolicyBuilder("canViewColleges")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("View"), "Colleges")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canCreateCollege")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Create"), "Colleges")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canUpdateCollege")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Update"), "Colleges")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canDeleteCollege")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Delete"), "Colleges")
      .requireAll()
      .build()
  );
};
