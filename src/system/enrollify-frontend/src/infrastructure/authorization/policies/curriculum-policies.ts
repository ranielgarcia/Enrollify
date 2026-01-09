import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerCurriculumPolicies = (registry: PolicyRegistry) => {
  registry.register(
    new PolicyBuilder("canViewCurriculums")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("View"), "Curriculums")
      .requireAll()
      .build()
  );
  registry.register(
    new PolicyBuilder("canCreateCurriculum")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Create"), "Curriculums")
      .requireAll()
      .build()
  );
  registry.register(
    new PolicyBuilder("canUpdateCurriculum")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Update"), "Curriculums")
      .requireAll()
      .build()
  );
  registry.register(
    new PolicyBuilder("canDeleteCurriculum")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Update"), "Curriculums")
      .requireAll()
      .build()
  );
};
