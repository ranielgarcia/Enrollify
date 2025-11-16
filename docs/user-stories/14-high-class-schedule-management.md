---
title: US-014 Class Schedule Management & Conflict Detection
id: US-014
epic: Scheduling
priority: high
create_by: Copilot
created_at: 2025-11-16T12:18:00Z
updated_at: 2025-11-16T12:18:00Z
---

## Summary

Define per-day schedules for offerings and detect conflicts for teacher, room, and section.

## Persona(s)

- Scheduler / Registrar
- Teacher (view timetable)

## User Story

As a Scheduler, I want to add per-day time slots to offerings and be warned of conflicts, so teachers and rooms are not double-booked.

## Acceptance Criteria

1. Given a valid offering When I add schedule with DayOfWeek in allowed set and StartTime < EndTime Then it saves (201).
2. Given an offering already has a day entry When I add the same day Then unique constraint rejects it (409).
3. Given a teacher/room has overlapping time on the same day across offerings Then system blocks or warns with clear message before saving.
4. Given a teacher opens timetable view When requested Then they see their weekly schedule (read-only).

## Definition of Done

- [ ] UI for adding per-day schedule rows
- [ ] Conflict detection for teacher, room, and section across overlapping times
- [ ] Timetable view for teacher and section

## API / Back-end Notes

- Endpoints: /api/offerings/{id}/schedules (GET, POST, DELETE)
- DB: ClassSchedules(Id, ClassSectionSubjectOfferingId FK, DayOfWeek CHECK, StartTime < EndTime, UQ(Offering, Day))
- Conflict detection query should check overlap: (startA < endB) AND (startB < endA) within same DayOfWeek for same TeacherId or RoomId or ClassSectionId across active offerings.

## UI Notes

- Time pickers; per-day grid; warnings for collisions.

## Related Requirements / Source

- SQL: ClassSchedules constraints; Requirements item 12 (duplicate/conflict schedules).
