---
title: US-007 Subject Prerequisite Management
id: US-007
epic: Curriculum & Scheduling
priority: medium
create_by: Copilot
created_at: 2025-11-16T12:09:00Z
updated_at: 2025-11-16T12:09:00Z
---

## Summary

Administrators can link subjects as prerequisites so the enrollment engine can enforce prerequisite checks.

## Persona(s)

- Registrar / Program Coordinator
- System Administrator

## User Story

As a Program Coordinator, I want to define prerequisite relationships between subjects, so the system can prevent enrollment into subjects when prerequisites are not completed.

## Acceptance Criteria

1. Given two existing subjects When I create a prerequisite mapping Then it saves (201) and prevents self-reference.
2. Given a mapping exists When I create a duplicate active mapping Then the unique index prevents duplicate (returns 409).
3. Given a student lacking prerequisite When they attempt to enroll Then enrollment is blocked with a clear message.

## Definition of Done

- [ ] UI to add/remove prerequisites
- [ ] API with CHK_SubjectPrerequisites_NoSelfReference upheld
- [ ] Enrollment validation uses prerequisites
- [ ] Tests for mapping and enrollment enforcement

## Business Rules / Validation

- No self-reference allowed (SourceSubjectId <> PrerequisiteSubjectId).
- Only one active mapping per pair when IsActive = 1.

## API / Back-end Notes

- Endpoints: /api/subjects/{id}/prerequisites (POST, GET, DELETE)
- DB: SubjectPrerequisites(SourceSubjectId, PrerequisiteSubjectId PK composite, audit, IsActive)

## UI Notes

- Show prerequisite list on Subject details; allow add by search and mapping reason.

## Edge Cases & Error Handling

- Attempt to map subject to itself -> 400.
- Duplicate active mapping -> 409.

## Test Cases

1. Create mapping A->B -> 201.
2. Create B->B -> 400.
3. Student without B attempts to enroll A -> blocked.

## Dependencies

- Subjects, Enrollments, Enrollment validation logic.

## Related Requirements / Source

- SQL: SubjectPrerequisites table and unique index.
