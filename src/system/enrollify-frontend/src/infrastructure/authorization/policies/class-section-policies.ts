import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerClassSectionPolicies = (registry: PolicyRegistry) => {
  registry.register(
    new PolicyBuilder("canViewClassSections")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("View"), "ClassSections")
      .requireAll()
      .build(),
  );

  registry.register(
    new PolicyBuilder("canCreateClassSection")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("Create"), "ClassSections")
      .requireAll()
      .build(),
  );

  registry.register(
    new PolicyBuilder("canUpdateClassSection")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("Update"), "ClassSections")
      .requireAll()
      .build(),
  );

  registry.register(
    new PolicyBuilder("canDeleteClassSection")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("Delete"), "ClassSections")
      .requireAll()
      .build(),
  );
};
