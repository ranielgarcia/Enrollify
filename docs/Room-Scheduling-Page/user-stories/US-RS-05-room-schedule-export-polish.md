---
title: US-RS-05 Room Schedule Export, Legend & Polish
id: US-RS-05
epic: Section Manage Page Redesign
priority: medium
create_by: Copilot
created_at: 2026-06-10T00:00:00Z
updated_at: 2026-06-10T00:00:00Z
---

## Summary

Add CSV export functionality, the schedule legend component, responsive mobile view, and performance optimization (memoization, lazy loading, virtualization) for the room schedule page.

## Persona(s)

- Scheduler / Administrator

## User Story

As a Scheduler, I want to export the room schedule as CSV, see a course color legend, and have the page perform well with many rooms, so that I can analyze data offline and navigate the page smoothly.

## Acceptance Criteria

1. Given the export button When clicked Then a CSV file is downloaded containing Room Number, Building, Type, Capacity, Offering Code, Section, Subject, Teacher, Time Start, Time End, Conflicts.
2. Given the CSV export When triggered Then it exports the currently filtered dataset (respects active filters).
3. Given the legend When rendered at page bottom Then it shows all courses with their assigned color swatches.
4. Given more than 6 courses When rendered in the legend Then a `[More ▾]` expander shows remaining courses.
5. Given a screen size below 1024px When viewed Then the page switches to a single-room detail view with a room picker or a course/program list view.
6. Given 200+ rooms When the page loads Then room rows are virtualized with TanStack Virtual.
7. Given offering cards When the user scrolls Then they are memoized to prevent unnecessary re-renders.

## Definition of Done

- [ ] CSV export button + backend endpoint or client-side generation
- [ ] `ScheduleLegend` component with color swatches and expander
- [ ] Responsive/mobile layout (single-room picker or course list view)
- [ ] Virtual scrolling for room rows (TanStack Virtual)
- [ ] Memoization of offering cards (`React.memo` or `useMemo`)
- [ ] Performance tested with 500+ rooms

## Preconditions & Assumptions

- Responsive design only for mobile fallback — primary design is desktop.
- TanStack Virtual is available.

## Business Rules / Validation

- Export respects active filters (only exports visible data).
- CSV format compatible with Excel/Google Sheets.
- Mobile view shows room picker dropdown at top, then the schedule for that room only.

## API / Back-end Notes

- `POST /api/rooms/schedule/export` — returns CSV file.
- Accepts same filter payload as `POST /api/rooms/schedule`.
- Response: `Content-Type: text/csv`, `Content-Disposition: attachment`.

## UI Notes

- Legend at bottom: colored squares with course code labels, `[More ▾]` if >6.
- Mobile: room selector dropdown, single-room schedule table (scrollable).
- Export button in filter bar area.
- Loading skeleton for initial page load.

## Edge Cases & Error Handling

- Export endpoint fails → show error toast with retry.
- No data to export → disable export button, tooltip: "No data to export."
- Very large export (>10k rows) → show progress indicator or background download.

## Test Cases

1. Export button downloads CSV with correct columns for filtered dataset.
2. Legend shows all courses with color swatches.
3. Legend [More ▾] expands to show remaining courses.
4. Mobile view (<1024px) shows single-room picker.
5. 200+ rooms load without performance degradation.

## Dependencies

- US-RS-01 (for core table)
- US-RS-02 (for offering cards and color coding)
- US-RS-03 (for filter-aware export)

## Related Requirements / Source

- `room-scheduler-page-design.md` §6.4 (Export & Analysis)
- `room-scheduler-page-design.md` §9.6 (Legend & Visual Indicators)
- `room-scheduler-page-design.md` §9.4 (Performance Considerations)
- `room-scheduler-page-design.md` §9.5 (Mobile / Responsive Design)
- `room-scheduler-page-design.md` §11 (Phase 5)

## Notes / Implementation Considerations

- CSV generation can be client-side (from fetched data) or server-side via export endpoint.
- For mobile: consider a simplified table showing Course × Time for the selected room.
- Virtual scrolling: fixed row height for room rows; dynamic column count for time slots.
- Memoize offering cards with `React.memo` and compare by `offering.id` + `conflicts.length`.
