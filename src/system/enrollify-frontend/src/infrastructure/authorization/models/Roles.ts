// This must stay in sync with the RolesEnum in the backend.
export const Roles = {
  SystemAdmin: 1,
  Admin: 2,
  Registrar: 3,
  FinanceOfficer: 4,
  AdmissionOfficer: 5,
  DepartmentHead: 6,
  AcademicAdvisor: 7,
  ScholarshipCoordinator: 8,
  Teacher: 9,
  ProgramCoordinator: 10,
  Student: 11,
  ParentOrGuardian: 12,
} as const;

export type RoleName = keyof typeof Roles;
export type RoleId = (typeof Roles)[RoleName];

export interface Role {
  id: RoleId;
  name: RoleName;
}
