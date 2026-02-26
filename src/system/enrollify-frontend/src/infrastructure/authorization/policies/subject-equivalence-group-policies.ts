import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerSubjectEquivalenceGroupPolicies = (
  registry: PolicyRegistry,
) => {
  registry.register(
    new PolicyBuilder("canViewSubjectEquivalenceGroups")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("View"), "SubjectEquivalenceGroups")
      .requireAll()
      .build(),
  );

  registry.register(
    new PolicyBuilder("canCreateSubjectEquivalenceGroup")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("Create"), "SubjectEquivalenceGroups")
      .requireAll()
      .build(),
  );

  registry.register(
    new PolicyBuilder("canUpdateSubjectEquivalenceGroup")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("Update"), "SubjectEquivalenceGroups")
      .requireAll()
      .build(),
  );

  registry.register(
    new PolicyBuilder("canDeleteSubjectEquivalenceGroup")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("Delete"), "SubjectEquivalenceGroups")
      .requireAll()
      .build(),
  );
};
