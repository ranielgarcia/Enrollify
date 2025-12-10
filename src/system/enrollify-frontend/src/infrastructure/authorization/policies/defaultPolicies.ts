import { createScope } from "../models/AuthorizationScope";
import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerDefaultPolicies = (registry: PolicyRegistry) => {
  // Simple role-based policy
  registry.register(
    new PolicyBuilder("AdminOnly").requireRole(createRole("Admin")).build()
  );

  // Permission-based policy
  registry.register(
    new PolicyBuilder("CanManageStudents")
      .requirePermission(createPermission("None"))
      .build()
  );

  // Scoped permission policy
  registry.register(
    new PolicyBuilder("CanManageDepartmentStudents")
      .requirePermission(createPermission("Update"))
      .requireScope(createScope("None"))
      .build()
  );

  // Complex multi-requirement policy (AND)
  registry.register(
    new PolicyBuilder("EnrollmentManager")
      .requireRole(createRole("Registrar"), createRole("FinanceOfficer"))
      .requirePermission(createPermission("Update"))
      .requireAll()
      .build()
  );

  // Alternative requirements policy (OR)
  registry.register(
    new PolicyBuilder("CanViewReports")
      .requireRole(createRole("Admin"), createRole("AdmissionOfficer"))
      .requireAny()
      .build()
  );

  // Department-scoped policy
  registry.register(
    new PolicyBuilder("DepartmentAdmin")
      .requireRole(createRole("DepartmentHead"))
      .requireScope(createScope("None"))
      .requirePermission(createPermission("Full"))
      .build()
  );
};
