---
title: US-010 Teacher Subject Assignments
id: US-010
epic: Scheduling
priority: medium
create_by: Copilot
created_at: 2025-11-16T12:13:00Z
updated_at: 2025-11-16T12:13:00Z
---

## Summary

Assign subjects a teacher is qualified to teach; used to constrain scheduling.

## Persona(s)

- Department Head / Program Coordinator

## User Story

As a Program Coordinator, I want to record which subjects a teacher can teach, so that schedule creation and offering assignment respects qualifications.

## Acceptance Criteria

1. Given a teacher and subject exist When I add assignment Then it saves (201).
2. Given duplicate assignment When I add again Then either 409 or server ignores duplicates (idempotent) — choose policy.
3. Given assignments exist When I create an offering Then UI suggests only qualified teachers.

## Definition of Done

- [ ] UI to add/remove teacher-subject mappings
- [ ] API and tests
- [ ] Offering form filters teachers by mapping

## Business Rules / Validation

- Only active teachers/subjects can be mapped.

## API / Back-end Notes

- Endpoints: /api/teachers/{id}/subjects
- DB: TeacherSubjects(Id, TeacherId FK, SubjectId FK, audit, IsActive)

## UI Notes

- Search subjects by code/title; show current assignments as tags.

## Edge Cases & Error Handling

- Attempt to assign inactive subject/teacher -> 400.

## Test Cases

1. Add mapping -> 201.
2. Duplicate mapping -> 409/idempotent 200 depending on policy.

## Dependencies

- Teachers, Subjects, Offerings UI.

## Related Requirements / Source

- SQL: TeacherSubjects table.
