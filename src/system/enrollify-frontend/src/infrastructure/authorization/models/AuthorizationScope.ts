// This must stay in sync with the RolesEnum in the backend.
export const Scopes = {
  None: 0,
  Users: 1,
  Roles: 2,
  Rooms: 3,
  RoomTypes: 4,
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
