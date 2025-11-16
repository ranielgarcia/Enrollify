---
title: US-011 Semester Management (CRUD)
id: US-011
epic: Academic Terms & Calendars
priority: high
create_by: Copilot
created_at: 2025-11-16T12:15:00Z
updated_at: 2025-11-16T12:15:00Z
---

## Summary

Admins manage academic terms (semester and school year) used by sections, enrollments, and reports.

## Persona(s)

- Registrar / System Administrator

## User Story

As a Registrar, I want to create and manage semesters, so enrollments and schedules are tied to the correct term.

## Acceptance Criteria

1. Given Semester in (1,2,3) and SchoolYear >= 2000 When I create Then it saves (201).
2. Given invalid semester value When I create Then 400 per CHK_Semesters_Semester_Valid.
3. Given semesters exist When I list Then filter by active and school year.

## Definition of Done

- [ ] CRUD UI/API with tests
- [ ] Validation per checks
- [ ] Audit fields & RBAC

## API / Back-end Notes

- Endpoints: /api/semesters
- DB: Semesters(Id, Semester, Name, Description, SchoolYear, audit, IsActive)

## UI Notes

- Fields: Semester (1/2/3), Name, Description, SchoolYear, Active.

## Related Requirements / Source

- SQL: Semesters table; Development Phases (Phase 1, setup).
