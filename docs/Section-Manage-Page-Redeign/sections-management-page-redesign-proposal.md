# Class Sections Management Page — Redesign Proposal

> **Audience:** School administrator / Scheduler responsible for scheduling class sections across colleges.
> **Goal:** Give the scheduler a bird's-eye view of scheduling status, surface conflicts early, enable batch operations, and reduce navigation overhead.

---

## Table of Contents

1. [Current Limitations](#1-current-limitations)
2. [Design Principles](#2-design-principles)
3. [Page Structure Overview](#3-page-structure-overview)
4. [Component Details & Wireframes](#4-component-details--wireframes)
   - [4.1 Stats Bar](#41-stats-bar)
   - [4.2 College / Course Grouping](#42-college--course-grouping)
   - [4.3 Section Card](#43-section-card)
   - [4.4 Enhanced Table View (Alternative)](#44-enhanced-table-view-alternative)
   - [4.5 View Toggle & Quick Filters](#45-view-toggle--quick-filters)
   - [4.6 Batch Operations Toolbar](#46-batch-operations-toolbar)
   - [4.7 Inline Status Transitions](#47-inline-status-transitions)
5. [Proposed File Structure](#5-proposed-file-structure)
6. [Implementation Phases](#6-implementation-phases)

---

## 1. Current Limitations

| Limitation | Impact on Scheduler |
|---|---|
| **Flat table** — all 250+ sections in one list | Hard to reason about sections per college/course |
| **No conflict visibility** — conflicts only visible on detail page (3 clicks away) | Scheduler must open every section to check for problems |
| **No scheduling progress** — can't see how many offerings have teacher/room/time assigned | Can't tell which sections are "done" vs "needs work" |
| **No batch operations** — actions are one-at-a-time | Bulk-assigning advisers or bulk-opening enrollment requires repetitive work |
| **Status transitions require detail page** — Open/Lock/Activate only available inside the section detail page | Extra navigation for a simple state change |
| **No summary metrics** — "How many sections have conflicts?" requires manual counting | No pulse on system health |

---

## 2. Design Principles

1. **Progressive disclosure** — show summary first, reveal details on demand.
2. **Conflict-first** — surface conflicts as early as possible (list-level badges, count indicators).
3. **Batch by default** — every action should have a multi-select variant.
4. **Grouped navigation** — sections belong to courses, which belong to colleges. Respect that hierarchy.
5. **Inline when possible** — status transitions, scheduling progress, and quick-edits should not require page navigation.

---

## 3. Page Structure Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  [Stats Bar: Draft 24 | Open 156 | Locked 43 | Conflicts 12 | Unscheduled  ]│
├─────────────────────────────────────────────────────────────────────────────┤
│  [Academic Year Selector]       [View: Card ◉ | Table ○ ]  [Quick Filters]  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  College of Engineering ─────────────────────────────────────────── 42 secs │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ │ BSCS — Computer Science  (12 sections)          [▮▮▮▮▮▮▮▮▮▮] 80%  │  │
│  │ │ ┌──────┬────────┬──────────┬──────┬──────────┐                    │  │
│  │ │ │ Name │ Status │ Sched %  │ Tchr │ Conflicts│                    │  │
│  │ │ ├──────┼────────┼──────────┼──────┼──────────┤                    │  │
│  │ │ │ 3A   │ █ Open │ ████░░ 2/3│ Smith│  ✗ 1    │                    │  │
│  │ │ │ 3B   │ █ Open │ ██████ 3/3│ Jones│  ✓ 0    │                    │  │
│  │ │ │ 3C   │ █ Lock │ ██████ 3/3│  --  │  ✓ 0    │                    │  │
│  │ │ └──────┴────────┴──────────┴──────┴──────────┘                    │  │
│  │ └───────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│  College of Business ────────────────────────────────────────────── 28 secs │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ │ BSA — Accountancy  (8 sections)                    [▮▮▮▮▮▮░░░░] 60%│  │
│  │ │ ...                                                               │  │
│  │ └───────────────────────────────────────────────────────────────────┘  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Component Details & Wireframes

### 4.1 Stats Bar

Displays aggregate counts. Each stat is clickable to filter the list.

```
┌───────────────────────────────────────────────────────────────────────────────┐
│  [📋 Draft 24]  [📂 Open 156]  [🔒 Locked 43]  [✅ Active 38]                │
│  [⚠️ Conflicts 12]  [🕐 Unscheduled 31]  [📊 312 Total]                       │
└───────────────────────────────────────────────────────────────────────────────┘
```

- Each stat is a button that adds/removes a filter for that status
- "Conflicts" stat counts sections with `unresolvedErrorsCount > 0`
- "Unscheduled" stat counts sections with 0 offerings scheduled (teacher + room + time assigned)
- Colors match `SectionStatusBadge` conventions

**Data source:** A lightweight aggregate endpoint or derive from the paginated response's summary metadata.

---

### 4.2 College / Course Grouping

Sections are grouped first by **College**, then by **Course** (program). Each group is an accordion.

```
┌─ College of Engineering ──────────────────────────────────────── 42 secs ──┐
│                                                                             │
│  BSCS — Computer Science (12 secs)             [████████░░] 80% scheduled   │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │  [Group actions bar]  ☐ Batch Select  │  [Assign Adviser] [Open All] │   │
│  │  ┌─────┬─────────┬───────────┬────────┬──────────┬───────────┐       │   │
│  │  │Code │ Status  │Sched Prog │ Teacher│ Conflicts│ Actions   │       │   │
│  │  ├─────┼─────────┼───────────┼────────┼──────────┼───────────┤       │   │
│  │  │ 3A  │ █ Open  │ 2/3 ████░░│ Smith  │  ✗ 1     │ [Open] [▸] │       │   │
│  │  │ 3B  │ █ Open  │ 3/3 ██████│ Jones  │  ✓ 0     │ [Lock] [▸] │       │   │
│  │  │ 3C  │ █ Locked│ 3/3 ██████│ Lee    │  ✓ 0     │ [Act.] [▸] │       │   │
│  │  └─────┴─────────┴───────────┴────────┴──────────┴───────────┘       │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  BSIT — Information Technology (8 secs)              [████░░░░░░] 45%       │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │  ...                                                                 │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
├─────────────────────────────────────────────────────────────────────────────┤
│  College of Business ──────────────────────────────────────────── 28 secs ──┤
│  ...                                                                         │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Group header** shows:
- College name
- Section count per course
- Aggregate scheduling progress bar (weighted average)
- "Batch Select" checkbox to act on all sections in the course

---

### 4.3 Section Card

Each section renders as a compact card (used in **card view**). This replaces a flat table row with richer visuals.

```
┌─────────────────────────────────────────────────────────────────┐
│  ☐ 3A                                             █ Open  ✗ 1  │
│  BSCS — Computer Science · 1st Semester AY 2025-2026            │
│  ─────────────────────────────────────────────────────────────── │
│  Adviser:     Dr. Smith  ──→  [Change]                          │
│  Scheduling:  [████████░░░░]  2 of 3 offerings scheduled        │
│  Teacher:     Smith (DS), Jones (MATH), _ (ENGL) ← needs assign │
│  Room:        101 (MWF), 102 (TTh)                              │
│  Schedule:    MWF 08:00-09:00, TTh 10:00-11:30                 │
│  ─────────────────────────────────────────────────────────────── │
│  [Open]  [Lock]  [Activate]  [Complete]  [Cancel]  [View Full] │
│                                                   [▸ Detail]   │
└─────────────────────────────────────────────────────────────────┘
```

**Card sections:**

| Zone                    | Content                                                             |
| ----------------------- | ------------------------------------------------------------------- |
| Header checkbox         | For batch selection                                                 |
| Section code (`3A`)     | Bold, primary identifier                                            |
| Status badge            | Colored per `SectionStatusBadge`                                    |
| Conflict badge          | Red count if conflicts exist, green checkmark if clean              |
| Subtitle                | Course name · Term                                                  |
| Adviser row             | Current adviser with quick-change button                            |
| Scheduling progress bar | Visual + fraction (e.g. "2 of 3 offerings")                         |
| Teacher summary         | Shows assigned teachers; highlights missing ones (`← needs assign`) |
| Room summary            | Compact room assignment per schedule pattern                        |
| Schedule summary        | Compact text showing day/time patterns                              |
| Action buttons          | Only show eligible transitions based on current status              |
| Detail link             | Navigate to section detail page                                     |

**Hover behaviour:** The card reveals a **mini weekly calendar** tooltip showing the section's schedule grid (smaller version of the detail page's weekly grid). This lets the scheduler visually check for overlaps without navigating.

---

### 4.4 Enhanced Table View (Alternative)

Some schedulers prefer dense tabular data. The **enhanced table** keeps the standard `DataTable` pattern but adds:

```
┌──────┬──────────┬──────────┬──────────┬──────┬──────┬──────────┬──────────┐
│ Code │ Section  │ Course   │ Status   │Sched%│Conf. │ Adviser  │ Actions  │
├──────┼──────────┼──────────┼──────────┼──────┼──────┼──────────┼──────────┤
│ 3A   │ BSCS 3A  │ Computer │ █ Open   │  67% │ ✗ 1  │ Dr.Smith │ [▸][O][L]│
│      │          │ Science  │          │ ████ │      │          │          │
│ 3B   │ BSCS 3B  │ Computer │ █ Open   │ 100% │ ✓ 0  │ Dr.Jones │ [▸][L][A]│
│      │          │ Science  │          │ ████ │      │          │          │
│ 3C   │ BSCS 3C  │ Computer │ █ Locked │ 100% │ ✓ 0  │ Dr.Lee   │ [▸][A][C]│
│      │          │ Science  │          │ ████ │      │          │          │
└──────┴──────────┴──────────┴──────────┴──────┴──────┴──────────┴──────────┘
```

**New/enhanced columns:**

| Column        | Description                                                             |
| ------------- | ----------------------------------------------------------------------- |
| **Sched%**    | Visual progress bar + percentage; click to see details                  |
| **Conflicts** | `✗ N` (red) or `✓ 0` (green); click to open conflict preview drawer     |
| **Actions**   | Status transition buttons + "Detail" link — always visible, no dropdown |
| **Course**    | Group header that spans the row group when in grouped mode              |

The table maintains all existing filtering/sorting/pagination via `nuqs` and `DataTableAdvancedToolbar`.

---

### 4.5 View Toggle & Quick Filters

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  [Academic Year: 2025-2026 ▾]     View: [Card ●] [Table ○]  [Export CSV]   │
│                                                                             │
│  Quick Filters:                                                             │
│  [All] [Draft] [Open] [Locked] [Active] [Has Conflicts] [Unscheduled]      │
│  [Needs Attention ◉]  ← combines: Draft + Has Conflicts + Unscheduled      │
└─────────────────────────────────────────────────────────────────────────────┘
```

- **Card/Table toggle** — persists choice in `sessionStorage`
- **Quick Filters** — preset filter configurations:
  - "Has Conflicts" → `unresolvedErrorsCount > 0`
  - "Unscheduled" → offerings with no teacher OR no room OR no schedules
  - "Needs Attention" → draft status OR has conflicts OR unscheduled
- **Export CSV** — downloads current filtered list as CSV for offline analysis

---

### 4.6 Batch Operations Toolbar

When one or more sections are selected (via row checkboxes or "Select All" in a course group):

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  ☑ 12 sections selected                                      [Clear]       │
│                                                                             │
│  [Assign Adviser ▾]  [Open Enrollment]  [Lock]  [Activate]  [Cancel]       │
│  [Bulk Edit ▾ → Capacity / Term / Year Level]                              │
│  [Export Selected]                                                          │
└─────────────────────────────────────────────────────────────────────────────┘
```

- Appears as a floating bar (sticky at bottom of viewport or below header)
- Each action opens a bulk-action drawer similar to `BulkInitializeSectionsDrawer`
- Adviser assignment opens `SearchTeachersDialog` with multi-select
- Bulk status transitions show a confirmation with count of affected sections

---

### 4.7 Inline Status Transitions

Each row/card shows only the **eligible** next-status buttons based on current state:

```
[Draft]    → [Open for Enrollment]  [Cancel]
[Open]     → [Lock Enrollment]      [Cancel]
[Locked]   → [Activate]             [Cancel]
[Active]   → [Complete]
[Completed]→ (no transitions)
[Cancelled]→ (no transitions)
```

These use the existing transition mutations (`openClassSectionOptions`, `lockClassSectionOptions`, etc.) with a confirmation toast/alert dialog (reuse `CancelSectionAlertDialog` pattern for each transition type).

---

## 5. Proposed File Structure

New/modified files under `src/page-components/sections-management-page/`:

```
sections-management-page/
├── index.tsx                           # Modified — adds view toggle, stats, grouping
├── searchParams.ts                     # Modified — adds 'view' param (card|table), groupBy
├── sections-table.tsx                  # Modified — enhanced columns (sched%, conflicts, actions)
├── sections-card-view.tsx              # NEW — card view layout with grouping
├── section-card.tsx                    # NEW — single section card component
├── section-card-mini-schedule.tsx      # NEW — mini weekly grid tooltip/hover card
├── stats-bar.tsx                       # NEW — aggregate statistics bar
├── quick-filters.tsx                   # NEW — preset filter buttons
├── batch-actions-toolbar.tsx           # NEW — floating batch operations bar
├── bulk-adviser-assign-drawer.tsx      # NEW — bulk assign adviser drawer
├── bulk-status-transition-dialog.tsx   # NEW — confirm bulk status change
├── conflict-preview-drawer.tsx         # NEW — inline conflict summary drawer
├── inline-transition-buttons.tsx       # NEW — status transition buttons (shared)
├── section-form-drawer.tsx             # Keep as-is (edit single section)
├── bulk-initialize-sections-drawer.tsx # Keep as-is
├── cancel-section-alert-dialog.tsx     # Keep as-is
├── delete-section-alert-dialog.tsx     # Keep as-is
├── section-status-badge.tsx            # Keep as-is
```

New/modified data layer:

```
src/api/collections/
├── class-section-collection.ts         # Modified — add bulk adviser, bulk transition, aggregate stats
src/api/models/
├── class-section.ts                    # Modified — add SchedulingSummary type if needed
```

---

## 6. Implementation Phases

### Phase 1: Quick Wins (1-2 days)
| Task | Files |
|---|---|
| Add inline status transition buttons to table rows | `inline-transition-buttons.tsx`, `sections-table.tsx` |
| Add conflict count + scheduling progress columns to table | `sections-table.tsx` |

### Phase 2: Overview & Navigation (2-3 days)
| Task | Files |
|---|---|
| Build stats bar component | `stats-bar.tsx` |
| Add card view toggle and quick filters | `sections-card-view.tsx`, `section-card.tsx`, `quick-filters.tsx`, `index.tsx` |
| Add college/course grouping to card view | `sections-card-view.tsx` (grouping logic) |

### Phase 3: Batch Operations (2-3 days)
| Task | Files |
|---|---|
| Build batch selection + floating toolbar | `batch-actions-toolbar.tsx` |
| Bulk adviser assignment drawer | `bulk-adviser-assign-drawer.tsx` |
| Bulk status transition with confirmation | `bulk-status-transition-dialog.tsx` |
| Add bulk API endpoints (backend) | `Enrollify.WebAPI` + `Enrollify.Application` |

### Phase 4: Conflict & Schedule Peek (2-3 days)
| Task | Files |
|---|---|
| Inline conflict preview drawer | `conflict-preview-drawer.tsx` |
| Mini weekly schedule tooltip on card hover | `section-card-mini-schedule.tsx` |
| Aggregate stats endpoint (backend) | `Enrollify.WebAPI` + `Enrollify.Application` |

### Phase 5: Polish & Performance (1-2 days)
| Task | Description |
|---|---|
| Debounce/optimistic updates for inline transitions | Ensure fast UX with background invalidation |
| Responsive card layout | Cards stack to 1-column on small screens |
| Keyboard navigation | Tab through cards, Enter to select, Space for batch |
| Accessibility | ARIA labels on progress bars, batch toolbar announcement |

---

## ASCII Wireframe: Final Page Mockup (Card View)

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  Curriculum & Scheduling  >  Class Sections  [Academic Year: 2025-2026 ▾]             │
├──────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                       │
│  [📋 Draft: 24]  [📂 Open: 156]  [🔒 Locked: 43]  [✅ Active: 38]                     │
│  [⚠️ Conflicts: 12]  [🕐 Unscheduled: 31]  [📊 Total: 312]                           │
│                                                                                       │
├──────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                       │
│  [● Card View]  [○ Table View]    │    Quick Filter: [All ▾] [✕ Conflicts Only]      │
│                                                                                       │
├──────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                       │
│  ▸ College of Engineering ────────────────────────────────────────────────────── ▼   │
│                                                                                       │
│    ○ BSCS — Computer Science (12)                     [████████░░░░] 68% scheduled    │
│    ┌────────────────────────────────────────────────────────────────────────────┐     │
│    │ ☐ Section        Status     Progress     Teacher      Room      Conflicts  │     │
│    │ ☑ 3A  BSCS 3A    █ Open     ████░░ 2/3   Dr.Smith    101(MWF)   ✗ 1       │     │
│    │    ☐ 3B  BSCS 3B  █ Open     ██████ 3/3   Dr.Jones    102(TTh)  ✓ 0       │     │
│    │    ☐ 3C  BSCS 3C  █ Locked   ██████ 3/3   Dr.Lee      201(MWF)  ✓ 0       │     │
│    │    ...                                                                      │     │
│    │    [☐ Select All]  [Assign Adviser]  [Open All]  [Lock All]  [Export]     │     │
│    └────────────────────────────────────────────────────────────────────────────┘     │
│                                                                                       │
│    ○ BSIT — Information Technology (8)                    [████░░░░░░░░] 42%          │
│    ┌────────────────────────────────────────────────────────────────────────────┐     │
│    │ ☐ Section        Status     Progress     Teacher      Room      Conflicts  │     │
│    │ ☐ 2A  BSIT 2A    █ Draft    ░░░░░░ 0/3   —            —         ✓ 0       │     │
│    │ ☑ 2B  BSIT 2B    █ Draft    ██░░░░ 1/3   Dr.Jones     —         ✓ 0       │     │
│    │ ...                                                                        │     │
│    └────────────────────────────────────────────────────────────────────────────┘     │
│                                                                                       │
│  ▸ College of Business ──────────────────────────────────────────────────────── ▼    │
│  ▸ College of Arts & Sciences ────────────────────────────────────────────────── ▼   │
│                                                                                       │
├──────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                       │
│  ☑ 5 sections selected  ───────────────────────────────────────────────────────────── │
│  [Assign Adviser ▾]  [Open for Enrollment]  [Lock]  [Export Selected]  [Clear]       │
│                                                                                       │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

---

## ASCII Wireframe: Final Page Mockup (Table View)

```
┌───────────────────────────────────────────────────────────────────────────────────────────┐
│  Curriculum & Scheduling  >  Class Sections    [Academic Year: 2025-2026 ▾]                 │
├───────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                            │
│  [📋 Draft: 24]  [📂 Open: 156]  [🔒 Locked: 43]  [✅ Active: 38]                          │
│  [⚠️ Conflicts: 12]  [🕐 Unscheduled: 31]  [📊 Total: 312]                                │
│                                                                                            │
├───────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                            │
│  [○ Card View]  [● Table View]  │  Group by: [None ▾]  │  Filter: [All ▾]  [Export CSV]   │
│                                                                                            │
├───────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                            │
│  ☐  │ Code │ Section Name   │ Course        │ Status   │ Progress │ Conflicts│ Teacher  │
│  ───┼──────┼────────────────┼───────────────┼──────────┼──────────┼──────────┼──────────┤
│  ☐  │ 3A   │ BSCS 3A        │ Computer Sci. │ █ Open   │ ████ 67% │  ✗ 1    │ Smith    │
│  ☐  │ 3B   │ BSCS 3B        │ Computer Sci. │ █ Open   │ ████ 100%│  ✓ 0    │ Jones    │
│  ☐  │ 3C   │ BSCS 3C        │ Computer Sci. │ █ Locked │ ████ 100%│  ✓ 0    │ Lee      │
│  ☐  │ 2A   │ BSIT 2A        │ Info Tech     │ █ Draft  │ ░░░░ 0%  │  ✓ 0    │ —        │
│  ☐  │ 2B   │ BSIT 2B        │ Info Tech     │ █ Draft  │ ██░ 33%  │  ✓ 0    │ Jones    │
│                                                                                            │
│  ──── Actions per row ────                                                                 │
│  [Open] [Lock] [Activate] [Cancel] [▸ Detail]                                               │
│                                                                                            │
├───────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                            │
│  ☑ 3 sections selected  ────────────────────────────────────────────────────────────────── │
│  [Assign Adviser ▾]  [Open]  [Lock]  [Cancel]  [Export Selected]  [Clear]                 │
│                                                                                            │
├───────────────────────────────────────────────────────────────────────────────────────────┤
│  Page 1 of 26  ◀ 1 2 3 ... 26 ▶  Per page: [25 ▾]                                         │
└───────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## Key Metrics to Surface

| Metric | Where | Data Source |
|---|---|---|
| **Scheduling progress** | Card progress bar, table column | `offerings.length` vs offerings with teacher+room+schedules assigned |
| **Conflict count** | Card badge, table column, stats bar | `unresolvedErrorsCount` (already available) |
| **Unscheduled count** | Stats bar | `offerings.length === 0 \|\| offerings.every(o => !o.teacher && !o.room)` |
| **Missing teacher** | Card teacher summary | `offerings.some(o => !o.teacher)` |
| **Missing room** | Card room summary | `offerings.some(o => !o.room)` |
| **Schedule density** | Mini grid tooltip | All `schedules` across offerings aggregated into day/time slots |

---

## Backend Changes Needed

| Endpoint | Purpose |
|---|---|
| `GET /api/class-sections/summary?academicYearId=N` | Aggregate stats (count per status, total conflicts, unscheduled count) |
| `POST /api/class-sections/bulk/assign-adviser` | Accept array of section IDs + adviser ID |
| `POST /api/class-sections/bulk/transition` | Accept array of section IDs + target status |
| `GET /api/class-sections/bulk/export?academicYearId=N&filters=...` | CSV export endpoint |

---

## Risk & Mitigation

| Risk | Mitigation |
|---|---|
| Performance with 300+ sections expanded | Lazy-load course groups; virtualize cards beyond 50 |
| BULK actions on stale data | Optimistic UI + refetch; conflict detection before status transitions |
| Cognitive overload (too much info) | Progressive disclosure (collapsed by default, expand on interaction) |
| Mobile / small screens | Card view stacks to 1 column; table view scrolls horizontally |
