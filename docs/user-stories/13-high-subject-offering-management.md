---
title: US-013 Subject Offering Management (per Section)
id: US-013
epic: Scheduling
priority: high
create_by: Copilot
created_at: 2025-11-16T12:17:00Z
updated_at: 2025-11-16T12:17:00Z
---

## Summary

Create class section subject offerings assigning subject, teacher, room, and capacity overrides.

## Persona(s)

- Scheduler / Registrar

## User Story

As a Scheduler, I want to create subject offerings per section with teacher and room, so schedules and enrollments can be created.

## Acceptance Criteria

1. Given valid SubjectId, ClassSectionId, TeacherId, RoomId When I save Then offering is created (201).
2. Given MaxNumberOfStudents set When value < 1 Then 400.
3. Given offering exists When I create ClassSchedules for multiple days Then unique per day is enforced; time Start < End.

## Definition of Done

- [ ] Offering form and API
- [ ] Validations and FKs enforced
- [ ] Soft delete and audit fields respected

## API / Back-end Notes

- Endpoints: /api/offerings
- DB: ClassSectionSubjectOffering(Id, SubjectId FK, TeacherId FK, ClassSectionId FK, RoomId FK, DayPattern, DaysPerWeek, HoursPerDay, MaxNumberOfStudents, audit, IsActive)

## UI Notes

- Fields: Subject, Section, Teacher, Room, DayPattern, DaysPerWeek, HoursPerDay, MaxNumberOfStudents.
- Provide teacher and room availability hints using ClassSchedules.

## Related Requirements / Source

- SQL: ClassSectionSubjectOffering; Phase 2 (Scheduling & enrollment).
