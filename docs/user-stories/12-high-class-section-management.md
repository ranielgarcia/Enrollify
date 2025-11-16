---
title: US-012 Class Section Management (CRUD)
id: US-012
epic: Scheduling
priority: high
create_by: Copilot
created_at: 2025-11-16T12:16:00Z
updated_at: 2025-11-16T12:16:00Z
---

## Summary

Admins create class sections per course/semester with adviser and capacity.

## Persona(s)

- Registrar / Scheduler

## User Story

As a Scheduler, I want to create sections with capacity and adviser, so offerings can be created and students grouped.

## Acceptance Criteria

1. Given valid fields When I create Then it saves (201) with YearLevel 1–6 and StudentCapacity > 0.
2. Given invalid YearLevel or capacity <= 0 When I create Then 400 per checks.
3. Given sections exist When I list Then filter by course, semester, adviser.

## Definition of Done

- [ ] CRUD UI/API
- [ ] Enforce FKs to Course, Semester, Teacher (Adviser)
- [ ] Respect checks and audit

## API / Back-end Notes

- Endpoints: /api/sections
- DB: ClassSections(Id, Name, YearLevel, CourseId, SemesterId, AdviserId, StudentCapacity, audit, IsActive)

## UI Notes

- Fields: Name, YearLevel, Course, Semester, Adviser, Capacity, Active.

## Related Requirements / Source

- SQL: ClassSections; Core models (Section).
