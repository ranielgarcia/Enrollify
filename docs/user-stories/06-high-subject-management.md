---
title: US-006 Subject Management (CRUD)
id: US-006
epic: Curriculum & Scheduling
priority: high
create_by: Copilot
created_at: 2025-11-16T12:08:00Z
updated_at: 2025-11-16T12:08:00Z
---

## Summary

Admins manage Subjects per Course with code uniqueness per course and units validation.

## Persona(s)

- Registrar / Program Coordinator
- System Administrator

## User Story

As a Program Coordinator, I want to manage subjects per course with units and codes, so students can enroll correctly and prerequisites can be validated.

## Acceptance Criteria

1. Given a course exists When I create a subject with unique (Code, CourseId) and Units 1–12 Then it saves (201).
2. Given a subject exists When I set Units to 0 or > 12 Then the system rejects with 400 (CHK_Subjects_Units_Valid).
3. Given subjects exist When I list Then I can filter by Course and Active.
4. Given a subject has prerequisite/equivalents mappings When I archive Then mappings remain but subject is hidden from new offerings.

## Definition of Done

- [ ] CRUD UI/API with tests
- [ ] Enforce unique (Code, CourseId)
- [ ] Units validation 1–12
- [ ] Soft delete and audit fields

## Business Rules / Validation

- Code required <=10; Title required <=100; Units 1–12.
- Must belong to a Course.

## API / Back-end Notes

- Endpoints: /api/subjects (GET, POST), /api/subjects/{id} (GET, PUT, DELETE)
- DB: Subjects(Id, Code, Title, Units CHECK, CourseId FK, audit, IsActive), UQ(Code, CourseId)

## UI Notes

- Fields: Course (select), Code, Title, Units, Description, Active.

## Edge Cases & Error Handling

- Duplicate (Code, CourseId) -> 409.
- Units boundary values -> 400 on invalid.

## Test Cases

1. Create valid subject -> 201.
2. Duplicate per course -> 409.
3. Units 0 -> 400.

## Dependencies

- SubjectPrerequisites, EquivalentSubjectMapping, ClassSectionSubjectOffering.

## Related Requirements / Source

- SQL: Subjects table
- Requirements: Core Domain Models (Subject)

## Notes / Implementation Considerations

- Future: curriculum year/semester mapping and outcomes.
