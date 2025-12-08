import { type Scope } from "../models/AuthorizationScope";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerDefaultPolicies = (registry: PolicyRegistry) => {
  // Simple role-based policy
  registry.register(
    new PolicyBuilder("AdminOnly").requireRole("Admin", "SuperAdmin").build()
  );

  // Permission-based policy
  registry.register(
    new PolicyBuilder("CanManageStudents")
      .requirePermission("students.manage")
      .build()
  );

  // Scoped permission policy
  registry.register(
    new PolicyBuilder("CanManageDepartmentStudents")
      .requirePermission("students.manage")
      .requireScope({ name: "None", id: 0 } as Scope)
      .build()
  );

  // Complex multi-requirement policy (AND)
  registry.register(
    new PolicyBuilder("EnrollmentManager")
      .requireRole("Registrar", "Dean")
      .requirePermission("enrollment.approve")
      .requireAll()
      .build()
  );

  // Alternative requirements policy (OR)
  registry.register(
    new PolicyBuilder("CanViewReports")
      .requireRole("Admin", "Registrar", "Dean")
      .requireAny()
      .build()
  );

  // Department-scoped policy
  registry.register(
    new PolicyBuilder("DepartmentAdmin")
      .requireRole("DepartmentHead")
      .requireScope({ name: "None", id: 0 } as Scope)
      .requirePermission("department.manage")
      .build()
  );
};
