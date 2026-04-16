import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerTeacherPolicies = (registry: PolicyRegistry) => {
  registry.register(
    new PolicyBuilder("canViewTeachers")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("View"), "Teachers")
      .requireAll()
      .build(),
  );

  registry.register(
    new PolicyBuilder("canCreateTeacher")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("Create"), "Teachers")
      .requireAll()
      .build(),
  );

  registry.register(
    new PolicyBuilder("canUpdateTeacher")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("Update"), "Teachers")
      .requireAll()
      .build(),
  );

  registry.register(
    new PolicyBuilder("canDeleteTeacher")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("Delete"), "Teachers")
      .requireAll()
      .build(),
  );
};
