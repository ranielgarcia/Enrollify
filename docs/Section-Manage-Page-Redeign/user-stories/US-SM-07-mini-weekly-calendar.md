---
title: US-SM-07 Mini Weekly Calendar Hover Tooltip
id: US-SM-07
epic: Section Manage Page Redesign
priority: medium
create_by: Copilot
created_at: 2026-06-10T00:00:00Z
updated_at: 2026-06-10T00:00:00Z
---

## Summary

Display a mini weekly calendar tooltip when hovering over a section card, using a two-phase loading approach: static summary instantly on hover, then async full mini-weekly grid with conflict visualization loads in the background from the offering details endpoint.

## Persona(s)

- Scheduler / Administrator

## User Story

As a Scheduler, I want to hover over a section to see its weekly schedule with conflict markers, so that I can quickly visualize offering distribution and conflicts without clicking through.

## Acceptance Criteria

1. Given a section card When the user hovers over it Then a static summary tooltip appears instantly (e.g., "3 offerings, 1 conflict detected"), derived from `validationSummary`.
2. Given the static summary is shown When the offering details complete loading Then the full mini weekly grid renders with time slots, offerings, and conflict visualization.
3. Given the mini grid When rendered Then it shows Mon-Fri columns, time rows (hourly or 30-min), and colored offering blocks with subject code and teacher name.
4. Given a conflicted offering When rendered in the mini grid Then it has a red border.
5. Given "View Full Schedule" When clicked Then it navigates to the section detail page.
6. Given a conflicting offering box When clicked Then it navigates to the conflicting section's detail page.
7. Given the mouse leaves the card When the tooltip is open Then it auto-closes.

## Definition of Done

- [ ] `SectionCardMiniSchedule` component with two-phase loading
- [ ] Static summary derived from `validationSummary` (instant, no fetch)
- [ ] Full mini weekly grid async-loaded from offering details endpoint
- [ ] Conflict visualization with red borders
- [ ] Navigation actions: "View Full Schedule" and click conflicting offering
- [ ] Auto-close on mouse leave
- [ ] Cached per `sectionId` after first fetch

## Preconditions & Assumptions

- Offering details endpoint returns full schedule data with `schedules[]`, `conflicts[]`.
- Hover interaction is desktop-only (mouse events).

## Business Rules / Validation

- Phase 1 (instant): static summary text from `validationSummary` (no API call).
- Phase 2 (async): full grid loaded from `GET /api/class-sections/{sectionId}/offerings`.
- Conflicting offerings show red border + warning icon.

## API / Back-end Notes

- Same offering details endpoint as US-SM-06.
- Data cached per `sectionId` after first fetch (TanStack Query, stale time > hover duration).

## UI Notes

- Tooltip: compact weekly grid, scrollable if many offerings.
- Days: MON through FRI columns.
- Time rows: hourly intervals (7:00 AM – 6:00 PM).
- Offering blocks: colored by course, showing subject code + teacher surname.
- Loading state: spinner in tooltip while async data loads.

## Edge Cases & Error Handling

- Offering details fetch fails → show cached static summary with "Unable to load schedule" note.
- No offerings → show "No offerings scheduled" in tooltip.
- Hover on table rows → tooltip also works (mini calendar triggered on row hover).
- Rapid hover across multiple cards → debounce or cancel previous fetch.

## Test Cases

1. Hover card → static summary appears immediately.
2. After offering details load → full mini grid renders.
3. Conflicted offering in grid has red border.
4. Click "View Full Schedule" navigates to detail page.
5. Tooltip auto-closes on mouse leave.

## Dependencies

- US-SM-01 (for offering details endpoint)
- US-SM-03 (for card component integration)

## Related Requirements / Source

- PRD-02 FR-09 (Mini Weekly Calendar Hover Tooltip)

## Notes / Implementation Considerations

- Use `useSuspenseQuery` with `enabled` flag for lazy fetch.
- Tooltip positioning: above/below card based on viewport space.
- Consider debounce (150ms) on hover to avoid excessive fetches during rapid mouse movement.
- Lazy-load mini calendar component to reduce bundle size.
