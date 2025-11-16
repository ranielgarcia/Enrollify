---
title: US-023 User, Role & Permission Management (RBAC)
id: US-023
epic: Platform & Security
priority: high
create_by: Copilot
created_at: 2025-11-16T12:45:00Z
updated_at: 2025-11-16T12:45:00Z
---

## Summary

Manage users, roles, and permissions so that access to modules (admissions, enrollment, cashier, registrar, scheduler, student/parent portal) is controlled.

## Persona(s)

- IT Administrator/System Admin

## User Story

As a System Admin, I want to manage users and roles with page/module permissions, so the right staff can access the right functions.

## Acceptance Criteria

1. Create users and assign roles; deactivate users (soft delete).
2. Roles define access to modules/pages (view/create/update/delete/export/approve) per requirement.
3. Audit: who created/updated users and role assignments.

## Notes

- Many tables reference Users(Id) for audit; enforce via auth context.

## Related Requirements / Source

- docs/administrative_roles.md; Phase 1 scope.
