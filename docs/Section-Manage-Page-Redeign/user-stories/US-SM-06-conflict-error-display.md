---
title: US-SM-06 Conflict & Error Display with Preview Drawer
id: US-SM-06
epic: Section Manage Page Redesign
priority: high
create_by: Copilot
created_at: 2026-06-10T00:00:00Z
updated_at: 2026-06-10T00:00:00Z
---

## Summary

Display a compound error/conflict badge on cards and table rows (`⚠️X/❌Y`), color-coded (amber for errors-only, red when conflicts present), and make it clickable to open a conflict preview drawer with detailed information about each validation message and conflict.

## Persona(s)

- Scheduler / Administrator

## User Story

As a Scheduler, I want to see a compact error/conflict summary on every card and table row and click it to see details, so that I can quickly identify problematic sections and understand the root cause without navigating away.

## Acceptance Criteria

1. Given a section with unresolved errors but no conflicts When the badge renders Then it shows `⚠️X/❌0` in amber color.
2. Given a section with conflicts (with or without errors) When the badge renders Then it shows `⚠️X/❌Y` in red color.
3. Given a section with no errors and no conflicts When the badge renders Then it shows `✓0/✓0` in green (or hidden — clean state).
4. Given the compound badge When clicked Then a slide-out drawer opens showing each validation message and conflict with details.
5. Given the conflict preview drawer When opened Then it lists eligibility validation messages (code, severity, message) and conflicts (type, severity, affected offerings, time range).
6. Given a conflict with an overlapping section When displayed Then the user can click to navigate to that section's detail page.
7. Given the conflict preview drawer When opened for a Draft section Then offering details are lazy-loaded from `GET /api/class-sections/{sectionId}/offerings`.

## Definition of Done

- [ ] Compound badge component (`⚠️X/❌Y` format) with conditional coloring
- [ ] Click handler opens `ConflictPreviewDrawer`
- [ ] `ConflictPreviewDrawer` component with grouped offering details
- [ ] Lazy-loading of offering details from dedicated endpoint
- [ ] Navigation from conflict to conflicting section's detail page
- [ ] Amber color for errors-only, red for conflicts present (or both)

## Preconditions & Assumptions

- Offering details endpoint returns eligibility validation messages (section-level + offering-level) and `conflicts[]` per offering.
- Conflicts only computed for Draft sections; Open sections have empty `conflicts[]`.

## Business Rules / Validation

- Enrollment-eligibility validation codes use domain codes (e.g., `CLASS_SECTION_ADVISER_REQUIRED`, `SUBJECT_OFFERING_TEACHER_REQUIRED`, `SUBJECT_OFFERING_ROOM_REQUIRED`, `SUBJECT_OFFERING_INSUFFICIENT_CLASS_SCHEDULES`, `SUBJECT_OFFERING_INVALID_CLASS_SCHEDULES`).
- Both unresolved errors AND conflicts prevent "Open for Enrollment".
- Badge is clickable only when errors or conflicts exist.

## API / Back-end Notes

- Badge data from `validationSummary` in the college-filtered list response.
- Drawer detail data from `GET /api/class-sections/{sectionId}/offerings` (lazy-loaded).
- Conflict types: `TEACHER_DOUBLE_BOOKED`, `ROOM_DOUBLE_BOOKED`, `SECTION_OVERLAP`, `DUPLICATE_SUBJECT_IN_SECTION`.

## UI Notes

- Badge format: `⚠️X/❌Y` where X = unresolved errors count, Y = conflicts count.
- Drawer shows sections grouped by offering, with validation messages and conflicts listed under each.
- Each conflict row shows: type, severity, offering code, teacher, time range, room.
- Clickable links to conflicting section detail pages.

## Edge Cases & Error Handling

- Offering details fetch fails → show "Unable to load details" with retry button in drawer.
- Section has 0 offerings → badge shows "—" or hidden.
- Offering has no conflicts but has validation messages → show only validation section.

## Test Cases

1. Badge shows `⚠️1/❌0` in amber for errors-only section.
2. Badge shows `⚠️0/❌1` in red for conflicts-only section.
3. Badge hidden or shows `✓0/✓0` for clean section.
4. Click badge opens drawer with lazy-loaded offering details.
5. Drawer shows navigation links to conflicting sections.

## Dependencies

- US-SM-01 (for college-filtered list + offering details endpoints)
- US-SM-03 (for card badge integration)
- US-SM-04 (for table badge integration)

## Related Requirements / Source

- PRD-02 FR-08 (Conflict & Error Display)

## Notes / Implementation Considerations

- Use `vaul` Drawer component (consistent with other drawers).
- Badge should be compact — single line, no wrapping.
- Lazy fetch offering details once per sectionId, cache via TanStack Query.
