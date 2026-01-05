import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerCurriculumPolicies = (registry: PolicyRegistry) => {
  registry.register(
    new PolicyBuilder("canManageCurriculum")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar")
      )
      .requirePermission(createPermission("Full"), "Curriculum")
      .requireAll()
      .build()
  );
};
