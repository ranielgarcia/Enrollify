import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerCoursesPolicies = (registry: PolicyRegistry) => {
  registry.register(
    new PolicyBuilder("canViewCourses")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("View"), "Courses")
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
      .requirePermission(createPermission("Create"), "Courses")
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
      .requirePermission(createPermission("Update"), "Courses")
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
      .requirePermission(createPermission("Delete"), "Courses")
      .requireAll()
      .build()
  );
};
