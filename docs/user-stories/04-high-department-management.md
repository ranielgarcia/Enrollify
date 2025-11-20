---
title: US-004 Department Management (CRUD)
id: US-004
epic: Master Data Management
priority: high
create_by: Copilot
created_at: 2025-11-16T12:06:00Z
updated_at: 2025-11-16T12:06:00Z
---

## Summary

Admins manage Departments under a College with unique (Code, CollegeId) to organize teachers and courses.

## Persona(s)

- System Administrator
- Department Head / Dean
- Registrar

## User Story

As an Admin, I want to manage Departments under the correct College, so that faculty and courses are properly grouped.

## Acceptance Criteria

1. Given a college exists When I create a department with a unique code for that college Then it saves (201).
2. Given an existing department When I update its CollegeId Then the unique (Code, CollegeId) must still be unique, else 409.
3. Given departments exist When I list Then I can filter by College and Active.
4. Given a department with teachers When I attempt to hard-delete Then system blocks; archive allowed.

## Definition of Done

- [ ] CRUD UI and API
- [ ] Enforce unique (Code, CollegeId)
- [ ] Soft delete and audit fields
- [ ] RBAC enforced (Admin, Dean)

## Business Rules / Validation

- Code required (<=10); Name required (<=100).
- Chairperson stored as text for now.

## API / Back-end Notes

- Endpoints: /api/departments (GET, POST), /api/departments/{id} (GET, PUT, DELETE)
- DB: Departments(Id, Code, Name, Chairperson, CollegeId FK, unique(Code,CollegeId), audit, IsActive)

## UI Notes

- Fields: College (select), Code, Name, Chairperson, Active.

## Edge Cases & Error Handling

- Duplicate composite key -> 409.
- College not found -> 404.

## Test Cases

1. Create with unique combo -> 201.
2. Duplicate per same college -> 409.

## Dependencies

- Teachers, Courses may reference Departments/Colleges.

## Related Requirements / Source

- SQL: Departments table
- Requirements: Core Domain Models

## Notes / Implementation Considerations

- Consider future constraint to ensure courses of a department align with the College.
