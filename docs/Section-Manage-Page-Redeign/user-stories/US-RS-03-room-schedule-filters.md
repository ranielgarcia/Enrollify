---
title: US-RS-03 Room Schedule Filters & Controls
id: US-RS-03
epic: Section Manage Page Redesign
priority: high
create_by: Copilot
created_at: 2026-06-10T00:00:00Z
updated_at: 2026-06-10T00:00:00Z
---

## Summary

Build the filter bar for the room schedule page with multi-select dropdowns for Building, Room Type, College, and Course. Filters are URL-synced via `nuqs` with Apply and Clear All controls.

## Persona(s)

- Scheduler / Administrator

## User Story

As a Scheduler, I want to filter the room schedule by building, room type, college, and course, so that I can focus on specific areas of campus or specific programs.

## Acceptance Criteria

1. Given the filter bar When rendered Then it shows dropdowns for Building, Room Type, College, and Course.
2. Given a filter dropdown When opened Then it shows multi-select checkboxes for available options.
3. Given filters selected When the user clicks [Apply] Then the schedule refetches with the selected filters.
4. Given filters active When the user clicks [Clear All] Then all filters reset and the full schedule reloads.
5. Given filter state When applied Then the URL search params update via `nuqs`.
6. Given the stats bar When filters are active Then it updates to reflect filtered dataset (not full dataset).
7. Given no rooms match the active filters When the schedule updates Then an empty state is shown with suggestions to widen filters.

## Definition of Done

- [ ] `ScheduleFilters` component with multi-select dropdowns
- [ ] Building, Room Type, College, Course filter controls
- [ ] Filter state URL-synced via `nuqs`
- [ ] Apply and Clear All buttons
- [ ] Stats bar updates based on filtered results
- [ ] Debounced filter changes (300ms)

## Preconditions & Assumptions

- Backend accepts filter params: `building[]`, `roomType[]`, `college[]`, `course[]`.
- Available filter options are loaded from existing master data endpoints.

## Business Rules / Validation

- Filters are combined with AND logic (building X AND room type Y).
- Within a filter category, selections are combined with OR logic (building A OR building B).
- Clear All resets all filter categories.

## API / Back-end Notes

- `POST /api/rooms/schedule` accepts filter payload:
  ```json
  { "academicYearId": 1, "building": ["Main", "Annex"], "roomType": ["Lecture", "Lab"], "college": ["Engineering"], "course": ["BSCS", "BSIT"], "dayOfWeek": "Monday" }
  ```
- Available filter options from existing endpoints: `GET /api/buildings`, `GET /api/room-types`, `GET /api/colleges`, `GET /api/courses`.

## UI Notes

- Filter bar layout: horizontal row of dropdowns + Apply + Clear All.
- Each dropdown shows selected count (e.g., "Building (2)").
- Checkbox list inside dropdown with search for long lists.
- Stats bar below filters: "Showing: 187 rooms | 312 offerings | 42 conflicts | 5,243 student capacity".

## Edge Cases & Error Handling

- Filters return 0 rooms → empty state: "No rooms match your current filters. Try selecting different buildings or room types."
- Network error on filter fetch → error state with retry.
- All filters cleared → full unfiltered schedule reloads.

## Test Cases

1. Select buildings + room type → schedule filters correctly.
2. Apply filters → stats bar updates with filtered counts.
3. Clear All → filters reset, full schedule loads.
4. URL updates with filter params on Apply.
5. Page reload preserves filter state from URL.

## Dependencies

- US-RS-01 (for core table and data fetching)

## Related Requirements / Source

- `room-scheduler-page-design.md` §4 (Filters & Controls)
- `room-scheduler-page-design.md` §11 (Phase 3)

## Notes / Implementation Considerations

- Use `nuqs` with `useQueryStates` for URL-synced filter state.
- Day of Week selector (Monday–Friday) as additional filter — default to current day.
- Consider persisting last-used filters in `sessionStorage`.
