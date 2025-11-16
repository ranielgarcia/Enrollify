---
title: US-018 Enrollment Management (Subject-level Enrollments)
id: US-018
epic: Enrollment
priority: high
create_by: Copilot
created_at: 2025-11-16T12:29:00Z
updated_at: 2025-11-16T12:29:00Z
---

## Summary

Create and manage student enrollments into specific subject offerings per semester; supports regular (section-based) and irregular (manual per-offering) flows.

## Persona(s)

- Admissions Officer
- Registrar

## User Story

As an Admissions Officer, I want to enroll a student into available offerings for the current term, so their subjects, schedules, and assessments can be processed.

## Acceptance Criteria

1. For regular students: selecting a ClassSection auto-lists section offerings to enroll; saving creates one Enrollments row per offering.
2. For irregular/transferee: officer can search ANY open offering by subject/code/teacher/time and add individually.
3. Validate: prerequisites completed (or advisor/registrar override), no duplicate enrollment for same offering, capacity not exceeded unless override by Dean/Registrar.
4. Validate: outstanding balance policy respected (Assessment clearance flag may allow exceptions).
5. Support status transitions: Pending -> Approved -> Enrolled; all transitions logged.

## API / Back-end Notes

- Endpoints: /api/enrollments (list/create), /api/students/{id}/enrollments
- DB: Enrollments(Id, StudentId FK, ClassSectionId NULL, ClassSectionSubjectOfferingId FK, SemesterId FK, Status FK, audit, IsActive)
- Constraints: optional ClassSectionId (null for irregular), FK to EnrollmentStatuses, indexes on foreign keys.

## UI Notes

- Regular: choose section -> bulk select preset offerings; Irregular: add-by-search offerings one-by-one.
- Show conflicts with student’s timetable; show remaining seats per offering.

## Related Requirements / Source

- Requirements: Phase 2 Enrollment module (regular, irregular, transferee), prerequisite checks, subject opening request.
- SQL: Enrollments table.
