import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerSubjectsPolicies = (registry: PolicyRegistry) => {
  registry.register(
    new PolicyBuilder("canViewSubjects")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("View"), "Subjects")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canCreateSubject")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Create"), "Subjects")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canUpdateSubject")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Update"), "Subjects")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canDeleteSubject")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Delete"), "Subjects")
      .requireAll()
      .build()
  );
};
