---
title: US-SM-09 Empty State, Performance & Polish
id: US-SM-09
epic: Section Manage Page Redesign
priority: high
create_by: Copilot
created_at: 2026-06-10T00:00:00Z
updated_at: 2026-06-10T00:00:00Z
---

## Summary

Build empty state components (filtered + no-data variants), add performance optimizations (virtual scrolling, `content-visibility`, debounce), implement desktop-only viewport check, add ARIA labels, and ensure keyboard navigation works across cards and table rows.

## Persona(s)

- Scheduler / Administrator

## User Story

As a Scheduler, I want clear empty states when filters return no results and a performant, accessible page, so that I can recover from empty results quickly and navigate the page efficiently.

## Acceptance Criteria

1. Given filters return zero sections When the list renders Then an empty state shows: "No sections match your current filters" with suggestions and a `[Clear All Filters]` button.
2. Given no sections exist at all for the academic year When the list renders Then an empty state shows: "No sections have been created for this academic year" with `[Bulk Initialize →]`.
3. Given the list is empty When rendered Then the stats bar and academic year selector remain visible.
4. Given many college accordions When the page loads Then only visible college headers are rendered (virtualized); inner course blocks use `content-visibility: auto`.
5. Given filter changes When the user types Then the filter update is debounced by 300ms.
6. Given a viewport smaller than 1024px When the page loads Then a banner appears: "This page is optimized for desktop. Use [Room Scheduler] or [Section Detail] on smaller screens."
7. Given a keyboard user When tabbing through cards Then focus is visible, Enter activates links/buttons, Space toggles checkboxes.
8. Given a screen reader When reading the page Then progress bars have ARIA labels, batch toolbar announcements are made.

## Definition of Done

- [ ] `EmptyState` component with filtered + no-data variants
- [ ] Empty state wired into card and table views
- [ ] 300ms debounce on filter changes
- [ ] `content-visibility: auto` on CourseGroup blocks
- [ ] Virtualized College accordion headers (TanStack Virtual)
- [ ] Desktop-only viewport check with banner
- [ ] ARIA labels on progress bars, batch toolbar
- [ ] Keyboard navigation (Tab, Enter, Space)

## Preconditions & Assumptions

- TanStack Virtual is available in the project.
- `content-visibility` CSS property supported in target browsers.

## Business Rules / Validation

- Stats bar always visible even when list is empty.
- [Clear All Filters] resets all `nuqs` search params.
- No-data variant (`[Bulk Initialize →]`) only shown when no sections exist at all.

## API / Back-end Notes

- No new endpoints needed — empty state derived from existing data.
- Bulk Initialize button links to existing bulk initialization flow/page.

## UI Notes

- Filtered empty state: "No sections match your current filters. Try: Widen your status filter, Clear Conflicts filter, Select a different year."
- No-data empty state: "No sections have been created for this academic year. [Bulk Initialize →]"
- Desktop banner: "This page is optimized for desktop. Use [Room Scheduler ↗] or [Section Detail ↗] on smaller screens."
- Debounce indicator: no visible spinner; list updates silently after debounce.

## Edge Cases & Error Handling

- Network error during section fetch → show error state with retry.
- All sections filtered out vs. no sections exist → different empty states.
- Viewport resize from desktop to mobile → banner shows/hides dynamically.

## Test Cases

1. Filters return 0 results → filtered empty state with [Clear All Filters].
2. No sections in academic year → no-data empty state with [Bulk Initialize].
3. Empty state shown → stats bar still visible.
4. Resize below 1024px → desktop banner appears.
5. Tab through cards → focus ring visible on each interactive element.

## Dependencies

- US-SM-03 (card view)
- US-SM-04 (table view)
- US-SM-02 (stats bar remains visible when empty)

## Related Requirements / Source

- PRD-02 FR-11 (Empty State)
- PRD-03 Technical Design §8 (Desktop-Only Constraint)
- PRD-03 Technical Design §9 (Performance Strategy)

## Notes / Implementation Considerations

- Use `useMemo` for filtering suggestions based on active filter state.
- Virtual scrolling: only College accordion headers are virtualized as top-level rows.
- Check viewport width with `useWindowSize` hook or CSS media query + JS listener.
- Debounce implemented on `searchParams` update (300ms).
