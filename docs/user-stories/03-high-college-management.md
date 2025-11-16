---
title: US-003 College Management (CRUD)
id: US-003
epic: Master Data Management
priority: high
create_by: Copilot
created_at: 2025-11-16T12:05:00Z
updated_at: 2025-11-16T12:05:00Z
---

## Summary

Admins manage Colleges with unique codes for organizing departments, courses, and governance.

## Persona(s)

- System Administrator
- Registrar / Enrollment Officer
- Department Chair / Dean

## User Story

As an Administrator, I want to create and manage Colleges with unique codes, so that departments and courses can be organized properly.

## Acceptance Criteria

1. Given a unique code and required fields When I create a college Then it is saved and returns 201.
2. Given an existing college When I update the code to a duplicate Then the system rejects with 409 Conflict.
3. Given colleges exist When I list Then I can filter by active and search by code/name.
4. Given a college with departments/courses When I attempt hard delete Then the system blocks; archive (IsActive = 0) succeeds with 200 and warning.

## Definition of Done

- [ ] CRUD UI and API with tests
- [ ] Unique code enforced
- [ ] Soft delete and audit fields
- [ ] RBAC for Admin/Registrar

## Preconditions & Assumptions

- Dean is stored as free text for now (per schema comment).

## Business Rules / Validation

- Code required, <=10, unique; Name required <=100.
- IsActive toggles visibility; archival preserves references.

## API / Back-end Notes

- Endpoints: /api/colleges (GET, POST), /api/colleges/{id} (GET, PUT, DELETE)
- DB: Colleges(Id, Code UNIQUE, Name, Dean, Description, audit, IsActive)

## UI Notes

- Fields: Code, Name, Dean, Description, Active.
- Show dependent counts (departments, courses) in details view.

## Edge Cases & Error Handling

- Duplicate Code -> 409.
- Blank/whitespace fields -> 400.

## Test Cases

1. Create valid -> 201.
2. Duplicate code -> 409.
3. Archive with dependencies -> 200 archived.

## Dependencies

- Departments, Courses reference Colleges.

## Related Requirements / Source

- SQL: Colleges in sql-queries/Initial-Tables.sql
- Requirements: Core Domain Models (College / Department)

## Notes / Implementation Considerations

- Consider future dean linkage to Users/Teachers table.
