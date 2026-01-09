// Reminder: Be mindful when changing the values of existing enums, as they are stored in the database and
// the value is considered an identifier (Id value).
// This enum must be kept in sync with `PermissionScopeEnum.cs` in the backend API
export const Scopes = {
  None: 0,
  Users: 1,
  Roles: 2,
  Colleges: 3,
  Rooms: 4,
  RoomTypes: 5,
  Buildings: 6,
  Departments: 7,
  Courses: 8,
  Subjects: 9,
  Curriculums: 10,
} as const;

export type ScopeName = keyof typeof Scopes;
export type ScopeId = (typeof Scopes)[ScopeName];

// Type-safe Scope that ensures id and name correspond correctly
export type Scope = {
  [K in ScopeName]: { id: (typeof Scopes)[K]; name: K };
}[ScopeName];

// Helper function to create a Scope from just the name
export const createScope = <T extends ScopeName>(
  name: T
): Extract<Scope, { name: T }> =>
  ({
    id: Scopes[name],
    name,
  }) as Extract<Scope, { name: T }>;

export interface AuthorizationScope {
  scope: Scope;
  metadata?: Record<string, string>;
}
