import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerDefaultPolicies = (registry: PolicyRegistry) => {
  // Simple role-based policy
  registry.register(
    new PolicyBuilder("AdminOnly").requireRole(createRole("Admin")).build()
  );

  // Permission-based policy - requires Update permission on Users scope
  registry.register(
    new PolicyBuilder("CanManageStudents")
      .requirePermission(createPermission("Update"), "Users")
      .build()
  );

  // Scoped permission policy - requires Update on Users scope
  registry.register(
    new PolicyBuilder("CanManageDepartmentStudents")
      .requirePermission(createPermission("Update"), "Users")
      .build()
  );

  // Complex multi-requirement policy (AND)
  registry.register(
    new PolicyBuilder("EnrollmentManager")
      .requireRole(createRole("Registrar"), createRole("FinanceOfficer"))
      .requirePermission(createPermission("Update"), "Users")
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

  // Department-scoped policy - requires DepartmentHead role AND Full permission on Users scope
  registry.register(
    new PolicyBuilder("DepartmentAdmin")
      .requireRole(createRole("DepartmentHead"))
      .requirePermission(createPermission("Full"), "Users")
      .build()
  );
};
