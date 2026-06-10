---
title: US-SM-05 Inline Status Transitions
id: US-SM-05
epic: Section Manage Page Redesign
priority: high
create_by: Copilot
created_at: 2026-06-10T00:00:00Z
updated_at: 2026-06-10T00:00:00Z
---

## Summary

Enable inline status transitions directly on section cards and table rows without navigating to the detail page. Support Draft → Open (with eligibility validation), Draft/Open → Cancelled (with confirmation dialog), with optimistic UI updates and toast notifications.

## Persona(s)

- Scheduler / Administrator

## User Story

As a Scheduler, I want to open or cancel sections directly from the list view without navigating to the detail page, so that I can perform status transitions efficiently at scale.

## Acceptance Criteria

1. Given a Draft section When the user clicks [Open for Enrollment] Then the system validates eligibility; if valid, transitions to Open, updates the badge/buttons inline, and shows a success toast.
2. Given a Draft section with unresolved errors OR conflicts When the user clicks [Open for Enrollment] Then the system rejects with an inline error message explaining why.
3. Given a Draft or Open section When the user clicks [Cancel] Then a confirmation dialog appears with impact summary ("X offerings will be freed — teacher/room assignments released").
4. Given the confirmation dialog When the user confirms Then the section transitions to Cancelled, card/row updates inline, stats bar refreshes.
5. Given a successful transition When the mutation completes Then a toast notification is shown; on failure, an inline error is displayed.
6. Given the transition completes When the card/row updates Then the status badge changes, action buttons update to reflect new status, and the section stays in place (no reordering).

## Definition of Done

- [ ] `InlineTransitionButtons` component supporting Draft(Open/Cancel), Open(Cancel), Others(Details)
- [ ] Draft → Open mutation with eligibility validation
- [ ] Draft/Open → Cancelled mutation with confirmation dialog
- [ ] Optimistic UI update on card/row
- [ ] Toast notification on success, inline error on failure
- [ ] Stats bar refreshes after transition

## Preconditions & Assumptions

- Existing mutations `openClassSectionOptions` and `cancelClassSectionOptions` exist in API collection (from US-SM-01).
- Lifecycle doc permits Open → Cancelled transition.

## Business Rules / Validation

- Both unresolved errors AND conflicts block "Open for Enrollment" for Draft sections.
- Only Draft → Open and Draft/Open → Cancelled transitions are available on this page.
- Locked → Active → Completed transitions are automated (not on this page).
- Cancellation performs soft-delete cascade, freeing teacher/room assignments.

## API / Back-end Notes

- Reuse existing single-section mutations from `class-section-collection-v2.ts`.
- `PATCH /api/class-sections/{id}/open` — validates eligibility; returns 400 with message if validation fails.
- `PATCH /api/class-sections/{id}/cancel` — performs soft-delete cascade.
- Stats bar refetched via `invalidateQueries` after success.

## UI Notes

- `InlineTransitionButtons`:
  - Draft: `[Open for Enrollment]` `[Cancel]` `[View Details →]`
  - Open: `[Cancel]` `[View Details →]`
  - Locked/Active/Completed/Cancelled: `[View Details →]`
- Cancel confirmation dialog shows impact summary and count of offerings to be freed.
- Inline error shown below buttons on failure.

## Edge Cases & Error Handling

- Draft → Open rejected due to validation → show specific error (e.g., "Section 3A has 2 unresolved errors preventing opening").
- Cancellation fails → rollback optimistic update, show error toast.
- Network error → rollback optimistic update, show error toast.

## Test Cases

1. Draft section transitions to Open successfully with valid eligibility.
2. Draft section with conflicts rejected from opening with inline error.
3. Draft section cancelled successfully with confirmation.
4. Open section cancelled successfully.
5. Checkbox and action buttons update after transition.

## Dependencies

- US-SM-01 (for API mutations and stats refetch infrastructure)

## Related Requirements / Source

- PRD-02 FR-07 (Inline Status Transitions), FR-12 (Lifecycle Scope)
- PRD-03 Technical Design §6 (Frontend State Management)

## Notes / Implementation Considerations

- Use `useMutation` with `onMutate` for optimistic update and `onSettled` for refetch.
- Confirmation dialog reuses `CancelSectionAlertDialog` pattern.
- Inline errors displayed below buttons using red text + icon.
