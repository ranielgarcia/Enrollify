---
title: US-SM-03 Card View with College/Course Grouping
id: US-SM-03
epic: Section Manage Page Redesign
priority: high
create_by: Copilot
created_at: 2026-06-10T00:00:00Z
updated_at: 2026-06-10T00:00:00Z
---

## Summary

Build the card view as the default display mode, with sections grouped by College (accordion header) then Course (sub-group). Each section renders as a rich card showing status, scheduling progress, adviser, issues, rooms, schedule summary, and action buttons.

## Persona(s)

- Scheduler / Administrator

## User Story

As a Scheduler, I want to see sections organized hierarchically by college and course with rich card visuals, so that I can quickly scan scheduling health per program and take action without navigating to detail pages.

## Acceptance Criteria

1. Given the page loads in card view When sections are returned Then they are grouped by College (accordion header) then by Course (sub-group).
2. Given a college accordion header When displayed Then it shows college name, total section count, and aggregate scheduling progress bar.
3. Given a course group When displayed Then it shows course code + name, section count, and scheduling progress bar.
4. Given a course group is collapsed by default When the user clicks it Then it expands to reveal section cards.
5. Given a Draft section card When rendered Then it shows: checkbox (enabled), section code, status badge, error/conflict compound badge, subtitle, adviser row, scheduling progress bar, issues row, room summary, schedule summary, and action buttons [Open] [Cancel] [Details].
6. Given an Open section card When rendered Then it shows checkbox (disabled), [Cancel] [Details] buttons.
7. Given a Locked/Active/Completed/Cancelled section card When rendered Then it shows checkbox (disabled) and only [Details] button.
8. Given a section has unresolved errors OR conflicts When the card renders Then it has a red left border.
9. Given a Draft section's action button When clicked Then the status transition fires inline (no page navigation).
10. Given the view toggle When the user toggles from Card to Table Then the preference is persisted in `sessionStorage`.

## Definition of Done

- [ ] `SectionCard` component with all zones (header, subtitle, adviser, progress, issues, rooms, schedule, actions)
- [ ] `SectionsCardView` with CollegeAccordion → CourseGroup → SectionCard nesting
- [ ] `SectionStatusBadge` component
- [ ] View toggle persisted in `sessionStorage`
- [ ] Red left border conditionally applied
- [ ] Card view is the default
- [ ] Checkbox disabled for non-Draft sections
- [ ] Action buttons disabled for non-Draft/non-Open sections

## Preconditions & Assumptions

- Card view is default; toggle persisted in `sessionStorage` only (not cross-tab).
- Offering details are NOT embedded in cards — only `validationSummary` is used.

## Business Rules / Validation

- Checkboxes disabled for non-Draft sections (tooltip: "Batch operations only available for Draft sections").
- Red left border if `offeringsWithErrors > 0` OR `offeringsWithConflicts > 0`.
- Draft cards show [Open] [Cancel] [Details]; Open cards show [Cancel] [Details]; others show only [Details].

## API / Back-end Notes

- Section list from `GET /api/class-sections/filter/{page}/{pageSize}` with `validationSummary`.
- View toggle stored in `sessionStorage` key (e.g., `sections-view-mode`).

## UI Notes

- Card zones: Header (checkbox + code + status badge + error/conflict badge) → Subtitle → Adviser row → Progress bar → Issues row → Room summary → Schedule summary → Action buttons.
- Red left border via CSS class; disabled checkbox grayed out.
- Academic Year selector visible in all views.

## Edge Cases & Error Handling

- Section with no offerings → progress bar at 0%, "0 offerings scheduled".
- Section with missing adviser → show "(unassigned)" with [Assign] button (Draft only).
- Section missing all offerings → "No offerings created" in rooms/schedule rows.

## Test Cases

1. Card renders all zones for Draft section with data.
2. Non-Draft section card has disabled checkbox.
3. Section with errors/conflicts shows red left border.
4. Toggle to Table view persists on refresh (same tab).
5. College accordion collapses/expands correctly.

## Dependencies

- US-SM-01 (for paginated endpoint with `validationSummary`)

## Related Requirements / Source

- PRD-02 FR-01 (View Toggle), FR-04 (College/Course Grouping), FR-05 (Section Card)
- PRD-03 Technical Design §3 (Component Tree)

## Notes / Implementation Considerations

- Virtualize only College accordion headers; use `content-visibility: auto` on inner course blocks.
- Progress bar derived from `validationSummary`: `offerings without errors / totalOfferings * 100` (backend-computed ratio preferred).
- Mini calendar tooltip handled in US-SM-07.
