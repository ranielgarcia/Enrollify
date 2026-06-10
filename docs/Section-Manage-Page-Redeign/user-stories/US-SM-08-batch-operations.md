---
title: US-SM-08 Batch Operations
id: US-SM-08
epic: Section Manage Page Redesign
priority: high
create_by: Copilot
created_at: 2026-06-10T00:00:00Z
updated_at: 2026-06-10T00:00:00Z
---

## Summary

Enable multi-select batch operations on Draft sections via checkboxes. Provide a floating sticky global toolbar and per-course group toolbars. Support bulk actions: Assign Adviser, Open for Enrollment, Cancel, and Bulk Edit.

## Persona(s)

- Scheduler / Administrator

## User Story

As a Scheduler, I want to select multiple Draft sections and perform batch actions (assign adviser, open for enrollment, cancel, bulk edit), so that I can efficiently manage large groups of sections without repetitive single-section operations.

## Acceptance Criteria

1. Given at least one Draft section checkbox is selected When checked Then a floating sticky toolbar appears at the bottom showing selection count and action buttons.
2. Given the floating toolbar When visible Then it shows: selection count, `[Assign Adviser]`, `[Open for Enrollment]`, `[Cancel]`, `[Bulk Edit ▾]`.
3. Given non-Draft section checkboxes When displayed Then they are disabled with tooltip: "Batch operations only available for Draft sections".
4. Given a user selects both Draft and non-Draft sections When the toolbar renders Then batch buttons are disabled with tooltip: "Batch operations only available for Draft sections".
5. Given a course group toolbar When the user selects sections within a course Then the toolbar actions are scoped to that course's selections only.
6. Given the global toolbar When the user has selections across courses Then actions affect all selected sections.
7. Given batch "Open for Enrollment" When triggered Then a confirmation shows count; on confirm, bulk endpoint validates each section, succeeds for eligible, reports failures per section.
8. Given batch "Cancel" When triggered Then a confirmation dialog details "X offerings will be freed" (teacher/room assignments); on confirm, performs soft-delete cascade.
9. Given batch "Assign Adviser" When clicked Then a drawer opens with adviser search and multi-select, applies to all selected Draft sections.

## Definition of Done

- [ ] Checkbox state management (local `Set<number>`, course-scoped + global)
- [ ] Floating sticky `BatchActionsToolbar` component
- [ ] Course-level group toolbar with scoped batch actions
- [ ] `BulkStatusTransitionDialog` with count summary + confirmation
- [ ] `BulkAdviserAssignDrawer` with search + multi-select
- [ ] Disabled batch buttons for non-Draft selections with tooltip
- [ ] Mixed-status selection warning
- [ ] Bulk Cancel confirmation with "X offerings will be freed" message

## Preconditions & Assumptions

- Batch operations only available for Draft sections.
- Course-level toolbar only appears when course group is expanded.
- Global toolbar appears when ≥1 Draft section is selected across courses.

## Business Rules / Validation

- Only Draft sections can be batch-operated.
- If non-Draft sections selected, batch buttons disabled with tooltip.
- Mixed-status selections show a warning before batch action.
- "Open All" only affects Draft sections within that course.
- Bulk Cancel performs soft-delete cascade (frees teacher/room assignments).
- Bulk Open validates eligibility per section individually.

## API / Back-end Notes

- Bulk endpoints from US-SM-01:
  - `POST /api/class-sections/bulk/assign-adviser`
  - `POST /api/class-sections/bulk/open`
  - `POST /api/class-sections/bulk/cancel`
- All accept `{ sectionIds: number[] }` and return `{ succeeded, failed, errors[] }`.
- Bulk assign-adviser additionally accepts `{ adviserId: number }`.

## UI Notes

- Floating toolbar: sticky at bottom of viewport, full-width.
- Course-level toolbar: inside each CourseGroup header area, below progress bar.
- Confirmation dialogs show count + per-section failure summary if partial.
- Bulk Cancel dialog: "X offerings will be freed, releasing Y teacher assignments and Z room assignments."
- Bulk Edit dropdown: Capacity, Term, Year Level options (opens corresponding dialog/drawer).

## Edge Cases & Error Handling

- All selected sections fail validation → show "No sections were eligible" with per-section error details.
- 0 sections selected → toolbar hidden.
- User navigates away while batch is processing → show "Unsaved changes" warning if dirty.
- Bulk endpoint partially succeeds → show "X succeeded, Y failed" with expandable error list.

## Test Cases

1. Select single Draft section → toolbar appears with correct count.
2. Select Draft + non-Draft → batch buttons disabled.
3. Course-level "Open All" only affects Draft sections in that course.
4. Bulk Cancel shows "X offerings will be freed" and soft-deletes.
5. Bulk Adviser Assign drawer opens and applies adviser.

## Dependencies

- US-SM-01 (for bulk endpoints)
- US-SM-03 (for card checkboxes)
- US-SM-04 (for table checkboxes)

## Related Requirements / Source

- PRD-02 FR-10 (Batch Operations)
- PRD-03 Technical Design §4.4 (Bulk Operation Endpoints)

## Notes / Implementation Considerations

- Selection state managed as local `Set<number>` in the page component.
- Course-level toolbar derived from filtering selection set by course ID.
- Toolbar transitions: slide up animation on appear.
- Consider optimistic updates for batch operations.
