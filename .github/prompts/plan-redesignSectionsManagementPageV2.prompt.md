# Plan: Senior Redesign of Sections Management Page v2

## TL;DR

Rebuild the sections-management-page-v2 surface as a focused, modern scheduling cockpit: a sticky context bar (college + term + create), a single consolidated KPI/filter strip that replaces the duplicate stats-bar + quick-filters UIs, a polished section card with clearer hierarchy and a richer schedule peek (HoverCard), a true cross-course floating selection toolbar, and a tighter table view. Underneath, extract a shared selection hook, a shared filter utility, a dialog state reducer, and add toast feedback to every mutation. Preserve all business logic, API integration, routing, and permissions.

---

## Scope decisions (already made — no follow-up needed)

- **Page route stays:** `src/routes/portal/curriculum-and-scheduling/scheduling/sections.tsx` keeps pointing at `@/page-components/sections-management-page-v2`. We rebuild the page-components folder in place.
- **No API/backend changes.** Use existing `class-section-collection`, `college-collection`, mutations (`bulkOpenSectionsMutationOptions`, `bulkCancelSectionsMutationOptions`, `bulkAssignAdviserMutationOptions`, `openClassSectionOptions`, `cancelClassSectionOptions`, `updateClassSectionOptions`), and `getOfferingDetailsForClassSectionOptions`.
- **Single-college, single-term workflow preserved.** No multi-college view.
- **Card view remains default**, table view kept as the dense alternative.
- **Quick-filters component is removed** and its responsibility folded into the KPI strip (stats pills + a single prominent "Needs attention" toggle). Eliminates the duplicate-filter-UI problem.
- **Selection is cross-course / cross-view.** A single `useSectionSelection` hook backs both card and table; switching views or filters preserves selection.
- **Floating selection toolbar** at the bottom of the viewport replaces the per-course inline batch bar.
- **Dialog/drawer state** consolidated via a `useSectionsDialogs` reducer hook.
- **Toast feedback** (sonner) on every mutation success/failure.
- **Stub `delete-section-alert-dialog.tsx` is deleted** (cancel covers the lifecycle here; delete is out of scope and the file is non-functional).
- **`section-form-drawer.tsx`** keeps the adviser-edit role only; dead "Section" empty branch is removed.

---

## Phase 1 — Foundation & shared primitives _(no UI yet)_

Lays the structural groundwork that the rest of the phases consume. Each item is independently authorable; all are _parallel with each other_.

1. **Create `lib/section-filters.ts`** with two pure helpers:
   - `filterSectionsByQuickFilter(sections, filter)` — single source of truth for `draft | open | cancelled | errors | conflicts | needs-attention | all`. Removes the duplicate `applyQuickFilter` blocks in `sections-card-view.tsx` and `sections-table.tsx`.
   - `getSchedulingProgress(section)` returning `{ scheduled, total, pct, tone: 'success'|'warn'|'danger' }`. Replaces inline calculations in `section-card.tsx`, `sections-card-view.tsx` (`getCourseProgress`), and `sections-table.tsx` (`ProgressCell`).
   - `getValidationSummary(section)` returning `{ errors, conflicts, hasIssues, issuesList }`. Used by every consumer of `validationSummary`.
2. **Create `hooks/use-section-selection.ts`** — a single selection store keyed by `sectionId` (flat `Set<number>`, no per-course Map). Exposes `{ selectedIds, isSelected, toggle, toggleMany, selectAllInCourse, clearCourseSelection, clearAll, getSelectedSections(allSections) }`. Selection survives view toggles, filter changes, and course collapse/expand because it lives in the page root.
3. **Create `hooks/use-sections-dialogs.ts`** — reducer with a discriminated-union state: `{ type: 'closed' } | { type: 'edit-adviser', section } | { type: 'cancel-single', section } | { type: 'bulk-open', sectionIds } | { type: 'bulk-cancel', sectionIds } | { type: 'bulk-assign-adviser', sectionIds } | { type: 'view-conflicts', sectionId, sectionName }`. Replaces the 5 separate `useState` calls in the current `index.tsx`.
4. **Create `lib/sections-toast.ts`** — thin sonner wrappers (`notifySuccess(action, count)`, `notifyError(error)`) so every mutation success/failure has consistent messaging.

**Verification**

- `npm run test:ts` from `src/system/enrollify-frontend` passes.
- No existing imports break — phase 1 only adds files; no deletions yet.

---

## Phase 2 — Page shell & sticky context bar

Replaces the loose top-of-page header layout with a proper context bar that anchors the workflow.

1. **Create `components/sections-context-bar.tsx`** — a sticky (`sticky top-0 z-20 bg-background/95 backdrop-blur border-b`) bar that contains:
   - Left cluster: refined `CollegeSelector` (treated as the page's primary filter, larger trigger with college icon) + a non-interactive Badge showing the academic term, with a tooltip showing AY/term names. If no term is selected, show a single inline destructive Alert _inside_ the context bar (not as a separate full-width banner).
   - Right cluster: view toggle (icon-only `ToggleGroup`: `LayoutGrid` vs `TableIcon`, smaller and grouped), and the existing "Bulk Initialize" button (kept disabled with the same placeholder behavior).
2. **Redesign `college-selector.tsx`**:
   - Use a `Button` with `Building2` icon prefix and the college name; chevron suffix; subtle outline.
   - Remove the leading "College:" label text — the icon + placeholder ("Select college...") carries the meaning.
   - Keep the existing `SearchCollegesDialog` integration and clear-X interaction; preserve `useQueryState("collegeId")`.
3. **Refactor `index.tsx`** to a slim orchestrator (~120 lines target):
   - Use `useSectionsDialogs` reducer instead of 5 `useState`s.
   - Use `useSectionSelection` hook for batch state.
   - Wrap stats and content in _separate_ Suspense boundaries so a slow `stats` query doesn't block the section list.
   - Render: `<ManagementPageLayout>` → `<SectionsContextBar />` → `<StatsAndFilters />` (Suspense) → `<SectionsContent />` (Suspense) → `<DialogsRoot />` (renders the right drawer/dialog from reducer state).

**Verification**

- Page loads with no console errors; college selection, term context, and view toggle all still work.
- Selection state preserved when toggling between card and table.

---

## Phase 3 — Stats + filter strip (consolidated)

Eliminates the duplicate-filter problem; produces a single, clearer KPI/filter row.

1. **Redesign `stats-bar.tsx`** as `<SectionStatsStrip />`:
   - Group into two horizontal clusters separated by a subtle vertical Separator:
     - **Pipeline** (Draft, Open, Cancelled) — neutral/muted styling, function as status filters.
     - **Health** (Unresolved Errors, Conflicts, Unscheduled) — warning/danger tints, function as health filters.
   - Each stat becomes a `<StatPill>` with: small uppercase eyebrow label, large tabular-nums number, optional delta or "of total" subtext, leading icon in a soft-tinted square. Active state uses ring + tint, not full background flip (less aggressive).
   - Add `aria-pressed` on each clickable pill. Disabled pills (e.g., currently `Unscheduled`) display as static stat cards (no `<button>`, no hover).
   - Layout: single horizontal row at `lg+`, 2x3 grid at `md`, 2-column grid at `sm`. Uses `flex` not `grid` at `lg+` so cluster groups are clear.
2. **Delete `quick-filters.tsx`** and remove all references. The "Needs attention" filter becomes a small prominent toggle button rendered to the right of the stats strip:
   - Label "Needs attention" with `AlertOctagon` icon; subtle amber when active.
   - Single source of truth for combined errors+conflicts filtering.
3. **Add a tiny "Clear filter" affordance** that appears inline beside the active filter pill (small `X` chip) instead of relying on the "all" pseudo-pill that the old design had.
4. **Reuse `getSchedulingProgress` / `getValidationSummary`** from Phase 1 so the strip's helper logic is centralized.

**Verification**

- Clicking any stat toggles its filter; clicking the same pill again clears.
- Active filter chip shows beside the strip with a one-click clear.
- Keyboard tab order traverses pills; `aria-pressed` toggles visibly.
- `Unscheduled` pill is non-interactive but still readable.

---

## Phase 4 — Section card redesign

Stronger hierarchy, less clutter, more discoverable schedule preview.

1. **Restructure `section-card.tsx`** into 3 horizontal zones (one CardContent, no internal dividers):
   - **Zone A (top):** left = checkbox + section code (display: `text-2xl font-bold leading-none tabular-nums`) + section name (`text-sm text-muted-foreground` underneath); right = status badge stacked above issues chip.
   - **Zone B (middle):** a clean `grid grid-cols-2 gap-x-4 gap-y-2` of compact metadata rows. Each row is `<MetaRow icon label value action?>`:
     - Adviser (with inline "Change" link button if Draft, only visible on hover/focus to reduce noise).
     - Scheduling: combined `"3 / 4 offerings · 75%"` text + slim Progress with semantic tone color.
     - Rooms summary (e.g., `"101, 102, +1"` truncated; full list on tooltip).
     - Days summary (e.g., `"MWF · TTh"` derived from offerings if available; otherwise `"—"`).
   - **Zone C (bottom):** left = `<InlineTransitionButtons>` (no change in behavior); right = `<SchedulePeek>` — a `HoverCard` trigger styled as a small ghost button with `CalendarDays` icon and label "Preview" that more clearly looks clickable.
2. **Visual polish:**
   - Replace the always-present `border-l-4` (transparent or amber) with a _conditional_ `border-l-2`. Use color tokens, not hardcoded amber.
   - Remove the `bg-muted/20` desaturation on non-Draft cards; instead reduce opacity of action buttons. Draft cards stay default; non-Draft cards lose only the checkbox affordance.
   - Use `Card` defaults; drop redundant `transition-all`, custom `hover:shadow-md` stays.
   - Selected card uses `ring-1 ring-primary` (subtler than `ring-2 ring-primary/40` + offset).
3. **Replace `Popover` + manual hover timer with `HoverCard`** (shadcn provides a built-in component). Default open-delay 300 ms, close-delay 100 ms. Cleaner code, native a11y. Keep lazy fetch via `useQuery({ enabled: hoverCardOpen && !!sectionId })`.
4. **Extract `<MetaRow>` and `<IssuesChip>`** into card-internal sub-components to keep the JSX flat.

**Verification**

- Cards scan top-to-bottom: title → status → metadata → actions.
- Hovering "Preview" opens the schedule peek; keyboard `Enter`/`Space` on the trigger also opens.
- Selected, draft, open, and cancelled visual states are clearly distinguishable.

---

## Phase 5 — Mini schedule peek improvements

Make the schedule peek useful at a glance, not a tiny illegible grid.

1. **Rewrite `section-card-mini-schedule.tsx`** as `<SchedulePeek />`:
   - Increase base font from 9px to **11px** for offering blocks; raise `SLOT_HEIGHT_PX` from 20 to 24.
   - Reduce time range to **07:00–19:00** by default but expand dynamically if any offering falls outside (keeps grid compact).
   - Render hour gridlines stronger, half-hour lines lighter; remove `bg-muted/40` lunch-hour highlight (cleanly distracting and not always accurate).
   - Offering block shows two lines: `subjectCode` (semibold) and `room · teacherLastName` truncated.
   - Conflict blocks get a hatched/striped overlay using a CSS background gradient (more accessible than relying only on rose color).
2. **Top of peek:** small inline summary chips (`3 offerings · 1 conflict · 12.5 hr/wk`) computed from the offerings, providing the instant Phase-1 value the proposal mentions.
3. **Footer button** "Open Full Schedule" navigates to the section detail (via `onViewDetails`).

**Verification**

- Peek opens within 300 ms, content readable at 100% zoom.
- Conflict blocks visible without color (hatched).
- Single offering renders at minimum 24 px height; no overflow.

---

## Phase 6 — Course grouping & content area redesign

Cleaner separation of course groups; better information density.

1. **Rewrite `sections-card-view.tsx`** as `<SectionsCardView />`:
   - Replace `Collapsible` with a card-based course group (`<Card>` with a sticky-ish header inside the page scroll context). Header contains: chevron, course code+name, badge with section count, slim progress bar, and an aligned trailing "Select all draft" pill (selection is now cross-course so this just adds the course's draft sections to the global selection).
   - Use a responsive grid for section cards: `grid-cols-1 md:grid-cols-2 2xl:grid-cols-3 gap-3` (currently caps at 2). On laptops you get the standard 2-up; on big monitors, 3-up.
   - Drop the inline `BatchActionsToolbar` per group — selection is now global. Course header gets a single "n selected" chip only.
   - Drop the redundant "Select all Draft sections (N)" row below the group header.
2. **Use `getSchedulingProgress` from Phase 1** to compute the course-level aggregate (currently `getCourseProgress` is duplicated logic).
3. **Course header sticky behavior:** when course list is long, course headers stick to the top of the scroll area (`sticky top-0` within content area, just under the context bar's offset). Improves orientation when scanning many sections.

**Verification**

- Course header progress matches sum-of-children calculation.
- Selecting "All draft" at course header adds those IDs to the global selection (visible in floating toolbar).
- Scrolling through many courses keeps current course header visible.

---

## Phase 7 — Table view redesign

Bring the table closer to the DataTable conventions used elsewhere; better hierarchy with course bands.

1. **Rewrite `sections-table.tsx`** as `<SectionsTable />`:
   - Keep the simple `<Table>` (TanStack DataTable is overkill given the grouped layout requirement).
   - Course-group header row uses a subtle `bg-muted/40`, full-width course summary (code + name + section count + slim progress bar) — same data shape as card view's course header for consistency.
   - Section rows: tightened columns — `Code`, `Section` (name + adviser stacked), `Status`, `Progress` (bar + %), `Issues`, `Actions`. Remove the static legend footer (not useful; the badges are self-explanatory).
   - Use the shared `<ProgressCell>` from a new `components/scheduling/progress-cell.tsx` (extracted in Phase 1's foundation work).
   - Action column shows three icon-only buttons (`Open`, `Cancel`, `Details`) styled consistently with card actions via the existing `<InlineTransitionButtons size="icon-sm">` — extend that component to support icon-only size.
   - Reuse the `useSectionSelection` hook so checkbox state mirrors card view.
2. **Row hover:** subtle `hover:bg-muted/40`, no left-border color trickery. Issues are communicated via the issues chip column, not row borders.
3. **Course-header checkbox:** indeterminate/checked logic from the shared selection hook.

**Verification**

- Selecting rows in table then switching to card view shows the same selection.
- Course header checkbox reflects the count of selected draft sections in that course.
- All buttons keyboard-accessible with visible focus rings.

---

## Phase 8 — Floating selection toolbar

Replaces the per-course inline batch bar with a single sticky bottom toolbar.

1. **Create `components/floating-selection-toolbar.tsx`**:
   - Sticky to the bottom of the page (`sticky bottom-4` inside the management layout's content area, OR `fixed bottom-4 left-1/2 -translate-x-1/2` for an "island" treatment — prefer the fixed-island look for prominence). `z-30`, animated slide-in (`data-[state=open]:animate-in slide-in-from-bottom-4`).
   - Renders only when `selectedIds.size > 0`.
   - Layout: selection count chip (`"3 sections selected"`) + small "Clear" link + Separator + action group (`Assign adviser`, `Open for enrollment`, `Cancel`) + close icon button.
   - Show inline warning chip ("Some selected sections are not Draft and will be skipped") if mixed statuses, computed from the selected IDs resolved against the loaded sections.
2. **Delete `batch-actions-toolbar.tsx`** and remove its references in card and table views.
3. **Bulk action handlers** dispatch into the `useSectionsDialogs` reducer to open the corresponding confirmation dialog/drawer.

**Verification**

- Selecting sections in any view shows the floating toolbar; deselecting all hides it.
- Mixed-status selection shows the inline warning; bulk actions still only operate on draft IDs.
- Toolbar dismisses cleanly on `Esc`.

---

## Phase 9 — Dialogs, drawers & feedback polish

Tighten the modal/drawer family and wire toast feedback throughout.

1. **`bulk-status-transition-dialog.tsx`** — keep the structure, but:
   - Use the existing semantic `Alert` component for the "Before opening" / "Effect of cancelling" blocks instead of hand-rolled bordered divs.
   - Inline Loader2 inside the action button label (already done) but also disable backdrop dismiss while pending.
2. **`bulk-adviser-assign-drawer.tsx`** — extract `<DrawerHeaderWithBadge>` shared with conflict drawer; surface the _list of selected sections_ (collapsible, max 5 visible, "+N more") so the user can verify before confirming.
3. **`conflict-preview-drawer.tsx`**:
   - Flatten the nested disclosure: one collapsible per offering, each one shows conflicts as a vertical list (no extra indent layer).
   - Replace the "Cannot open for enrollment" trailing div with a top-of-drawer `Alert` so it's visible before scrolling.
   - Add a small footer button "Open Room Scheduler" stub (`onClick` placeholder ok — navigation target TBD) so users see a clear next step.
4. **`section-form-drawer.tsx`** — remove the unused "Not editing" branch; tighten the adviser picker UI to match the bulk drawer's pattern; add toast on save success/failure.
5. **`cancel-section-alert-dialog.tsx`** — keep; add toast on success; convert the consequences block to the shared `Alert` styling.
6. **Toasts on every mutation:** `openClassSection`, `cancelClassSection`, `updateClassSection` (adviser save), `bulkOpenSections`, `bulkCancelSections`, `bulkAssignAdviser`. Use the `lib/sections-toast.ts` helpers from Phase 1.
7. **Delete `delete-section-alert-dialog.tsx`** (stub, not wired, not needed).
8. **All drawers** share consistent width (`w-[480px]`) and consistent `DrawerHeader` pattern.

**Verification**

- Every mutation surfaces a sonner toast (success or error).
- No drawers/dialogs exceed `w-[480px]`.
- Bulk drawers show the selected sections list before commit.
- `delete-section-alert-dialog.tsx` removed and no remaining imports.

---

## Phase 10 — Empty / loading / error / a11y polish

1. **`empty-state.tsx`** — keep three variants; bigger icon ring, more whitespace; for `no-college` add a primary `Open college selector` button that triggers the same dialog as the context bar.
2. **`sections-skeleton.tsx`** — match the redesigned layout: context bar skeleton + stats strip skeleton + grouped course cards.
3. **Error boundary:** wrap each Suspense in a small `<QueryErrorResetBoundary>` (use existing utility if available) with a `<Alert variant="destructive">` fallback and a Retry button.
4. **Accessibility audit pass:**
   - All clickable non-Button elements become `<button type="button">` with focus ring utilities.
   - Stats pills get `aria-pressed`; floating toolbar gets `role="toolbar" aria-label="Selected sections actions"`.
   - Conflict severity uses both color and a "Error"/"Warning" text badge.
   - HoverCard / Popover triggers have descriptive `aria-label`.
   - Mini-schedule font ≥ 11 px (WCAG AA-leaning).
5. **Responsive sweep:** verify context bar wraps cleanly on tablet width, stats strip falls back to 2x3 grid, card grid drops to single column at narrow widths, table gains horizontal scroll (`overflow-x-auto`) below `md`.
6. **Performance sanity:** memoize filtered course list once at page-component level (single `useMemo`) and pass through to both views to avoid duplicate work when toggling.

**Verification**

- Lighthouse-style spot check: no axe-violation-level issues (no missing labels, sufficient contrast, focusable controls reachable).
- Page renders at 1280, 1024, 768 widths without overflow or broken layouts.
- Stats Suspense fallback shows independently of content Suspense fallback.

---

## Relevant files

### Will be rewritten in place

- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/index.tsx` — slim orchestrator using new hooks + Suspense splits.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/searchParams.ts` — remove `quickFilter` enum value `needs-attention` is kept; `view` parser kept; consider removing `page/perPage/filters/sort/joinOperator` since not wired (decision: keep for forward-compat with DataTable, no behavior change).
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/college-selector.tsx` — visual refresh; same data wiring.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/stats-bar.tsx` → renamed in spirit to `section-stats-strip.tsx`; clustered groups; `aria-pressed`.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/section-card.tsx` — 3-zone layout, `MetaRow`, `IssuesChip`, `SchedulePeek` (HoverCard).
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/section-card-mini-schedule.tsx` — larger font, hatched conflict pattern, summary chips.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/sections-card-view.tsx` — course-card layout, sticky course header, shared selection hook, responsive grid.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/sections-table.tsx` — tightened columns, shared selection hook, icon-only `InlineTransitionButtons`.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/inline-transition-buttons.tsx` — add `icon-sm` size variant, otherwise behavior identical.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/conflict-preview-drawer.tsx` — flatten nesting, move blocking alert to top, add footer next-step button.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/bulk-adviser-assign-drawer.tsx` — show selected sections list, shared drawer header.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/bulk-status-transition-dialog.tsx` — use shared `Alert` for consequences.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/section-form-drawer.tsx` — drop dead "not editing" branch; toast on save.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/cancel-section-alert-dialog.tsx` — toast on success, shared `Alert` styling.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/empty-state.tsx` — primary action in `no-college` variant.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/sections-skeleton.tsx` — match new layout.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/section-status-badge.tsx` — minor polish (consistent height with badge sm variant); no API change.

### New files

- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/sections-context-bar.tsx` — sticky page header (college + term + view toggle + create slot).
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/floating-selection-toolbar.tsx` — global batch action bar.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/components/meta-row.tsx` — small primitive for card metadata rows.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/components/issues-chip.tsx` — shared compound errors/conflicts pill.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/components/progress-cell.tsx` — shared `{ bar, pct, tone }` cell used by card and table.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/components/schedule-peek.tsx` — `HoverCard` wrapper around the mini schedule.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/components/course-group-header.tsx` — shared course header (badge, progress, select-all chip).
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/hooks/use-section-selection.ts` — flat global selection store.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/hooks/use-sections-dialogs.ts` — reducer-based dialog state.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/lib/section-filters.ts` — `filterSectionsByQuickFilter`, `getSchedulingProgress`, `getValidationSummary`.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/lib/sections-toast.ts` — sonner wrappers.

### Will be deleted

- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/quick-filters.tsx` — folded into the stats strip.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/batch-actions-toolbar.tsx` — replaced by the floating selection toolbar.
- `src/system/enrollify-frontend/src/page-components/sections-management-page-v2/delete-section-alert-dialog.tsx` — non-functional stub, not part of the lifecycle this page owns.

### Unchanged but consumed

- `src/system/enrollify-frontend/src/api/collections/class-section-collection.ts` — every query/mutation reused as-is.
- `src/system/enrollify-frontend/src/api/collections/college-collection.ts` — `getAllCollegesOptions` reused.
- `src/system/enrollify-frontend/src/components/page-layouts/management-page-layout.tsx` — wrapping layout, unchanged.
- `src/system/enrollify-frontend/src/components/shared/search-teachers-dialog.tsx`, `search-college-dialog.tsx` — reused.
- `src/system/enrollify-frontend/src/components/ui/*` — shadcn primitives (Card, Badge, Button, HoverCard, ToggleGroup, Alert, AlertDialog, Drawer, Tooltip, Progress, Checkbox, Separator, ScrollArea, Skeleton, Collapsible, sonner).
- `src/system/enrollify-frontend/src/contexts/enrollment-context/enrollment-context.tsx` — `useEnrollmentContext()` reused.
- `src/system/enrollify-frontend/src/routes/portal/curriculum-and-scheduling/scheduling/sections.tsx` — route stays; default export still consumed.

---

## Verification (end-to-end)

1. **Type check:** `cd src/system/enrollify-frontend && npm run test:ts` — zero errors.
2. **Lint:** `npm run lint` — zero new warnings.
3. **Smoke test in dev:**
   - Open `/portal/curriculum-and-scheduling/scheduling/sections` with a selected academic term and a college.
   - Verify stats pills reflect counts and toggle filtering with active visual state.
   - Hover a section's "Preview" trigger → schedule peek opens with readable text and a hatched conflict block (when applicable).
   - Switch from card to table view → selection persists; same data shown.
   - Select sections across two course groups → floating selection toolbar shows total count; bulk action opens correct dialog/drawer; success toast appears after commit.
   - Trigger "Open for Enrollment" on a single draft section → success toast.
   - Trigger "Cancel" on a single section → cancel dialog uses shared Alert; success toast.
   - Click an issues chip on a card → conflict drawer opens, blocking alert visible at top.
   - With no college selected → empty state with `Open college selector` primary CTA.
   - With no term selected → context-bar inline alert; main content empty.
4. **Accessibility spot checks:**
   - Tab through stats strip; each pill is focusable with visible ring and announces pressed state.
   - Tab into a card; checkbox, change-adviser, transition buttons, preview trigger, details all reachable.
   - Conflict severity badges legible without color (text "Error"/"Warning" visible).
5. **Responsive checks:** 1440, 1280, 1024, 768 widths render cleanly; no horizontal overflow; table gets horizontal scroll below 768.
6. **No regressions in route:** `src/routes/portal/curriculum-and-scheduling/scheduling/sections.tsx` continues to import `SectionsManagementPageV2` default export.

---

## Out of scope (explicitly excluded)

- Any backend / API changes (no new endpoints, no schema updates).
- Wiring the "Bulk Initialize" creation flow (button stays disabled placeholder).
- Navigation target for "View details" / "Open Full Schedule" / "Open Room Scheduler" buttons — handlers stay as the existing placeholders (today they log to console; will continue to do so until a detail route is wired in a separate task).
- Multi-college dashboard view.
- Locked / Active / Completed lifecycle transitions (the page intentionally only owns Draft → Open and Draft/Open → Cancelled).
- TanStack Table migration of the section table.
- New permission / authorization rules.
- Section deletion (the stub dialog is removed; deletion isn't a workflow this page supports).
- Server-driven sort/filter for the section list (current view is full-list-for-college; pagination/server filtering would be a follow-up if list sizes grow).
