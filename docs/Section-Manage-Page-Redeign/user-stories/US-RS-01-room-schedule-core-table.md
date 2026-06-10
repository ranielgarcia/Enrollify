---
title: US-RS-01 Room Schedule Core Table & Layout
id: US-RS-01
epic: Section Manage Page Redesign
priority: high
create_by: Copilot
created_at: 2026-06-10T00:00:00Z
updated_at: 2026-06-10T00:00:00Z
---

## Summary

Build the core room schedule page with a room × time slot table layout, sticky headers (left columns + top time row), time column generation, and room row rendering. Fetch room + offering data from a dedicated backend endpoint.

## Persona(s)

- Scheduler / Administrator

## User Story

As a Scheduler, I want to see a grid of rooms by time slots with sticky headers, so that I can visualize room utilization across campus at a glance.

## Acceptance Criteria

1. Given the page loads When room schedule data is returned Then a grid renders with rooms as rows and time slots as columns.
2. Given the grid When scrolling horizontally Then room number, type, and building columns remain sticky (fixed left).
3. Given the grid When scrolling vertically Then time column headers remain sticky at top.
4. Given time slots When rendered Then they show hourly intervals from 7:00 AM to 6:00 PM.
5. Given the room rows When rendered Then each row shows room number, room type, building name, and hasConflicts indicator.
6. Given the academic year selector When changed Then the schedule refetches for the selected year.
7. Given the stats bar When rendered Then it shows total rooms, total offerings, total conflicts, and total student capacity.

## Definition of Done

- [ ] New route for room schedule page (e.g., `/scheduling/rooms`)
- [ ] `RoomScheduleTable` component with sticky headers
- [ ] Time column header generation (hourly intervals)
- [ ] Room row rendering with room info columns
- [ ] Backend endpoint `GET /api/rooms/schedule` returning rooms + offerings + conflicts
- [ ] Stats bar with room/offering/conflict/capacity counts
- [ ] Academic Year selector

## Preconditions & Assumptions

- Backend endpoint `GET /api/rooms/schedule` exists with query params for building, roomType, college, course, dayOfWeek.
- Rooms, offerings, and schedules data exist.

## Business Rules / Validation

- Time slot granularity: 1-hour columns (7:00 AM, 8:00 AM, ... 6:00 PM).
- Lunch break (12:00-1:00 PM) rendered as gray cells.
- Offerings spanning multiple columns span across column boundaries.

## API / Back-end Notes

- `POST /api/rooms/schedule` — accepts filters, returns rooms with offerings and conflicts.
- Response shape (see `room-scheduler-page-design.md` §7.2):
  ```json
  {
    "rooms": [{ "id", "roomNumber", "building", "roomType", "capacity", "hasConflicts", "offerings": [...] }],
    "stats": { "totalRooms", "totalOfferings", "totalConflicts", "totalStudentCapacity" }
  }
  ```

## UI Notes

- Grid layout: horizontal scroll for time columns, vertical scroll for room rows.
- Left sticky columns: Room #, Type, Building.
- Top sticky row: time headers (7:00 AM, 8:00 AM, ...).
- Stats bar at bottom: "187 rooms scheduled | 42 conflicts detected | 5,243 student capacity".

## Edge Cases & Error Handling

- No rooms found → empty state: "No rooms found for the selected filters."
- API error → error state with retry button.
- Room with no offerings → empty cells for all time slots.

## Test Cases

1. Grid renders with correct number of room rows and time columns.
2. Left columns stay fixed when scrolling horizontally.
3. Time header stays fixed when scrolling vertically.
4. Room with conflicts shows `hasConflicts: true` indicator.
5. Stats bar shows correct aggregate counts.

## Dependencies

- Existing room, offering, and schedule data models.

## Related Requirements / Source

- `room-scheduler-page-design.md` §1-3 (Overview, Layout, Wireframes)
- `room-scheduler-page-design.md` §6.3 (Scroll Behavior)
- `room-scheduler-page-design.md` §11 (Implementation Phases — Phase 1)

## Notes / Implementation Considerations

- Use CSS `position: sticky` for sticky headers.
- Consider virtualization for 200+ rooms (TanStack Virtual).
- Time column width based on viewport; min-width 80px per hour.
- Lunch break cells are non-interactive with gray background.
