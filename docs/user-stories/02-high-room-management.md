---
title: US-002 Room Management (CRUD)
id: US-002
epic: Master Data Management
priority: high
create_by: Copilot
created_at: 2025-11-16T12:02:00Z
updated_at: 2025-11-16T12:02:00Z
---

## Summary

Administrators can manage rooms with capacity and type to support scheduling and capacity checks.

## Persona(s)

- System Administrator
- Scheduler / Curriculum Planner

## User Story

As a Scheduler, I want to manage rooms with their capacities and types, so that classes can be assigned to appropriate spaces without overbooking.

## Acceptance Criteria

1. Given a valid room name, capacity > 0, and existing room type When I save Then a room record is created (201).
2. Given an existing room When I update capacity to <= 0 Then the system rejects with 400 due to CHK_Rooms_StudentCapacity_Positive.
3. Given rooms exist When I list Then I can filter by room type and active status.
4. Given a room with scheduled offerings When I try to delete Then hard delete is blocked; soft archive IsActive = 0 succeeds with 200 and warning.
5. Given a duplicate room name (same building convention) When I save Then system should allow duplicates unless a separate uniqueness policy is defined (Assumption), but must always require RoomTypeId.

## Definition of Done

- [ ] CRUD UI and list with filters
- [ ] API with validation and tests
- [ ] Enforces FK to RoomTypes
- [ ] Audit fields populated
- [ ] RBAC (Admin, Registrar)

## Preconditions & Assumptions

- No unique constraint on Name in SQL; allow institutional naming conventions.
- Capacity must be positive; IsActive used for soft delete.

## Business Rules / Validation

- StudentCapacity > 0.
- RoomTypeId must reference an active room type.
- Prevent scheduling into inactive rooms.

## API / Back-end Notes

- Endpoints: /api/rooms (GET, POST), /api/rooms/{id} (GET, PUT, DELETE)
- DB: Rooms(Id, Name, StudentCapacity, RoomTypeId FK, audit fields, IsActive)

## UI Notes

- Fields: Name (text), Type (select from RoomTypes), Capacity (number > 0), Active.
- Show current and upcoming schedules in details panel (read-only).

## Edge Cases & Error Handling

- Attempt to assign a schedule to inactive room -> 400.
- Delete a room referenced by future offering -> block hard delete; allow archive.

## Test Cases

1. Create valid room -> 201.
2. Create with capacity 0 -> 400.
3. Archive a room with references -> 200, IsActive = 0.

## Dependencies

- RoomTypes, ClassSectionSubjectOffering, ClassSchedules.

## Related Requirements / Source

- SQL: Rooms table in sql-queries/Initial-Tables.sql
- Domain: Core Domain Models (Room)

## Notes / Implementation Considerations

- Consider “building/floor” metadata in future; for now keep minimal fields.
