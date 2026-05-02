import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerAcademicYearAndTermsPolicies = (
  registry: PolicyRegistry,
) => {
  registry.register(
    new PolicyBuilder("canViewAcademicYearsAndTerms")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("View"), "AcademicYearsAndTerms")
      .requireAll()
      .build(),
  );

  registry.register(
    new PolicyBuilder("canCreateAcademicYearAndTerms")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("Create"), "AcademicYearsAndTerms")
      .requireAll()
      .build(),
  );

  registry.register(
    new PolicyBuilder("canUpdateAcademicYearAndTerms")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("Update"), "AcademicYearsAndTerms")
      .requireAll()
      .build(),
  );

  registry.register(
    new PolicyBuilder("canDeleteAcademicYearAndTerms")
      .requireRole(
        createRole("Admin"),
        createRole("SystemAdmin"),
        createRole("Registrar"),
      )
      .requirePermission(createPermission("Delete"), "AcademicYearsAndTerms")
      .requireAll()
      .build(),
  );
};
