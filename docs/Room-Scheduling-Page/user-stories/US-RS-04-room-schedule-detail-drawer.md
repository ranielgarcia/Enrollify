---
title: US-RS-04 Room Schedule Detail Drawer & Interactions
id: US-RS-04
epic: Section Manage Page Redesign
priority: high
create_by: Copilot
created_at: 2026-06-10T00:00:00Z
updated_at: 2026-06-10T00:00:00Z
---

## Summary

Implement click and hover interactions on offering cards in the room schedule grid. Click opens a detail drawer with offering info, conflict details, and quick actions. Hover shows a tooltip with section summary.

## Persona(s)

- Scheduler / Administrator

## User Story

As a Scheduler, I want to click an offering card to see its full details and conflicts, and hover for a quick summary, so that I can investigate and resolve scheduling conflicts efficiently.

## Acceptance Criteria

1. Given an offering card When clicked Then a right-slide drawer opens showing section code, subject, teacher, room, schedule, and conflict details.
2. Given a conflicted offering in the drawer When displayed Then it shows all conflicts with overlapping section code, teacher, time range, and room.
3. Given conflict details in the drawer When rendered Then quick action buttons are shown: `[Change Time]`, `[Change Room]`, `[View Section Detail]`, `[View Room Management]`.
4. Given a non-conflicted offering in the drawer When rendered Then no conflict section is shown; quick actions show only `[View Section Detail]`.
5. Given an offering card When hovered Then a tooltip appears with section code, teacher, room, and time range (plus conflict warning if applicable).
6. Given the tooltip for a conflicted offering When shown Then it adds "⚠️ OVERLAPS WITH {section}" text.
7. Given any navigation action When clicked (e.g., "View Section Detail") Then the user navigates to the appropriate page.

## Definition of Done

- [ ] `OfferingDetailDrawer` component with offering info + conflicts + quick actions
- [ ] Hover tooltip on offering cards
- [ ] Navigation to section detail page
- [ ]  to room management pageNavigation
- [ ] Conflict details with overlapping section info
- [ ] Quick action buttons (Change Time, Change Room, View Section Detail)

## Preconditions & Assumptions

- Section detail page exists at existing route.
- Room management page exists at its route.

## Business Rules / Validation

- Conflict details include overlapping section, teacher, time range, room.
- "Change Time" and "Change Room" navigate to the section detail page where editing happens.
- Conflicted offerings show warning in both tooltip and drawer.

## API / Back-end Notes

- Offering detail from existing `GET /api/subject-offerings/{id}` or from `POST /api/rooms/schedule` response.
- No new endpoint needed if data is already in schedule response.

## UI Notes

- Drawer slides from right (vaul Drawer).
- Drawer sections: Header (section code + status) → Subject info → Teacher → Room → Schedule → Conflict list (if any) → Quick action buttons → Footer links.
- Tooltip: compact, auto-closes on mouse leave.
- Tooltip content: `BSCS 3A (Dr. Smith) | Room 301 | 07:30-08:30 | 45 students`. If conflicted: add `⚠️ OVERLAPS WITH BSIT 2A`.

## Edge Cases & Error Handling

- Offering data not found → drawer shows "Offering not found" with close button.
- Navigation to section detail fails → error toast.
- Tooltip delay: 300ms before showing to avoid flicker on mouse pass-through.

## Test Cases

1. Click conflicted offering card → drawer opens with conflict details + quick actions.
2. Click non-conflicted offering card → drawer opens without conflict section.
3. Hover offering card → tooltip appears with correct info.
4. Hover conflicted offering card → tooltip includes conflict warning.
5. Click "View Section Detail" → navigates to correct route.

## Dependencies

- US-RS-02 (for offering cards to be interactive)

## Related Requirements / Source

- `room-scheduler-page-design.md` §6.1 (Click an Offering Card)
- `room-scheduler-page-design.md` §6.2 (Hover Over Offering Card)
- `room-scheduler-page-design.md` §5.3 (Conflict Resolution Workflow)
- `room-scheduler-page-design.md` §11 (Phase 4)

## Notes / Implementation Considerations

- Drawer content lazy-loaded if not already in schedule response.
- Use `vaul` Drawer for consistency with other drawers in the app.
- Tooltip implemented with CSS hover + absolutely positioned element, or a lightweight library.
