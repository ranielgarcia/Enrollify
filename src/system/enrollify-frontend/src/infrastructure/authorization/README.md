# Policy-Based Authorization System

A type-safe, ASP.NET Core-inspired authorization system for React applications.

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [Quick Start](#quick-start)
- [Examples](#examples)
  - [Policy Registration](#example-1-policy-registration)
  - [Protecting Pages](#example-2-protecting-pages)
  - [Protecting Buttons/Actions](#example-3-protecting-buttonsactions)
  - [Using AuthorizeView](#example-4-using-authorizeview)
  - [Programmatic Authorization](#example-5-programmatic-authorization)
  - [Multiple Requirements](#example-6-multiple-requirements)
  - [Route Protection](#example-7-route-protection)
  - [Feature-Scoped Authorization](#example-8-feature-scoped-authorization)
  - [Entity-Level Authorization](#example-9-entity-level-authorization)

---

## Overview

This authorization system provides:

- **Policy-based authorization** - Named policies that encapsulate complex authorization logic
- **Role-based authorization** - Check if user has specific roles
- **Permission-based authorization** - Check if user has specific permissions (with bitwise flags)
- **Scoped authorization** - Check permissions within specific feature scopes (Rooms, Users, etc.)
- **Resource-based authorization** - Check permissions on specific entities

---

## Core Concepts

### Permissions (Bitwise Flags)

```typescript
import { createPermission, Permissions } from "./models/PermissionsEnum";

// Available permissions (synced with backend PermissionEnum.cs)
Permissions.None    // 0
Permissions.View    // 1
Permissions.Create  // 2
Permissions.Update  // 4
Permissions.Delete  // 8
Permissions.Full    // 15 (View | Create | Update | Delete)

// Create a type-safe permission
const viewPermission = createPermission("View");
const fullPermission = createPermission("Full");
```

### Roles

```typescript
import { createRole, Roles } from "./models/Roles";

// Available roles (synced with backend RolesEnum)
Roles.SystemAdmin        // 1
Roles.Admin              // 2
Roles.Registrar          // 3
Roles.FinanceOfficer     // 4
Roles.DepartmentHead     // 6
// ... etc

// Create a type-safe role
const adminRole = createRole("Admin");
const registrarRole = createRole("Registrar");
```

### Scopes (Feature Modules)

```typescript
import { Scopes, ScopeName } from "./models/AuthorizationScope";

// Available scopes (synced with backend PermissionScopeEnum)
Scopes.None      // 0
Scopes.Users     // 1
Scopes.Roles     // 2
Scopes.Rooms     // 3
Scopes.RoomTypes // 4

// Use scope names directly
const scope: ScopeName = "Rooms";
```

### Resources

```typescript
import { 
  AuthorizationResource, 
  createResource, 
  createEntityResource 
} from "./models/AuthorizationResource";

// Feature-level resource
const roomsResource = createResource("Rooms");
// { scope: "Rooms" }

// Entity-level resource (for future entity-specific checks)
const specificRoom = createEntityResource("Rooms", 42);
// { scope: "Rooms", entityId: 42 }
```

---

## Quick Start

### 1. Wrap your app with AuthorizationProvider

```tsx
// App.tsx
import { AuthorizationProvider } from "@/infrastructure/authorization";

function App() {
  return (
    <AuthenticationProvider>
      <AuthorizationProvider>
        <RouterProvider router={router} />
      </AuthorizationProvider>
    </AuthenticationProvider>
  );
}
```

### 2. Use the Authorized component or hook

```tsx
import { Authorized } from "@/infrastructure/authorization/components/Authorized";
import { useAuthorization } from "@/infrastructure/authorization/components/useAuthorization";
import { createRole, createPermission } from "@/infrastructure/authorization/models";

// Component-based
<Authorized policy="AdminOnly">
  <AdminPanel />
</Authorized>

// Hook-based
const { currentUserHasRole, currentUserHasPermission } = useAuthorization();
if (currentUserHasRole(createRole("Admin"))) {
  // show admin features
}
```

---

## Examples

### Example 1: Policy Registration

```typescript
// policies/defaultPolicies.ts
import { createPermission } from "../models/PermissionsEnum";
import { createRole } from "../models/Roles";
import { PolicyBuilder } from "./PolicyBuilder";
import type { PolicyRegistry } from "./PolicyRegistry";

export const registerDefaultPolicies = (registry: PolicyRegistry) => {
  // ═══════════════════════════════════════════════════════════════
  // ROLE-BASED POLICIES
  // ═══════════════════════════════════════════════════════════════

  // Simple: User must have Admin role
  registry.register(
    new PolicyBuilder("AdminOnly")
      .requireRole(createRole("Admin"))
      .build()
  );

  // Multiple roles: User must have ANY of these roles
  registry.register(
    new PolicyBuilder("CanViewReports")
      .requireRole(
        createRole("Admin"),
        createRole("Registrar"),
        createRole("DepartmentHead")
      )
      .requireAny()  // OR logic - any role satisfies
      .build()
  );

  // ═══════════════════════════════════════════════════════════════
  // PERMISSION-BASED POLICIES
  // ═══════════════════════════════════════════════════════════════

  // Permission with scope (scope is REQUIRED)
  registry.register(
    new PolicyBuilder("CanManageStudents")
      .requirePermission(createPermission("Update"), "Users")
      .build()
  );

  // Permission on specific scope
  registry.register(
    new PolicyBuilder("CanManageRooms")
      .requirePermission(createPermission("Update"), "Rooms")
      .build()
  );

  // Full permission (View + Create + Update + Delete)
  registry.register(
    new PolicyBuilder("CanFullyManageUsers")
      .requirePermission(createPermission("Full"), "Users")
      .build()
  );

  // ═══════════════════════════════════════════════════════════════
  // COMBINED POLICIES (Role + Permission)
  // ═══════════════════════════════════════════════════════════════

  // AND logic: Must have role AND permission
  registry.register(
    new PolicyBuilder("DepartmentAdmin")
      .requireRole(createRole("DepartmentHead"))
      .requirePermission(createPermission("Full"), "Users")
      .requireAll()  // AND logic - all requirements must pass
      .build()
  );

  // Complex: Multiple roles (ANY) AND specific permission
  registry.register(
    new PolicyBuilder("EnrollmentManager")
      .requireRole(
        createRole("Registrar"),
        createRole("FinanceOfficer")
      )
      .requirePermission(createPermission("Update"), "Users")
      .requireAll()
      .build()
  );
};
```

### Example 2: Protecting Pages

```tsx
// pages/RoomManagementPage.tsx
import { Authorized } from "@/infrastructure/authorization/components/Authorized";
import { createResource } from "@/infrastructure/authorization/models/AuthorizationResource";

// Basic policy protection
const RoomManagementPage = () => {
  return (
    <Authorized
      policy="CanManageRooms"
      fallback={<LoadingSpinner />}
      unauthorized={<AccessDenied message="You cannot manage rooms" />}
    >
      <RoomManagementContent />
    </Authorized>
  );
};

// With explicit resource scope
const RoomTypesPage = () => {
  return (
    <Authorized
      policy="CanManageRoomTypes"
      resource={createResource("RoomTypes")}
      fallback={<LoadingSpinner />}
      unauthorized={<AccessDenied />}
    >
      <RoomTypesContent />
    </Authorized>
  );
};

// Admin-only page
const SystemSettingsPage = () => {
  return (
    <Authorized
      policy="AdminOnly"
      unauthorized={<Navigate to="/" replace />}
    >
      <SystemSettings />
    </Authorized>
  );
};
```

### Example 3: Protecting Buttons/Actions

```tsx
// components/RoomActions.tsx
import { Authorized } from "@/infrastructure/authorization/components/Authorized";
import { createPermission } from "@/infrastructure/authorization/models/PermissionsEnum";
import { createRole } from "@/infrastructure/authorization/models/Roles";
import { createResource } from "@/infrastructure/authorization/models/AuthorizationResource";

interface RoomActionsProps {
  room: Room;
  onEdit: () => void;
  onDelete: () => void;
}

const RoomActions: React.FC<RoomActionsProps> = ({ room, onEdit, onDelete }) => {
  const roomsResource = createResource("Rooms");

  return (
    <div className="flex gap-2">
      {/* View button - requires View permission on Rooms */}
      <Authorized
        permissions={[createPermission("View")]}
        resource={roomsResource}
      >
        <Button variant="outline" onClick={() => viewRoom(room)}>
          <Eye className="h-4 w-4" />
          View
        </Button>
      </Authorized>

      {/* Edit button - requires Update permission on Rooms */}
      <Authorized
        permissions={[createPermission("Update")]}
        resource={roomsResource}
      >
        <Button onClick={onEdit}>
          <Pencil className="h-4 w-4" />
          Edit
        </Button>
      </Authorized>

      {/* Delete button - requires Delete permission OR Admin role */}
      <Authorized
        permissions={[createPermission("Delete")]}
        resource={roomsResource}
      >
        <Button variant="destructive" onClick={onDelete}>
          <Trash className="h-4 w-4" />
          Delete
        </Button>
      </Authorized>

      {/* Admin-only action */}
      <Authorized roles={[createRole("Admin"), createRole("SystemAdmin")]}>
        <Button variant="ghost" onClick={() => auditRoom(room)}>
          <Shield className="h-4 w-4" />
          Audit
        </Button>
      </Authorized>
    </div>
  );
};
```

### Example 4: Using AuthorizeView

```tsx
// components/DashboardWidget.tsx
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";

// Show different content based on authorization
const ReportsWidget = () => {
  return (
    <AuthorizeView
      policy="CanViewReports"
      authorized={
        <Card>
          <CardHeader>
            <CardTitle>Reports Dashboard</CardTitle>
          </CardHeader>
          <CardContent>
            <ReportsChart />
          </CardContent>
        </Card>
      }
      unauthorized={
        <Card className="opacity-50">
          <CardHeader>
            <CardTitle>Reports Dashboard</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-muted-foreground">
              You don't have permission to view reports.
              Contact your administrator.
            </p>
          </CardContent>
        </Card>
      }
    />
  );
};

// Conditional rendering based on role
const AdminWidget = () => {
  return (
    <AuthorizeView
      roles={[createRole("Admin")]}
      authorized={<AdminDashboard />}
      unauthorized={null}  // Simply don't render
    />
  );
};
```

### Example 5: Programmatic Authorization

```tsx
// components/RoomForm.tsx
import { useAuthorization } from "@/infrastructure/authorization/components/useAuthorization";
import { createPermission } from "@/infrastructure/authorization/models/PermissionsEnum";
import { createRole } from "@/infrastructure/authorization/models/Roles";
import { createResource } from "@/infrastructure/authorization/models/AuthorizationResource";

const RoomForm = ({ room, onSave }: RoomFormProps) => {
  const { currentUserHasPermission, currentUserHasRole, checkPolicy } = useAuthorization();
  const roomsResource = createResource("Rooms");

  // Check permissions synchronously
  const canEdit = currentUserHasPermission(createPermission("Update"), roomsResource);
  const canDelete = currentUserHasPermission(createPermission("Delete"), roomsResource);
  const isAdmin = currentUserHasRole(createRole("Admin"));

  // Check policy asynchronously
  const handleSave = async (data: RoomData) => {
    const canManage = await checkPolicy("CanManageRooms", roomsResource);

    if (!canManage) {
      toast.error("You don't have permission to manage rooms");
      return;
    }

    // Proceed with save
    await onSave(data);
    toast.success("Room saved successfully");
  };

  const handleDelete = async () => {
    if (!canDelete) {
      toast.error("You don't have permission to delete rooms");
      return;
    }

    await deleteRoom(room.id);
    toast.success("Room deleted");
  };

  return (
    <form onSubmit={handleSubmit(handleSave)}>
      <Input
        {...register("name")}
        disabled={!canEdit}
        placeholder="Room Name"
      />

      <Input
        {...register("capacity")}
        disabled={!canEdit}
        type="number"
        placeholder="Capacity"
      />

      <div className="flex gap-2">
        {canEdit && (
          <Button type="submit">Save</Button>
        )}

        {canDelete && (
          <Button
            type="button"
            variant="destructive"
            onClick={handleDelete}
          >
            Delete
          </Button>
        )}

        {isAdmin && (
          <Button type="button" variant="outline" onClick={showAuditLog}>
            View Audit Log
          </Button>
        )}
      </div>
    </form>
  );
};
```

### Example 6: Multiple Requirements

```tsx
import { Authorized } from "@/infrastructure/authorization/components/Authorized";
import { createPermission } from "@/infrastructure/authorization/models/PermissionsEnum";
import { createRole } from "@/infrastructure/authorization/models/Roles";
import { createResource } from "@/infrastructure/authorization/models/AuthorizationResource";

// ═══════════════════════════════════════════════════════════════
// ROLE CHECKS
// ═══════════════════════════════════════════════════════════════

// Require ANY of the roles (OR logic)
<Authorized
  roles={[
    createRole("Admin"),
    createRole("SystemAdmin"),
    createRole("Registrar")
  ]}
  requireAll={false}  // Any role satisfies
>
  <AdminPanel />
</Authorized>

// Require ALL roles (unusual but supported)
<Authorized
  roles={[
    createRole("DepartmentHead"),
    createRole("Teacher")
  ]}
  requireAll={true}  // User must have BOTH roles
>
  <DepartmentTeacherView />
</Authorized>

// ═══════════════════════════════════════════════════════════════
// PERMISSION CHECKS
// ═══════════════════════════════════════════════════════════════

// Require ALL permissions
<Authorized
  permissions={[
    createPermission("View"),
    createPermission("Update"),
    createPermission("Delete")
  ]}
  resource={createResource("Rooms")}
  requireAll={true}  // Must have View AND Update AND Delete
>
  <AdvancedRoomManager />
</Authorized>

// Require ANY permission
<Authorized
  permissions={[
    createPermission("Update"),
    createPermission("Delete")
  ]}
  resource={createResource("Rooms")}
  requireAll={false}  // Must have Update OR Delete
>
  <RoomModifyButton />
</Authorized>

// ═══════════════════════════════════════════════════════════════
// COMBINED: ROLES + PERMISSIONS
// ═══════════════════════════════════════════════════════════════

// Must have role AND permission (default behavior)
<Authorized
  roles={[createRole("DepartmentHead")]}
  permissions={[createPermission("Full")]}
  resource={createResource("Users")}
>
  <DepartmentUserManagement />
</Authorized>
```

### Example 7: Route Protection

```tsx
// routes/ProtectedRoute.tsx
import { Authorized } from "@/infrastructure/authorization/components/Authorized";
import type { AuthorizationResource } from "@/infrastructure/authorization/models/AuthorizationResource";
import type { PolicyName } from "@/infrastructure/authorization/models/PolicyNames";
import type { Role } from "@/infrastructure/authorization/models/Roles";
import type { Permission } from "@/infrastructure/authorization/models/PermissionsEnum";
import { Navigate } from "react-router-dom";

interface ProtectedRouteProps {
  policy?: PolicyName;
  roles?: Role[];
  permissions?: Permission[];
  resource?: AuthorizationResource;
  element: React.ReactNode;
  redirectTo?: string;
}

export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  policy,
  roles,
  permissions,
  resource,
  element,
  redirectTo = "/unauthorized",
}) => {
  return (
    <Authorized
      policy={policy}
      roles={roles}
      permissions={permissions}
      resource={resource}
      fallback={<LoadingSpinner />}
      unauthorized={<Navigate to={redirectTo} replace />}
    >
      {element}
    </Authorized>
  );
};

// router.tsx
import { createBrowserRouter } from "react-router-dom";
import { createRole } from "@/infrastructure/authorization/models/Roles";
import { createPermission } from "@/infrastructure/authorization/models/PermissionsEnum";
import { createResource } from "@/infrastructure/authorization/models/AuthorizationResource";

const router = createBrowserRouter([
  {
    path: "/",
    element: <RootLayout />,
    children: [
      // Policy-protected route
      {
        path: "rooms",
        element: (
          <ProtectedRoute
            policy="CanManageRooms"
            element={<RoomManagementPage />}
          />
        ),
      },

      // Role-protected route
      {
        path: "admin",
        element: (
          <ProtectedRoute
            roles={[createRole("Admin"), createRole("SystemAdmin")]}
            element={<AdminDashboard />}
          />
        ),
      },

      // Permission-protected route with resource
      {
        path: "users",
        element: (
          <ProtectedRoute
            permissions={[createPermission("View")]}
            resource={createResource("Users")}
            element={<UserManagementPage />}
          />
        ),
      },

      // Complex protection
      {
        path: "settings",
        element: (
          <ProtectedRoute
            policy="AdminOnly"
            redirectTo="/dashboard"
            element={<SystemSettingsPage />}
          />
        ),
      },
    ],
  },
]);
```

### Example 8: Feature-Scoped Authorization

```tsx
// Feature scope ensures permission is checked within the correct module

import { useAuthorization } from "@/infrastructure/authorization/components/useAuthorization";
import { createPermission } from "@/infrastructure/authorization/models/PermissionsEnum";
import { createResource } from "@/infrastructure/authorization/models/AuthorizationResource";

const FeaturePage = () => {
  const { currentUserHasPermission } = useAuthorization();

  // Check permission within Rooms scope
  const canViewRooms = currentUserHasPermission(
    createPermission("View"),
    createResource("Rooms")
  );

  // Check permission within Users scope
  const canViewUsers = currentUserHasPermission(
    createPermission("View"),
    createResource("Users")
  );

  // Check permission within RoomTypes scope
  const canManageRoomTypes = currentUserHasPermission(
    createPermission("Update"),
    createResource("RoomTypes")
  );

  return (
    <div className="grid grid-cols-3 gap-4">
      {canViewRooms && <RoomsCard />}
      {canViewUsers && <UsersCard />}
      {canManageRoomTypes && <RoomTypesCard />}
    </div>
  );
};

// In policies - scope is baked into the policy definition
registry.register(
  new PolicyBuilder("CanManageRooms")
    .requirePermission(createPermission("Update"), "Rooms")  // Scoped!
    .build()
);

registry.register(
  new PolicyBuilder("CanManageRoomTypes")
    .requirePermission(createPermission("Update"), "RoomTypes")  // Scoped!
    .build()
);
```

### Example 9: Entity-Level Authorization

```tsx
// For future entity-specific authorization (e.g., "Can user edit Room #42?")

import { createEntityResource } from "@/infrastructure/authorization/models/AuthorizationResource";
import { Authorized } from "@/infrastructure/authorization/components/Authorized";

interface RoomDetailsProps {
  room: Room;
}

const RoomDetails: React.FC<RoomDetailsProps> = ({ room }) => {
  // Create a resource for this specific room
  const roomResource = createEntityResource("Rooms", room.id);
  // { scope: "Rooms", entityId: 42 }

  return (
    <div>
      <h1>{room.name}</h1>

      {/* Entity-specific authorization */}
      <Authorized
        policy="CanEditRoom"
        resource={roomResource}
      >
        <Button onClick={() => editRoom(room)}>
          Edit This Room
        </Button>
      </Authorized>
    </div>
  );
};

// Note: Entity-level authorization requires custom handlers
// that check if the user has access to the specific entity.
// This is typically used for ownership or assignment-based access.
```

---

## API Reference

### Components

| Component | Description |
|-----------|-------------|
| `<Authorized>` | Conditionally renders children based on authorization |
| `<AuthorizeView>` | Renders authorized or unauthorized content |
| `<AuthorizationProvider>` | Context provider for authorization |

### Hooks

| Hook | Description |
|------|-------------|
| `useAuthorization()` | Access authorization functions |

### Hook Return Values

```typescript
const {
  authorize,                  // (policy, resource?) => Promise<AuthorizationResult>
  checkPolicy,                // (policy, resource?) => Promise<boolean>
  currentUserHasRole,         // (...roles) => boolean
  currentUserHasPermission,   // (permission, resource?) => boolean
} = useAuthorization();
```

### Helper Functions

| Function | Description |
|----------|-------------|
| `createRole(name)` | Create a type-safe Role |
| `createPermission(name)` | Create a type-safe Permission |
| `createResource(scope)` | Create a feature-level resource |
| `createEntityResource(scope, entityId)` | Create an entity-level resource |

---

## Adding New Policies

1. Add the policy name to `PolicyNames`:

```typescript
// models/PolicyNames.ts
export const PolicyNames = {
  // ... existing
  CanManageSchedules: "CanManageSchedules",
} as const;
```

2. Register the policy:

```typescript
// policies/defaultPolicies.ts
registry.register(
  new PolicyBuilder("CanManageSchedules")
    .requireRole(createRole("Registrar"))
    .requirePermission(createPermission("Update"), "Schedules")
    .build()
);
```

3. Add the scope (if new):

```typescript
// models/AuthorizationScope.ts
export const Scopes = {
  // ... existing
  Schedules: 5,
} as const;
```

---

## Best Practices

1. **Use policies for complex logic** - Encapsulate multi-requirement checks in policies
2. **Use hooks for simple checks** - `currentUserHasRole()` and `currentUserHasPermission()` for inline checks
3. **Always specify scope for permissions** - Scope is required in `requirePermission(permission, scope)`
4. **Use `requireAll()` vs `requireAny()`** - Be explicit about AND/OR logic
5. **Provide fallback and unauthorized props** - Better UX during loading and access denied
