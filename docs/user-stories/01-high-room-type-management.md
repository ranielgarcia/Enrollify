---
title: US-001 Room Type Management (CRUD)
id: US-001
epic: Master Data Management
priority: high
create_by: Copilot
created_at: 2025-11-16T12:00:00Z
updated_at: 2025-11-16T12:00:00Z
---

## Summary

Administrators can create, read, update, and archive room types used to categorize rooms (e.g., Lecture, Laboratory).

## Persona(s)

- System Administrator: manages master data and permissions.
- Registrar: needs accurate room type data for scheduling.

## User Story

As a System Administrator, I want to manage room types, so that rooms can be categorized correctly for scheduling and capacity rules.

## Acceptance Criteria

1. Given a unique room type name When I submit the create form Then the system saves the room type and returns 201 Created.
2. Given an existing room type When I update the name to another unique value Then the system saves changes and returns 200 OK.
3. Given an existing room type When I attempt to rename it to a name that already exists Then the system rejects the request with 409 Conflict and a validation message.
4. Given a room type in use by rooms When I attempt to delete it Then the system prevents hard delete and allows soft-archive (IsActive = 0) with 200 OK and usage warning.
5. Given room types exist When I view the list Then I can filter by active/inactive and search by name.

## Definition of Done

- [ ] UI form with validation (required, unique)
- [ ] API endpoints implemented with tests
- [ ] DB writes respect constraints and soft delete (IsActive)
- [ ] Audit fields (CreatedAt/By, UpdatedAt/By) set via auth context
- [ ] RBAC enforced (Admin only)
- [ ] Story documented and indexed

## Preconditions & Assumptions

- Room types are a prerequisite for creating rooms and courses (PreferRoomTypeId).
- Soft delete via IsActive; records are retained for audit.

## Business Rules / Validation

- Name is required, max 50 chars, unique.
- Cannot hard-delete if referenced; use IsActive = 0.

## API / Back-end Notes

- Endpoints: POST /api/room-types, GET /api/room-types, GET /api/room-types/{id}, PUT /api/room-types/{id}, DELETE /api/room-types/{id}
- Request (POST/PUT):
{
  "name": "Lecture",
  "isActive": true
}
- Response: 201/200 with entity payload.
- DB: RoomTypes(Id, Name UNIQUE, CreatedAt/By, UpdatedAt/By, DeletedAt/By, IsActive); FKs to Users.

## UI Notes

- Fields: Name (text, required, <=50), Active (checkbox), Audit (read-only).
- List with search and active filter; inline archive/restore.

## Edge Cases & Error Handling

- Duplicate name -> 409 Conflict.
- Archive a type in use -> allowed but show warning banner in UI.
- Attempt to set empty/whitespace name -> 400 Bad Request.

## Test Cases

1. Create with unique name -> 201, appears in list.
2. Create with duplicate name -> 409.
3. Archive type referenced by a room -> 200, IsActive = 0, room creation must still validate existing references.
4. Update name length > 50 -> 400.

## Dependencies

- Rooms, Courses (PreferRoomTypeId) depend on room types.

## Related Requirements / Source

- SQL: sql-queries/Initial-Tables.sql (RoomTypes)
- Domain: docs/requirements/Core Domain Models in a College Enrollment System.md (Room/RoomType)

## Notes / Implementation Considerations

- Consider optimistic concurrency with UpdatedAt token.
- Add index on Name (already unique) for search.
