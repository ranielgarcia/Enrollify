---
title: US-RS-02 Room Schedule Offerings & Conflict Display
id: US-RS-02
epic: Section Manage Page Redesign
priority: high
create_by: Copilot
created_at: 2026-06-10T00:00:00Z
updated_at: 2026-06-10T00:00:00Z
---

## Summary

Render offering cards in each room × time cell, color-coded by course/program, with conflict visualization (red borders, stacking, warning labels). Support offering cards that span multiple time columns.

## Persona(s)

- Scheduler / Administrator

## User Story

As a Scheduler, I want to see color-coded offering cards in the room schedule grid with conflict indicators, so that I can easily identify scheduling conflicts and distinguish sections by course.

## Acceptance Criteria

1. Given an offering with no conflicts When rendered in a time cell Then it shows a colored card with section code, teacher name, and time range.
2. Given an offering with conflicts When rendered Then the card has a red border and a `⚠️ CONFLICT` label.
3. Given multiple offerings in the same time cell When rendered (conflict) Then they stack vertically with red borders.
4. Given an offering spanning multiple hours When rendered Then the card spans the corresponding number of columns.
5. Given different courses When rendered Then they use the assigned color scheme (BSCS=Green, BSIT=Blue, BSEE=Orange, BSA=Purple, BSBA=Pink, others=Gray).
6. Given lunch hours (12:00-1:00 PM) When rendered Then the cells show "LUNCH BREAK" with gray background (non-interactive).

## Definition of Done

- [ ] `OfferingCard` component with course color coding
- [ ] `ConflictIndicator` component with red border + warning label
- [ ] Multiple offering stacking in conflicted cells
- [ ] Col-span for multi-hour offerings
- [ ] `LunchBreakCell` component
- [ ] Color assignment per course (configurable mapping)
- [ ] `ScheduleLegend` component at bottom of page

## Preconditions & Assumptions

- Backend returns offerings with `startTime`, `endTime`, and `conflicts[]`.
- Color scheme defined in design doc (§9.1).

## Business Rules / Validation

- Conflict types: `ROOM_DOUBLE_BOOKED`, `TIME_OVERLAP`, `TEACHER_DOUBLE_BOOKED`.
- Conflicting offerings stacked vertically in same cell with red border.
- Multi-hour offerings span columns proportionally (e.g., 07:30-09:00 spans 1.5 columns).

## API / Back-end Notes

- Offering data from `GET /api/rooms/schedule` response.
- Conflicts pre-computed by backend per offering.

## UI Notes

- Offering card: `[Section Code] [Teacher] [Time Range]`, colored background by course.
- Conflict: red border + `⚠️ CONFLICT` label. If multiple offerings in cell, stack vertically.
- Legend at bottom: "■ BSCS ■ BSIT ■ BSEE ■ BSA ■ BSBA [More ▾]" with course colors.

## Edge Cases & Error Handling

- Offering without teacher → show "TBA" for teacher name.
- Offering without room → still render with "(no room)" note (theoretically shouldn't happen on this page).
- Very long section codes → truncate with ellipsis.

## Test Cases

1. Normal offering card renders with course color, no border.
2. Conflicted offering card renders with red border + warning.
3. Multiple conflicted offerings stack vertically in same cell.
4. Multi-hour offering spans correct number of columns.
5. Course color matches assigned mapping.
6. Lunch break cell renders with gray background.

## Dependencies

- US-RS-01 (for core table structure and data fetching)

## Related Requirements / Source

- `room-scheduler-page-design.md` §3.2 (Offering Card Cell Detail)
- `room-scheduler-page-design.md` §5 (Conflict Visualization)
- `room-scheduler-page-design.md` §9.1 (Color Coding Scheme)
- `room-scheduler-page-design.md` §11 (Phase 2)

## Notes / Implementation Considerations

- Color mapping stored in config or CSS custom properties.
- Column span calculated as `(endTime - startTime) / 1 hour`.
- Consider `useMemo` for card rendering to prevent re-renders on scroll.
