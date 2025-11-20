---
title: US-005 Course (Program) Management (CRUD)
id: US-005
epic: Master Data Management
priority: high
create_by: Copilot
created_at: 2025-11-16T12:07:00Z
updated_at: 2025-11-16T12:07:00Z
---

## Summary

Admins manage Courses (Programs) with duration, preferred room type, and college linkage.

## Persona(s)

- System Administrator
- Registrar / Program Coordinator

## User Story

As an Admin, I want to create and manage Courses with duration and preferred room type, so scheduling and curriculum planning work consistently.

## Acceptance Criteria

1. Given a unique course code When I create a course Then it saves (201) with DurationYears within allowed range.
2. Given an existing course When I set DurationYears <= 0 or > 10 Then system rejects with 400 (CHK_Courses_DurationYears_Valid).
3. Given a course exists When I change PreferRoomTypeId to a non-existent/inactive type Then 400/409.
4. Given a course used by students/subjects When I delete Then hard delete blocked; archive allowed with warning.

## Definition of Done

- [ ] CRUD UI/API with tests
- [ ] Unique Code enforced
- [ ] Validate DurationYears and RoomType FK
- [ ] Audit fields & RBAC

## Business Rules / Validation

- Code required <=10 unique; Name required <=100.
- DurationYears: 1–10 inclusive.
- PreferRoomTypeId required and must be active.

## API / Back-end Notes

- Endpoints: /api/courses (GET, POST), /api/courses/{id} (GET, PUT, DELETE)
- DB: Courses(Id, Code UNIQUE, Name, DurationYears CHECK, CollegeId FK, PreferRoomTypeId FK, audit, IsActive)

## UI Notes

- Fields: Code, Name, College, DurationYears, Preferred Room Type, Description, Active.

## Edge Cases & Error Handling

- Duplicate Code -> 409.
- Duration out-of-range -> 400.

## Test Cases

1. Create valid course -> 201.
2. Duration 0 -> 400.
3. PreferRoomTypeId missing -> 400.

## Dependencies

- Subjects, Students, ClassSections reference Course.

## Related Requirements / Source

- SQL: Courses table; Core Domain Models (Course/Program)

## Notes / Implementation Considerations

- Consider curriculum linkage in future stories.
