---
title: US-SM-04 Enhanced Table View
id: US-SM-04
epic: Section Manage Page Redesign
priority: high
create_by: Copilot
created_at: 2026-06-10T00:00:00Z
updated_at: 2026-06-10T00:00:00Z
---

## Summary

Build the enhanced table view as an alternative to the card view, adding scheduling-specific columns (progress bar, errors/conflicts compound badge, inline action buttons) to the existing DataTable pattern while maintaining filtering/sorting via `nuqs`.

## Persona(s)

- Scheduler / Administrator

## User Story

As a Scheduler who prefers dense tabular data, I want an enhanced table view with scheduling progress, error/conflict badges, and inline action buttons, so that I can quickly scan and act on sections without switching to card view.

## Acceptance Criteria

1. Given the page is in table view When sections are loaded Then columns display: checkbox, Code, Section, Course, Status, Progress (Sched%), Errors/Conflicts, Adviser, Actions.
2. Given a row with errors/conflicts When rendered Then it has a red left border indicator.
3. Given a non-Draft row When rendered Then it has a gray background to indicate read-only state.
4. Given an action button on a Draft row When clicked Then the inline status transition fires.
5. Given a non-Draft row When rendered Then action buttons for status transitions are disabled.
6. Given the compound badge When displayed Then it shows `⚠️X/❌Y` format (amber for errors-only, red when conflicts present).
7. Given the compound badge When clicked Then it opens the conflict preview drawer.
8. Given the existing sorting/filtering When the user interacts Then it works via `nuqs` + `DataTableAdvancedToolbar`.

## Definition of Done

- [ ] `SectionsTable` component with enhanced columns
- [ ] Progress bar column with percentage + visual bar
- [ ] Errors/Conflicts compound badge column
- [ ] Inline action buttons column (Draft: Open/Cancel/Details, Open: Cancel/Details, Others: Details)
- [ ] Red left border on rows with errors/conflicts
- [ ] Gray background on non-Draft rows
- [ ] Existing filtering/sorting maintained

## Preconditions & Assumptions

- View toggle between card and table is already implemented (US-SM-03).

## Business Rules / Validation

- Same business rules as card view for action buttons and checkbox behavior.
- Red left border if `offeringsWithErrors > 0` OR `offeringsWithConflicts > 0`.
- Non-Draft rows rendered with gray background and disabled interactions.

## API / Back-end Notes

- Same college-filtered list endpoint as card view: `GET /api/colleges/{collegeId}/class-sections?academicYearId={id}`.
- Same `validationSummary` used for progress and error/conflict columns.

## UI Notes

- Table columns: `☐ | Code | Section | Course | Status | Sched% ████ | Errors/CF ⚠️X/❌Y | Adviser | Actions`
- Progress column: visual bar + percentage (e.g., "██░░ 50% (2/4)").
- Errors/Conflicts column: compound badge, clickable → opens conflict preview drawer.
- Actions column: inline buttons matching card view.
- Maintain `DataTableAdvancedToolbar` pattern for filters/sort.

## Edge Cases & Error Handling

- Empty cells for sections with no adviser → show "(unassigned)".
- Section with 0 offerings → progress shows "0% (0/0)" or "N/A".

## Test Cases

1. Table renders all columns with correct data.
2. Draft row shows enabled [Open] [Cancel] [Details] buttons.
3. Open row shows [Cancel] [Details] buttons only.
4. Non-Draft row has gray background.
5. Row with errors has red left border.
6. Compound badge click opens conflict preview drawer.

## Dependencies

- US-SM-01 (for data types and API)
- US-SM-03 (for view toggle mechanism)
- US-SM-06 (for compound badge → conflict preview drawer)

## Related Requirements / Source

- PRD-02 FR-01 (View Toggle), FR-06 (Enhanced Table View)
- PRD-03 Technical Design §3 (Component Tree)

## Notes / Implementation Considerations

- Reuse `DataTable` pattern from existing management pages.
- Use `useDataTable` hook for table state.
- Progress bar uses Tailwind width classes (`w-[${percentage}%]`).
