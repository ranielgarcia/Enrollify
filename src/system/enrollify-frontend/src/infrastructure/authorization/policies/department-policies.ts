import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerDepartmentPolicies = (registry: PolicyRegistry) => {
  registry.register(
    new PolicyBuilder("canViewDepartments")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("View"), "Departments")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canCreateDepartment")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Create"), "Departments")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canUpdateDepartment")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Update"), "Departments")
      .requireAll()
      .build()
  );

  registry.register(
    new PolicyBuilder("canDeleteDepartment")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Delete"), "Departments")
      .requireAll()
      .build()
  );
};
