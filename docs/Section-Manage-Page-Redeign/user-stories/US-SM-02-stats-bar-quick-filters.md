---
title: US-SM-02 Stats Bar & Quick Filters
id: US-SM-02
epic: Section Manage Page Redesign
priority: high
create_by: Copilot
created_at: 2026-06-10T00:00:00Z
updated_at: 2026-06-10T00:00:00Z
---

## Summary

Build the stats bar showing full-dataset aggregate counts (Draft, Open, Cancelled, Unresolved Errors, Conflicts, Unscheduled) and the quick filter buttons (All, Draft, Open, Cancelled, Unresolved Errors, Conflicts, Needs Attention). Stats are clickable to apply filters. Quick filters are URL-synced via `nuqs`.

## Persona(s)

- Scheduler / Administrator

## User Story

As a Scheduler, I want to see aggregate counts for all sections at a glance and click them to filter the list, so that I can quickly assess scheduling health and drill into problem areas.

## Acceptance Criteria

1. Given the page loads When the stats endpoint returns data Then each stat (Draft, Open, Cancelled, Unresolved Errors, Conflicts, Unscheduled) displays its count with matching color convention.
2. Given a stat button is clicked When the user clicks it Then a `nuqs` filter param is applied and the list filters accordingly.
3. Given filters are active When the stats bar renders Then it still shows full-dataset numbers (unaffected by filters).
4. Given a quick filter button (e.g., "Conflicts") is clicked When the filter is applied Then the button is visually highlighted.
5. Given multiple quick filters are active When the user clicks another filter Then it toggles or combines (e.g., Draft + Conflicts).
6. Given the "Needs Attention" filter When clicked Then it applies a combined filter: Draft status OR has unresolved errors OR has conflicts.

## Definition of Done

- [ ] Stats bar component with clickable stat buttons
- [ ] Quick filters component with preset buttons
- [ ] Filters URL-synced via `nuqs` (`useQueryStates`)
- [ ] Active filter visually highlighted
- [ ] Stats always show full-dataset numbers from separate endpoint
- [ ] Locked/Active/Completed excluded from stats bar

## Business Rules / Validation

- Stats always reflect full-dataset numbers regardless of current page or active filters.
- "Needs Attention" = Draft status OR has unresolved errors OR has conflicts.
- Locked, Active, and Completed statuses excluded from stats bar.

## API / Back-end Notes

- Stats come from `GET /api/class-sections/stats?academicYearId={id}`.
- Refreshed via TanStack Query with background refetch after mutations.
- Quick filters apply as URL search params via `nuqs`.

## UI Notes

- Stat buttons use colors matching `SectionStatusBadge` conventions.
- Stat format: label + count (e.g., "Draft 24").
- Quick filters row: [All] [Draft] [Open] [Cancelled] [Unresolved Errors] [Conflicts] [Needs Attention].
- Active filter highlighted with primary color.

## Edge Cases & Error Handling

- Stats endpoint fails → show dash ("—") for each stat, retry button.
- "Needs Attention" is a compound filter — clearing it removes both status and error/conflict filters.

## Test Cases

1. Stats bar loads full-dataset counts independent of filters.
2. Clicking "Draft" stat applies Draft filter.
3. Clicking "Needs Attention" applies compound filter.
4. Stats bar refreshes after a status transition mutation.

## Dependencies

- US-SM-01 (for stats endpoint and API collection)

## Related Requirements / Source

- PRD-02 Functional Requirements FR-02, FR-03
- PRD-03 Technical Design §6 (Frontend State Management)

## Notes / Implementation Considerations

- Stats bar always visible, even when list is empty.
- Use `useSuspenseQuery` for stats with separate query key from section list.
- Refetch stats after any mutation via `invalidateQueries`.
