# Class Sections Management Page — Product Requirements Document

## PRD-01: Overview

> **Status:** Approved (post-grilling session, 2026-06-10)
> **Audience:** School administrator / Scheduler responsible for scheduling class sections across colleges.
> **Goal:** Give the scheduler a bird's-eye view of scheduling status, surface conflicts early, enable batch operations (Draft sections only), and reduce navigation overhead.

---

### 1. Problem Statement

The current Class Sections management page has seven key limitations:

| Limitation | Impact on Scheduler |
|---|---|
| **Flat table** — all 250+ sections in one list | Hard to reason about sections per college/course |
| **No conflict visibility** — conflicts only visible on detail page (3 clicks away) | Scheduler must open every section to check for problems |
| **No scheduling progress** — can't see how many offerings have teacher/room/time assigned | Can't tell which sections are "done" vs "needs work" |
| **No batch operations** — actions are one-at-a-time | Bulk-assigning advisers or bulk-opening enrollment requires repetitive work |
| **Status transitions require detail page** — Open/Lock/Activate only available inside the section detail page | Extra navigation for a simple state change |
| **No summary metrics** — "How many sections have conflicts?" requires manual counting | No pulse on system health |
| **No card view** — only a dense table with no visual hierarchy | Hard to scan status, conflicts, and progress at a glance |

### 2. Design Principles

1. **Progressive disclosure** — show summary first, reveal details on demand.
2. **Conflict-first** — surface conflicts as early as possible (list-level badges, count indicators).
3. **Batch by default** — every action should have a multi-select variant.
4. **Grouped navigation** — sections belong to courses, which belong to colleges. Respect that hierarchy.
5. **Inline when possible** — status transitions, scheduling progress, and quick-edits should not require page navigation.

### 3. Scope

#### In Scope

- New route (`/sections-v2` or `/scheduling/sections`) — built from scratch alongside the existing page
- Two view modes: **Card view** (default) and **Enhanced Table view** (toggle persisted in `sessionStorage`)
- College → Course → Section accordion grouping in card view
- Stats bar with full-dataset aggregate counts (separate API endpoint)
- Quick filters: All, Draft, Open, Cancelled, Unresolved Errors, Conflicts, Needs Attention
- Inline status transitions (Draft ↔ Open, Draft/Open ↔ Cancelled) directly on cards and table rows
- Batch operations toolbar (floating, sticky) for Draft sections only
- Lazy-loaded offering details and mini weekly calendar on hover
- Separate full-dataset stats endpoint + per-section `validationSummary` embed
- Empty state design when filters return zero results
- Desktop-only (1024px+). Mobile/tablet users directed to Room Scheduler or Section Detail pages.

#### Out of Scope

- Export functionality (data analysis happens in Room Scheduler page)
- Mobile/tablet responsive layout
- Modifying existing `/sections` page components (old page remains at its route until manually deleted)
- Open → Locked, Locked → Active, Active → Completed transitions (automated, managed outside this page per Class Section Lifecycle)
- Per-teacher conflict view (Section Detail page)
- Per-room conflict view (Room Scheduler page)

### 4. Success Metrics

| Metric | Target | How Measured |
|---|---|---|
| **Sections opened per session** | ≥50% improvement over current page | Before/after analytics |
| **Time to identify conflicts** | <5 seconds (currently 3+ clicks/navigations) | User observation |
| **Batch operations usage** | ≥30% of status transitions use batch | Mutation logging |
| **Card view adoption** | ≥60% of sessions use card view | `sessionStorage` toggle read |
| **Stats bar interaction** | ≥40% of sessions click a stat to filter | Click event analytics |

### 5. Key Resolved Decisions (from Design Grilling)

| Decision | Resolution | Rationale |
|---|---|---|
| Checkbox behavior | Checkboxes disabled for non-Draft sections | Batch ops are Draft-only; showing active checkboxes on Open/Locked rows misleads |
| Conflict/Error badge | Compound `⚠️X/❌Y` format in both views, colored amber for errors-only, red for conflicts | Single badge saves space; compound preserves diagnostic info; clicking opens conflict-preview drawer |
| Conflicts block enrollment | YES — both unresolved errors **and** conflicts prevent "Open for Enrollment" | Consistency: scheduler should not open a section that has any issue |
| Open sections can be cancelled | YES — lifecycle doc shows `Open → Cancelled` | Open sections need Cancel button alongside `[Details]` |
| Needs Attention scope | Draft sections only (with errors or conflicts) | Open section issues are read-only observational; action happens in Draft |
| Stats endpoint | Separate API endpoint for full-dataset aggregate stats | Avoids computing full-dataset stats on every paginated call; keeps paginated responses lightweight |
| Per-section stats | Embedded `validationSummary` per section on paginated response | Card/table rendering uses this directly; no extra computation |
| Offering details | Lazy-loaded (not included in paginated response body) | Keeps payload size manageable; avoids 2-3x blowup from nested validation + conflict arrays |
| Mini calendar | Static summary rendered on hover immediately; full mini-weekly grid fetched async | Hover is instant (no blank tooltip); data loads in background |
| Virtualization strategy | Virtualize College accordion headers only; `content-visibility: auto` on inner course blocks | Nested accordion is too complex for full virtual scrolling; college count is small enough |
| Course-level vs Global toolbar | Both co-exist. Course toolbar scoped to that course's selections. Global toolbar affects all selected across courses | Each level has its use case; clear labeling prevents confusion |
| Bulk Cancel | Soft-delete cascade; frees up teacher/room assignments; confirmation dialog shows "X offerings will be freed" | Prevents resource leaks; user knows impact before confirming |
| Page strategy | New route (`/sections-v2`), built from scratch, old page kept until manually deleted | Avoids regressions; faster iteration; no constraint of existing component API |
| Empty states | Stats bar always visible showing full-dataset numbers. List area shows empty state with "Clear All Filters" CTA | User sees context even when list is empty; clear path to recovery |
