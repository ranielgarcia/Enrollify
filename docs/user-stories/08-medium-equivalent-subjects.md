---
title: US-008 Equivalent Subject Mapping
id: US-008
epic: Curriculum & Scheduling
priority: medium
create_by: Copilot
created_at: 2025-11-16T12:10:00Z
updated_at: 2025-11-16T12:10:00Z
---

## Summary

Administrators can map equivalent subjects across colleges to support credit transfers and scheduling substitutions.

## Persona(s)

- Registrar
- Program Coordinator

## User Story

As a Registrar, I want to map equivalent subjects across colleges, so students can receive credit or be enrolled in alternatives when needed.

## Acceptance Criteria

1. Given two existing subjects When I create an equivalent mapping Then it saves (201) and is queryable during enrollment.
2. Given duplicate mapping active When attempt to insert Then unique index prevents it with 409.
3. Given a student needs a subject not offered Then registrar can suggest equivalent subjects from other colleges and enroll the student if approved.

## Definition of Done

- [ ] UI to create and search equivalents
- [ ] API and DB mappings with tests
- [ ] Enrollment flow uses equivalents during seat search

## Business Rules / Validation

- Mappings are directional (Source -> Equivalent) but can be reciprocal when created.

## API / Back-end Notes

- Endpoint: /api/subjects/{id}/equivalents
- DB: EquivalentSubjectMapping(SourceSubjectId, EquivalentSubjectId, Reason, audit, IsActive)

## UI Notes

- Show equivalents on subject details; provide reason and link to approve substitution.

## Edge Cases & Error Handling

- De-duplicate active mappings; archived mappings can be re-created.

## Test Cases

1. Create mapping -> 201; search returns mapping.
2. Use mapping in enrollment substitution flow -> student enrolled in equivalent offering.

## Dependencies

- Subjects, Enrollment module, Course equivalency policies.

## Related Requirements / Source

- SQL: EquivalentSubjectMapping table and index.
