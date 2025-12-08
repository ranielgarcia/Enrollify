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

export interface Scope {
  id: ScopeId;
  name: ScopeName;
}

export interface AuthorizationScope {
  scope: Scope;
  metadata?: Record<string, string>;
}
