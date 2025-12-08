export const PolicyNames = {
  hasAnyRole: "hasAnyRole",
  canManageRoles: "canManageRoles",

  // temp
  AdminOnly: "AdminOnly",
  CanManageStudents: "CanManageStudents",
  CanManageDepartmentStudents: "CanManageDepartmentStudents",
  EnrollmentManager: "EnrollmentManager",
  CanViewReports: "CanViewReports",
  DepartmentAdmin: "DepartmentAdmin",
} as const;

export type PolicyName = keyof typeof PolicyNames;
