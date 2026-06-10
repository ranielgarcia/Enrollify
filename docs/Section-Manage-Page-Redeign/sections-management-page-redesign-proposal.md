# Class Sections Management Page — Redesign Proposal

> **Audience:** School administrator / Scheduler responsible for scheduling class sections across colleges.
> **Goal:** Give the scheduler a bird's-eye view of scheduling status, surface conflicts early, enable batch operations (Draft sections only), and reduce navigation overhead.

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
   - [4.8 Mini Weekly Calendar (Hover Tooltip)](#48-mini-weekly-calendar-hover-tooltip)
5. [Proposed File Structure](#5-proposed-file-structure)
6. [Implementation Phases](#6-implementation-phases)
7. [Performance Considerations](#7-performance-considerations)
8. [API Response Structure](#8-api-response-structure)

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
│  [Stats Bar: Draft 24 | Open 156 | Cancelled 8 | Conflicts 12 | Unscheduled  ]│
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

Displays aggregate counts **for Draft, Open, and Cancelled statuses only**. Each stat is clickable to filter the list.

```
┌───────────────────────────────────────────────────────────────────────────────┐
│  [📋 Draft 24]  [📂 Open 156]  [❌ Cancelled 8]                               │
│  [⚠️ Unresolved Errors 12]  [🔴 Conflicts 7]  [🕐 Unscheduled 31]             │
└───────────────────────────────────────────────────────────────────────────────┘
```

**Stat Definitions:**
- **Draft (X)** — Sections in Draft status (newly created, awaiting scheduling)
- **Open (X)** — Sections opened for enrollment (scheduling complete)
- **Cancelled (X)** — Sections that were cancelled before/after opening
- **Unresolved Errors (X)** — Count of Draft/Open/Cancelled sections with ≥1 offering missing teacher OR room OR schedule
- **Conflicts (X)** — Count of Draft/Open/Cancelled sections with ≥1 offering that has scheduling conflicts with other sections' offerings. Conflicts are only computed for Draft sections (Open sections are considered finalized).
- **Unscheduled (X)** — Count of Draft/Open/Cancelled sections with 0 complete offerings (offerings lacking teacher+room+schedule)

**Note:** Locked, Active, and Completed statuses are excluded from stats bar as they are managed outside the scheduling workflow.

- Each stat is a button that adds/removes a filter for that status
- Colors match `SectionStatusBadge` conventions (Draft=secondary, Open=green, Cancelled=red)
- Stats always reflect **full-dataset numbers** regardless of current page or active filters

**Data source:** Separate API endpoint (`GET /api/class-sections/stats`) dedicated to aggregate stats. Refreshed on page load and after mutations.

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
│  │  │Code │ Status  │Sched Prog │ Teacher│Errors/CF │ Actions   │       │   │
│  │  ├─────┼─────────┼───────────┼────────┼──────────┼───────────┤       │   │
│  │  │ 3A  │ Draft   │ 2/3 ████░░│ Smith  │⚠️ 1/❌ 0 │ [Open][∅] │       │   │
│  │  │ 3B  │ █ Open  │ 3/3 ██████│ Jones  │✓ 0 /✓ 0 │ [▸]       │       │   │
│  │  │ 3C  │ █ Open  │ 3/3 ██████│ Lee    │✓ 0 /✓ 0 │ [▸]       │       │   │
│  │  │ 2A  │ █ Draft │ 1/3 ██░░░░│  —     │⚠️ 2/❌ 1 │ [Open][∅] │       │   │
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
- "Batch Select" checkbox (applies only to **Draft sections** in this course; disabled for non-Draft)

**Status & Actions Column:**

| Status | Actions | Notes |
|---|---|---|
| **Draft** | `[Open]` `[Cancel]` `[▸ Details]` | Fully editable; actions available for batch operations |
| **Open** | `[Cancel]` `[▸ Details]` | Cancellable per lifecycle (Open → Cancelled); otherwise read-only |
| **Locked** | `[▸ Details]` | Read-only (managed by enrollment deadline automation) |
| **Active** | `[▸ Details]` | Read-only (managed by term start automation) |
| **Completed** | `[▸ Details]` | Read-only (managed by term end automation) |
| **Cancelled** | `[▸ Details]` | Read-only; no further actions |

**Checkbox behavior:**
- Checkboxes are **disabled** (grayed out) for non-Draft sections
- Only Draft section checkboxes are interactive and can be selected for batch operations
- Tooltip on disabled checkboxes: "Batch operations only available for Draft sections"

**Course-level Group Actions Bar:**
Each course group has its own toolbar with `[☐ Select All]` `[Assign Adviser]` `[Open All]` `[Cancel All]`. These actions are **scoped to that course only** — they affect only the sections within that specific course group. "Open All" transitions only Draft sections (validates eligibility per section).

**Errors/Conflicts Column Display:**
- Format: `⚠️ X / ❌ Y` where:
  - `⚠️ X` = Unresolved errors count (missing teacher, room, or schedule across offerings)
  - `❌ Y` = Conflicts count (scheduling conflicts with other sections)
- Amber color when only errors present; red when conflicts present (or both)
- Click to see details in drawer (see Section 4.9)
- Red left border on row if errors OR conflicts exist

---

### 4.3 Section Card

Each section renders as a compact card (used in **card view**). This replaces a flat table row with richer visuals.

```
Draft Section (Editable):
┌─────────────────────────────────────────────────────────────────┐
│ ☐ 3A                                         █ Draft  ⚠️1/❌0  │
│ BSCS — Computer Science · 1st Semester AY 2025-2026             │
│ ─────────────────────────────────────────────────────────────── │
│ Adviser:     Dr. Smith  ──→  [Change]                           │
│ Scheduling:  [████████░░░░]  2 of 3 offerings scheduled         │
│ Issues:      1 offering missing teacher · 1 conflict detected   │
│ Rooms:       101, 102, (missing room for offering 1)           │
│ Schedule:    MWF 08:00-09:00, TTh 10:00-11:30, ? 1 offering   │
│ ─────────────────────────────────────────────────────────────── │
│ [Open for Enrollment]  [Cancel]  [View Details →]              │
│                                                                 │
│ ⓘ Hover to see weekly schedule grid with conflicts            │
└─────────────────────────────────────────────────────────────────┘

Open Section (Cancellable):
┌─────────────────────────────────────────────────────────────────┐
│ ☐ 3A  (disabled)                       █ Open   ✓0/✓0          │
│ BSCS — Computer Science · 1st Semester AY 2025-2026             │
│ ─────────────────────────────────────────────────────────────── │
│ Adviser:     Dr. Smith                                          │
│ Scheduling:  [██████████]  3 of 3 offerings scheduled (100%)    │
│ Teachers:    Smith (DS), Jones (MATH), Lee (ENGL)              │
│ Rooms:       101 (MWF), 102 (TTh), 103 (Wed)                   │
│ Schedule:    MWF 08:00-09:00, TTh 10:00-11:30, Wed 14:00-15:00│
│ ─────────────────────────────────────────────────────────────── │
│ [Cancel]  [View Details →]                                      │
│                                                                 │
│ ⓘ Hover to see weekly schedule grid                            │
└─────────────────────────────────────────────────────────────────┘

Locked/Active/Completed/Cancelled Section (Read-Only):
┌─────────────────────────────────────────────────────────────────┐
│ ☐ 3A  (disabled)                       █ Locked                │
│ BSCS — Computer Science · 1st Semester AY 2025-2026             │
│ ─────────────────────────────────────────────────────────────── │
│ Adviser:     Dr. Smith                                          │
│ Scheduling:  [██████████]  3 of 3 offerings scheduled (100%)    │
│ Teachers:    Smith (DS), Jones (MATH), Lee (ENGL)              │
│ Rooms:       101 (MWF), 102 (TTh), 103 (Wed)                   │
│ Schedule:    MWF 08:00-09:00, TTh 10:00-11:30, Wed 14:00-15:00│
│ ─────────────────────────────────────────────────────────────── │
│ [View Details →]                                                │
└─────────────────────────────────────────────────────────────────┘
```

**Card sections:**

| Zone                    | Content                                                             |
| ----------------------- | ------------------------------------------------------------------- |
| Header checkbox         | For batch selection; **disabled** (grayed) for non-Draft sections   |
| Section code (`3A`)     | Bold, primary identifier                                            |
| Status badge            | Colored per `SectionStatusBadge`                                    |
| Error/Conflict badge    | Compound `⚠️X/❌Y` format; **amber** when only errors present, **red** when conflicts present (or both); clickable → opens conflict preview drawer |
| Subtitle                | Course name · Term                                                  |
| Adviser row             | Current adviser; change button only for Draft                       |
| Scheduling progress bar | Visual + fraction (e.g. "2 of 3 offerings" or "100%")              |
| Issues row              | Summary of missing teacher/room/schedule issues (Draft only)        |
| Room summary            | Compact room assignment per schedule pattern                        |
| Schedule summary        | Compact text showing day/time patterns; gaps indicated              |
| Action buttons          | **Draft:** [Open], [Cancel], [Details]; **Open:** [Cancel], [Details]; **Others:** [Details] only |
| Detail link             | Navigate to section detail page for full editing                   |

**Hover behaviour:** The card reveals a **mini weekly calendar** tooltip (see Section 4.8). A static summary is shown immediately on hover; the full weekly grid loads asynchronously from the offering details endpoint.

**Visual Indicators:**
- **Red left border** if section has ≥1 unresolved error OR ≥1 conflict
- **Disabled checkbox** on non-Draft sections (grayed out with tooltip on hover)
- **Disabled state** for action buttons on non-Draft/non-Open sections

---

### 4.4 Enhanced Table View (Alternative)

Some schedulers prefer dense tabular data. The **enhanced table** keeps the standard `DataTable` pattern but adds scheduling-specific columns and lifecycle-aware actions.

```
┌──────┬──────────┬──────────┬─────────┬──────┬──────────┬──────────┬──────────┐
│ Code │ Section  │ Course   │ Status  │Sched%│Errors/CF │ Adviser  │ Actions  │
├──────┼──────────┼──────────┼─────────┼──────┼──────────┼──────────┼──────────┤
│ 3A   │ BSCS 3A  │ Computer │ Draft   │  67% │ ⚠️1/❌0  │ Dr.Smith │ [O][C][▸]│
│      │          │ Science  │ (row    │ ████ │          │          │          │
│      │          │          │ red)    │      │          │          │          │
│ 3B   │ BSCS 3B  │ Computer │ █ Open  │ 100% │ ✓0/✓0   │ Dr.Jones │ [C][▸]    │
│      │          │ Science  │ (read)  │ ████ │          │          │          │
│ 3C   │ BSCS 3C  │ Computer │ █ Open  │ 100% │ ✓0/✓0   │ Dr.Lee   │ [C][▸]    │
│      │          │ Science  │ (read)  │ ████ │          │          │          │
└──────┴──────────┴──────────┴─────────┴──────┴──────────┴──────────┴──────────┘

Legend:
  [O] = Open for Enrollment (Draft only)
  [C] = Cancel (Draft and Open)
  [▸] = View Details
  ⚠️1/❌0 = 1 unresolved error, 0 conflicts
```

**New/enhanced columns:**

| Column            | Description                                                             |
| ----------------- | ----------------------------------------------------------------------- |
| **Sched%**        | Visual progress bar + percentage (0-100%); hover for offering breakdown |
| **Errors/Conflicts** | Format: `⚠️X/❌Y` where X=unresolved errors, Y=conflicts; click for details |
| **Actions**       | Status buttons (Draft: [Open][Cancel], Open: [Cancel][Details], Others: [Details]); always visible |
| **Course**        | Group header that spans the row group when in grouped mode              |

**Visual Indicators:**
- **Red left border** on rows with errors OR conflicts
- **Gray background** on non-Draft (read-only) rows
- **Disabled buttons** for non-Draft status transitions

The table maintains all existing filtering/sorting/pagination via `nuqs` and `DataTableAdvancedToolbar`.

---

### 4.5 View Toggle & Quick Filters

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  [Academic Year: 2025-2026 ▾]     View: [Card ●] [Table ○]                 │
│                                                                             │
│  Quick Filters:                                                             │
│  [All] [Draft] [Open] [Cancelled] [Unresolved Errors] [Conflicts]          │
│  [Needs Attention ◉]  ← combines: Draft + Unresolved Errors + Conflicts    │
└─────────────────────────────────────────────────────────────────────────────┘
```

- **Card/Table toggle** — persists choice in `sessionStorage`
- **Quick Filters** — preset filter configurations:
  - "Unresolved Errors" → sections with ≥1 offering missing teacher OR room OR schedule
  - "Conflicts" → sections with ≥1 offering that has scheduling conflicts
  - "Needs Attention" → Draft status OR has unresolved errors OR has conflicts
- **Status filters:** Draft, Open, Cancelled only (Locked/Active/Completed excluded per scheduling scope)
- **Note:** Export functionality is not currently supported; data analysis happens in detail/room scheduler pages

---

### 4.6 Batch Operations Toolbar

When one or more **Draft sections** are selected (via row checkboxes or "Select All" in a course group):

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  ☑ 5 Draft sections selected                                   [Clear]     │
│                                                                             │
│  [Assign Adviser ▾]  [Open for Enrollment]  [Cancel]                       │
│  [Bulk Edit ▾ → Capacity / Term / Year Level]                              │
│                                                                             │
│  ⓘ Batch operations only available for Draft sections                      │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Two Levels of Batch Toolbars:**

| Level | Location | Scope |
|---|---|---|
| **Course-level** | Inside each expanded Course Group | Actions affect only sections selected within that specific course |
| **Global** | Floating sticky bar (bottom of viewport) | Actions affect all selected sections across all courses on the current page |

**Behavior:**
- **Only Draft sections can be batch-operated.** If non-Draft sections are selected, batch buttons are disabled with tooltip: "Batch operations only available for Draft sections"
- Course-level toolbar actions are scoped to that course's selections only (e.g., clicking "Assign Adviser" on Course A's toolbar affects only Course A's selected sections)
- Global toolbar actions affect all selected sections across all courses
- Each action opens a bulk-action drawer similar to `BulkInitializeSectionsDrawer`
- Adviser assignment opens `SearchTeachersDialog` with multi-select
- Status transitions show a confirmation with count of affected sections

**Available Batch Actions (Draft only):**
| Action | Purpose |
|---|---|
| **Assign Adviser** | Bulk assign same adviser to multiple Draft sections |
| **Open for Enrollment** | Transition selected Draft sections to Open status (validates eligibility per section) |
| **Cancel** | Cancel selected Draft sections. Shows confirmation dialog with "X offerings will be freed" (teacher/room assignments released via soft-delete cascade) |
| **Bulk Edit** | Edit capacity, term, or year level across multiple sections |

**Constraints:**
- Cannot batch-operate on mixed statuses. Show warning if user selects both Draft and non-Draft sections.
- "Open All" (course-level) only affects Draft sections in that course. Eligible sections are validated individually by the backend.

---

### 4.7 Inline Status Transitions

This page supports transitions for **Draft** and **Open** sections. Locked, Active, and Completed are managed outside the scheduling workflow and are read-only.

```
[Draft]      → [Open for Enrollment]  [Cancel]  [View Details]
[Open]       → [Cancel]  [View Details]
[Locked]     → [View Details] (read-only, managed by enrollment deadline)
[Active]     → [View Details] (read-only, managed by term start)
[Completed]  → [View Details] (read-only, managed by term end)
[Cancelled]  → [View Details] (read-only)
```

**Class Section Lifecycle Context:**
Per `docs/design-decisions/class-section-lifecycle.md`:
- **Draft → Open:** Admin opens enrollment period (happens on this page)
- **Draft → Cancelled:** Admin cancels before opening (happens on this page)
- **Open → Cancelled:** Admin cancels section (happens on this page)
- **Open → Locked:** Automatic when enrollment deadline passes (not on this page)
- **Locked → Active:** Automatic when term begins (not on this page)
- **Active → Completed:** Automatic when term ends (not on this page)
- **Active → Cancelled:** Emergency cancellation (handled by admin, potentially outside this page)

**Post-Transition Behavior:**
- Card does **not** visually move — it stays in place
- Status badge updates immediately (optimistic update)
- Action buttons reflect new status (e.g., Draft → Open changes buttons from `[Open][Cancel][Details]` to `[Cancel][Details]`)
- Stats bar refreshes to reflect updated counts
- Toast notification shown on success; inline error on failure

**Implementation Notes:**
- Use existing transition mutations: `openClassSectionOptions(sectionId)`, `cancelClassSectionOptions(sectionId)`
- Show confirmation alert dialog (reuse `CancelSectionAlertDialog` pattern) before cancelling
- For Draft → Open: backend validates enrollment eligibility; rejection shows inline error
- After successful transition, refetch sections + stats and show toast notification

---

### 4.8 Mini Weekly Calendar (Hover Tooltip)

When hovering over a section card or row, a tooltip appears showing a **compact weekly grid** of scheduled offerings with conflict visualization.

```
┌────────────────────────────────────────────────────────────────────┐
│  BSCS 3A — Weekly Schedule                    [View Full Schedule] │
├────────────────────────────────────────────────────────────────────┤
│           MON         TUE         WED         THU         FRI      │
│  08:00 ┌─────────┐                                                  │
│        │ DS-Smith│ (Room 101)                                       │
│  09:00 └─────────┘                                                  │
│  10:00                 ┌──────────┐                                 │
│        │ MATH-Jones   │ (Room 102)                                  │
│  11:00                 └──────────┘                                 │
│  12:00          [ LUNCH BREAK ]                                     │
│  13:00                             ┌──────────┐                     │
│        │ ENGL-Lee    │ (Room 103)                                   │
│  14:00                             └──────────┘                     │
│  15:00 ┌─────────────────────────┐ (CONFLICT!)                    │
│        │ ⚠️ BSEE 2A-Lee          │ (Same room+time overlap)         │
│        │ From Eng. College       │                                  │
│  16:00 └─────────────────────────┘                                 │
│                                                                     │
│  Legend:  ■ Current Section  ■ Conflicting Offering (red border)  │
└────────────────────────────────────────────────────────────────────┘
```

**Content Display:**
- **Days:** Monday through Friday (or 7-day view based on offerings)
- **Time slots:** 30-minute or hourly intervals (configurable)
- **Current section offerings:** Colored boxes with subject code, teacher surname, room number
- **Conflicting offerings:** Red-bordered boxes showing the conflicting section's offering
- **Lunch break indicators:** Gray cells for typical lunch periods (12:00-13:00)

**Conflict Information (from API):**
The hover tooltip receives **conflicting offerings data from the offering details endpoint**. For each offering with conflicts, the endpoint includes:
- `conflictingOfferingId`, `conflictingSection`, `conflictingTeacher`, `conflictingRoom`
- Time range of overlap
- Conflict severity

**Interaction:**
- Click "View Full Schedule" → navigates to section detail page's "Weekly Grid" tab
- Click conflicting offering box → navigates to conflicting section's detail page
- Tooltip auto-closes when mouse leaves card

**Performance & Loading Strategy (two-phase):**
1. **Phase 1 (instant):** Static summary text is rendered on hover immediately (e.g., "3 offerings, 1 conflict detected"). No data fetch — derived from the `validationSummary` already available in the paginated response.
2. **Phase 2 (async):** Full mini weekly grid with conflict visualization is fetched from `GET /api/class-sections/{sectionId}/offerings` (offering details endpoint) and rendered once loaded. A subtle loading indicator shows during fetch.
- Mini calendar is **lazy-rendered** on hover (not pre-rendered for all cards)
- Offering details are **cached** per sectionId after first fetch (TanStack Query)
- No offering data is embedded in the paginated response — keeps payload lean

All files live under a new directory `src/page-components/sections-management-page-v2/`. The existing `/sections` page and its components are **not modified** — the new page runs at a separate route (e.g., `/scheduling/sections`).

```
sections-management-page-v2/
├── index.tsx                           # Page root — view toggle, stats, grouping orchestration
├── searchParams.ts                     # nuqs parsers (view, groupBy, filters, sort, page)
├── sections-table.tsx                  # Enhanced table view (new columns, inline actions)
├── sections-card-view.tsx              # Card view layout with college/course grouping
├── section-card.tsx                    # Single section card component
├── section-card-mini-schedule.tsx      # Mini weekly grid tooltip/hover card (async load)
├── stats-bar.tsx                       # Aggregate statistics bar (clickable filters)
├── quick-filters.tsx                   # Preset filter buttons
├── batch-actions-toolbar.tsx           # Floating sticky batch operations bar
├── bulk-adviser-assign-drawer.tsx      # Bulk assign adviser drawer
├── bulk-status-transition-dialog.tsx   # Confirm bulk status change
├── conflict-preview-drawer.tsx         # Inline conflict summary drawer
├── inline-transition-buttons.tsx       # Status transition buttons — Draft(Open/Cancel), Open(Cancel), Others(Details)
├── empty-state.tsx                     # Empty state component (filtered + no-data variants)
├── section-status-badge.tsx            # Re-implemented (no dependency on old page)
├── cancel-section-alert-dialog.tsx     # Re-implemented with bulk-cancel awareness
└── section-form-drawer.tsx             # Edit single section form (keep as-is pattern)
```

New/modified data layer:

```
src/api/collections/
├── class-section-collection-v2.ts      # NEW — queries/mutations for sections-v2 page
src/api/models/
├── class-section.ts                    # Extend with SchedulingSummary type
```

---

## 6. Implementation Phases

### Phase 1: Scaffold & Data Layer (2-3 days)
**Goal:** New route, new files, backend API extensions, and query/mutation infrastructure.

| Task | Deliverable | Dependencies |
|---|---|---|
| Create new route `/scheduling/sections` with TanStack Router file | `src/routes/scheduling.sections.tsx` | None |
| Set up `searchParams.ts` with added `view` (card/table) param | `searchParams.ts` | None |
| Extend backend paginated endpoint to include `validationSummary` per section | Backend: `FilterClassSectionsPaginatedQuery` | None |
| Create backend aggregate stats endpoint (full-dataset, separate call) | Backend: `GET /api/class-sections/stats` | None |
| Create backend offering details lazy endpoint | Backend: `GET /api/class-sections/{sectionId}/offerings` | None |
| Create backend bulk operation endpoints (open, cancel, assign-adviser) | Backend: 3 POST endpoints | None |
| Update frontend API collection with new queries/mutations | `class-section-collection-v2.ts` | Backend endpoints |
| Regenerate API types (`npm run generate:api:win`) | `src/api/generated/api.ts` | Backend endpoints |
| Create page scaffold (`index.tsx`) with `useSuspenseQuery` for sections + stats | `index.tsx` | API collection |

### Phase 2: Card View (2-3 days)
**Goal:** Card view with college/course grouping, section cards, progress display.

| Task | Deliverable | Dependencies |
|---|---|---|
| Build `StatsBar` component with clickable stat buttons | `stats-bar.tsx` | Phase 1 API |
| Build `QuickFilters` component | `quick-filters.tsx` | None |
| Build `SectionCard` component with all zones (header, adviser, progress, issues, actions) | `section-card.tsx` | Phase 1 data types |
| Build `SectionsCardView` with CollegeAccordion → CourseGroup → SectionCard nesting | `sections-card-view.tsx` | `SectionCard` |
| Wire up view toggle in `index.tsx` | `index.tsx` | All above |
| Build `InlineTransitionButtons` with Draft(Open/Cancel), Open(Cancel), Others(Details) | `inline-transition-buttons.tsx` | API mutations |
| Integrate status transitions on card (Draft ↔ Open, Draft/Open → Cancelled) | `section-card.tsx` | `InlineTransitionButtons` |
| Add red left border for sections with errors/conflicts | `section-card.tsx` | `validationSummary` |

### Phase 3: Enhanced Table View (1-2 days)
**Goal:** Table view with enhanced columns.

| Task | Deliverable | Dependencies |
|---|---|---|
| Build `SectionsTable` with new columns (Progress bar, Errors/Conflicts badge, inline actions) | `sections-table.tsx` | Phase 1 data types |
| Add red left border on rows with errors/conflicts | `sections-table.tsx` | Phase 1 |
| Add gray background for non-Draft rows | `sections-table.tsx` | Phase 1 |
| Add compound badge (`⚠️X/❌Y`) to table rows | `sections-table.tsx` | Phase 1 |
| Wire table view in `index.tsx` | `index.tsx` | Phase 2 view toggle |

### Phase 4: Batch Operations (2-3 days)
**Goal:** Batch select, floating toolbar, course-level toolbar, bulk actions.

| Task | Deliverable | Dependencies |
|---|---|---|
| Build batch selection state management (local `Set<number>`, course-scoped + global) | `sections-card-view.tsx` + `sections-table.tsx` | Phases 2-3 |
| Build floating `BatchActionsToolbar` (global, sticky) | `batch-actions-toolbar.tsx` | Selection state |
| Build `BulkStatusTransitionDialog` with count summary + "X offerings will be freed" | `bulk-status-transition-dialog.tsx` | Phase 1 bulk endpoints |
| Build `BulkAdviserAssignDrawer` | `bulk-adviser-assign-drawer.tsx` | Phase 1 bulk endpoints |
| Add course-level group toolbar with scoped batch actions | `sections-card-view.tsx` (CourseGroupToolbar) | Phase 2 |
| Disable batch buttons when non-Draft sections are selected (with tooltip) | `batch-actions-toolbar.tsx` | Selection state |

### Phase 5: Conflict & Schedule Peek (2-3 days)
**Goal:** Mini calendar tooltip, conflict preview drawer, lazy offering loading.

| Task | Deliverable | Dependencies |
|---|---|---|
| Build `ConflictPreviewDrawer` (click from compound badge) | `conflict-preview-drawer.tsx` | Phase 1 offering details endpoint |
| Build `SectionCardMiniSchedule` — static summary on hover + async full grid load | `section-card-mini-schedule.tsx` | Phase 1 offering details endpoint |
| Wire lazy offering fetch into card hover/expand interaction | `section-card.tsx` → `section-card-mini-schedule.tsx` | Phase 1 |

### Phase 6: Empty State, Polish & Performance (1-2 days)
**Goal:** Empty states, edge cases, performance optimization, accessibility.

| Task | Deliverable | Dependencies |
|---|---|---|
| Build `EmptyState` component (filtered + no-data variants) | `empty-state.tsx` | All phases |
| Wire empty state into card and table views | `sections-card-view.tsx`, `sections-table.tsx` | `EmptyState` |
| Add 300ms debounce on filter changes | `searchParams.ts` or `index.tsx` | None |
| Add `content-visibility: auto` to CourseGroup blocks | `sections-card-view.tsx` CSS | Phase 2 |
| Virtualize College accordion headers (TanStack Virtual) | `sections-card-view.tsx` | Phase 2 |
| Add desktop-only viewport check with redirect suggestion banner | `index.tsx` | None |
| Add ARIA labels on progress bars, batch toolbar announcements | All components | All phases |
| Keyboard navigation (Tab through cards, Enter to select, Space for batch) | `section-card.tsx`, `sections-table.tsx` | All phases |

### Phase 7: Integration Tests (2-3 days)
**Goal:** Full test coverage for new endpoints and page.

| Task | Deliverable | Dependencies |
|---|---|---|
| WebAPI tests for aggregate stats endpoint | `Enrollify.IntegrationTests` | Phase 1 backend |
| WebAPI tests for bulk operation endpoints | `Enrollify.IntegrationTests` | Phase 1 backend |
| WebAPI tests for lazy offering details endpoint | `Enrollify.IntegrationTests` | Phase 1 backend |
| Application tests for aggregate stats query handler | `Enrollify.IntegrationTests` | Phase 1 backend |
| Application tests for bulk operation command handlers | `Enrollify.IntegrationTests` | Phase 1 backend |
| Application tests for offering details query | `Enrollify.IntegrationTests` | Phase 1 backend |

### Dependency Graph

```
Phase 1 (Scaffold)
    │
    ├──► Phase 2 (Card View) ──► Phase 4 (Batch Ops) ──► Phase 6 (Polish)
    │                                                         │
    └──► Phase 3 (Table View) ──► Phase 5 (Conflict) ────────┘
                                                              │
                                                              ▼
                                                      Phase 7 (Tests)
```

Phases 2 and 3 can be built in parallel after Phase 1. Phases 4 and 5 can be partially parallel once their dependencies are done.

---

## ASCII Wireframe: Final Page Mockup (Card View)

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  Curriculum & Scheduling  >  Class Sections  [Academic Year: 2025-2026 ▾]             │
├──────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                       │
│  [📋 Draft: 24]  [📂 Open: 156]  [❌ Cancelled: 8]                                    │
│  [⚠️ Unresolved Errors: 12]  [🔴 Conflicts: 7]  [🕐 Unscheduled: 31]                 │
│                                                                                       │
├──────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                       │
│  [● Card View]  [○ Table View]    │    Quick Filters: [All] [Draft] [Open] [Cancelled] │
│                          [Unresolved Errors] [Conflicts] [Needs Attention]           │
│                                                                                       │
├──────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                       │
│  ▸ College of Engineering ────────────────────────────────────────────────────── ▼   │
│                                                                                       │
│    ○ BSCS — Computer Science (4)                        [████████░░] 75% scheduled    │
│    ┌────────────────────────────────────────────────────────────────────────────┐     │
│    │ ☑ 3A  BSCS 3A                             █ Draft    ⚠️1/❌0            │     │
│    │    BSCS — Computer Science · 1st Sem AY 2025-26                          │     │
│    │    Adviser: Dr. Smith                                                     │     │
│    │    Scheduling: [████████░░] 2 of 3 offerings scheduled                    │     │
│    │    Issues: 1 offering missing room · 1 conflict detected                  │     │
│    │    Rooms: 101 (MWF), 102 (TTh)  [missing for 1]                          │     │
│    │    Schedule: MWF 08:00-09:00, TTh 10:00-11:30, ? 1 offering              │     │
│    │    [Open for Enrollment]  [Cancel]  [View Details →]  ⓘ Hover for schedule │     │
│    │                                                                           │     │
│    │ ☐ (disabled) 3B  BSCS 3B                    █ Open     ✓0/✓0              │     │
│    │    BSCS — Computer Science · 1st Sem AY 2025-26                          │     │
│    │    Adviser: Dr. Jones                                                     │     │
│    │    Scheduling: [██████████] 3 of 3 offerings scheduled (100%)             │     │
│    │    Teachers: Smith (DS), Jones (MATH), Lee (ENGL)                         │     │
│    │    Rooms: 101 (MWF), 102 (TTh), 201 (Wed)                                 │     │
│    │    Schedule: MWF 08:00-09:00, TTh 10:00-11:30, Wed 14:00-15:00           │     │
│    │    [Cancel]  [View Details →]  ⓘ Hover for schedule                     │     │
│    │                                                                           │     │
│    │ ☐ (disabled) 3C  BSCS 3C                    █ Open     ✓0/✓0              │     │
│    │    ...                                                                    │     │
│    │    [Cancel]  [View Details →]                                            │     │
│    │                                                                           │     │
│    │ ☑ 2A  BSIT 2A                             █ Draft    ⚠️2/❌1            │     │
│    │    BSIT — Information Tech · 1st Sem AY 2025-26                           │     │
│    │    Adviser: (unassigned)                                                  │     │
│    │    Scheduling: [██░░░░░░░░] 1 of 3 offerings scheduled (33%)              │     │
│    │    Issues: 2 offerings missing teacher · 1 missing room · 1 conflict      │     │
│    │    [Open for Enrollment]  [Cancel]  [View Details →]                     │     │
│    │                                                                           │     │
│    └────────────────────────────────────────────────────────────────────────────┘     │
│                                                                                       │
│    [☐ Select All]  [Assign Adviser ▾]  [Open for Enrollment]  [Cancel]              │
│                                                                                       │
│  ▸ College of Business ──────────────────────────────────────────────────────── ▼    │
│  ▸ College of Arts & Sciences ────────────────────────────────────────────────── ▼   │
│                                                                                       │
├──────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                       │
│  ☑ 3 Draft sections selected  ──────────────────────────────────────────── [Clear]   │
│  [Assign Adviser ▾]  [Open for Enrollment]  [Cancel]  [Bulk Edit ▾]                 │
│                                                                                       │
│  ⓘ Batch operations only available for Draft sections                               │
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
│  [📋 Draft: 24]  [📂 Open: 156]  [❌ Cancelled: 8]                                         │
│  [⚠️ Unresolved Errors: 12]  [🔴 Conflicts: 7]  [🕐 Unscheduled: 31]                      │
│                                                                                            │
├───────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                            │
│  [○ Card View]  [● Table View]  │ Quick Filters: [All] [Draft] [Open] [Cancelled]       │
│                                              [Unresolved Errors] [Conflicts]             │
│                                                                                            │
├───────────────────────────────────────────────────────────────────────────────────────────┤
│ ☐ │ Code │ Section      │ Course   │ Status  │Progress│Errors/CF│ Adviser  │ Actions    │
├───┼──────┼──────────────┼──────────┼─────────┼────────┼─────────┼──────────┼────────────┤
│ ☑ │ 3A   │ BSCS 3A      │ Comp Sci │ Draft   │ ████ 67% │ ⚠️1/❌0│ Dr.Smith │ [O][C][▸]  │
│   │      │ (1st Sem)    │ (BSCS)   │         │ (2/3)   │         │          │ (has error)│
├───┼──────┼──────────────┼──────────┼─────────┼────────┼─────────┼──────────┼────────────┤
│ ☐ │ 3B   │ BSCS 3B      │ Comp Sci │ Open    │ ██████100%│ ✓0/✓0 │ Dr.Jones │ [C][▸]      │
│   │      │ (1st Sem)    │ (BSCS)   │ (read)  │ (3/3)   │         │          │ (no issues)│
├───┼──────┼──────────────┼──────────┼─────────┼────────┼─────────┼──────────┼────────────┤
│ ☐ │ 3C   │ BSCS 3C      │ Comp Sci │ Open    │ ██████100%│ ✓0/✓0 │ Dr.Lee   │ [C][▸]      │
│   │      │ (1st Sem)    │ (BSCS)   │ (read)  │ (3/3)   │         │          │ (no issues)│
├───┼──────┼──────────────┼──────────┼─────────┼────────┼─────────┼──────────┼────────────┤
│ ☑ │ 2A   │ BSIT 2A      │ Info Tech│ Draft   │ ██░░ 33%  │ ⚠️2/❌1│ (none)   │ [O][C][▸]  │
│   │      │ (1st Sem)    │ (BSIT)   │         │ (1/3)   │         │          │ (has error)│
├───┼──────┼──────────────┼──────────┼─────────┼────────┼─────────┼──────────┼────────────┤
│ ☑ │ 2B   │ BSIT 2B      │ Info Tech│ Draft   │ ░░░░░░ 0%│ ⚠️3/❌0│ Dr.Jones │ [O][C][▸]  │
│   │      │ (1st Sem)    │ (BSIT)   │         │ (0/3)   │         │          │ (has error)│
├───┴──────┴──────────────┴──────────┴─────────┴────────┴─────────┴──────────┴────────────┤
│                                                                                            │
│  Legend:                                                                                  │
│  ⚠️1/❌0 = 1 unresolved error, 0 conflicts    [O] = Open for Enrollment (Draft only)    │
│  ✓ = No issues                                [C] = Cancel (Draft and Open)              │
│  (read) = Read-only (non-Draft status)       [▸] = View Details                        │
│                                                                                            │
├───────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                            │
│  ☑ 3 Draft sections selected  ──────────────────────────────────────────────── [Clear]   │
│  [Assign Adviser ▾]  [Open for Enrollment]  [Cancel]  [Bulk Edit ▾]                     │
│                                                                                            │
│  ⓘ Batch operations only available for Draft sections                                   │
│                                                                                            │
├───────────────────────────────────────────────────────────────────────────────────────────┤
│  Page 1 of 13  ◀ 1 2 3 ... 13 ▶  Per page: [25 ▾]                                        │
└───────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## Key Metrics to Surface

**All metrics are derived from the `validationSummary` object in the paginated response or the aggregate stats endpoint.** The backend pre-computes validation errors and conflict counts — the frontend reads these without traversing offering data.

| Metric | Where | Source |
|---|---|---|
| **Scheduling progress %** | Card progress bar, table column | `validationSummary.totalOfferings` vs backend's scheduled count |
| **Unresolved Errors count** | Card badge, table column | `validationSummary.offeringsWithErrors` |
| **Missing teacher count** | Card "Issues" row | `validationSummary.missingTeacherCount` |
| **Missing room count** | Card "Issues" row | `validationSummary.missingRoomCount` |
| **Missing schedule count** | Card "Issues" row | `validationSummary.missingScheduleCount` |
| **Conflicts count** | Card badge, table column | `validationSummary.offeringsWithConflicts` |
| **Unscheduled count** | Stats bar | Aggregate stats endpoint: `unscheduledCount` |
| **Schedule density** | Mini weekly calendar tooltip | Offering details endpoint (lazy, async) |

**Data Flow:**
```
Paginated API Response (validationSummary per section)
       +
Separate Stats API (full-dataset aggregate counts)
    ↓
Frontend reads pre-computed values directly
    ↓
Display on card, table, stats bar
    ↓ (on user interaction — hover, expand, click badge)
Offering Details API (lazy fetch per sectionId)
    ↓
Mini calendar grid, conflict preview drawer
```

---

## Backend Changes Needed

### Existing Endpoint Enhancement

**`GET /api/class-sections/filter/{page}/{pageSize}`** — Extend response to include validation errors and conflicts per offering:

```csharp
// CURRENT: Returns PagedResult<ClassSection>
// PROPOSED: Extend ClassSection to include:

new response structure {
  items: ClassSection[], // with extended offerings
  page, pageSize, totalCount, totalPages,
  
  stats: {
    totalDraft,
    totalOpen,
    totalCancelled,
    sectionsWithUnresolvedErrors,    // count with ≥1 offering missing teacher/room/schedule
    sectionsWithConflicts,            // count with ≥1 offering having conflicts
    unscheduledCount
  }
}

// Inside ClassSection:
offerings: Offering[] {
  id, classSectionId, subjectId,
  teacher?, room?,
  schedules: ClassSchedule[],
  
  // NEW FIELD: Validation messages (per-offering messages only)
  // ValidationMessageDto array for this specific offering
  validationMessages: ValidationMessageDto[] {
    Severity: "Error" | "Warning" | "Info" (DomainValidationErrorSeverityEnum),
    Code: string (e.g., "MISSING_TEACHER", "MISSING_ROOM", "NO_SCHEDULE"),
    Message: string (human-readable validation message),
    ComputedAt: ISO 8601 timestamp (when validation was computed)
  }
  
  // NEW FIELD: Conflicts with other offerings
  conflicts: ConflictResult[] {
    id, type, severity, message,
    conflictingOfferingId,
    conflictingSection: { id, code, name, course },
    conflictingTeacher: { id, firstName, lastName },
    conflictingRoom: { id, roomNumber },
    day, startTime, endTime
  }
}
```

### New Endpoints

| Endpoint | Method | Purpose |
|---|---|---|
| `POST /api/class-sections/bulk/assign-adviser` | POST | Bulk assign adviser to Draft sections |
| `POST /api/class-sections/bulk/open` | POST | Bulk transition Draft sections to Open |
| `POST /api/class-sections/bulk/cancel` | POST | Bulk transition Draft or Open sections to Cancelled (soft-delete cascade) |

**Note:** No export endpoint needed; data analysis handled in Room Scheduler page.

---

## 7. Performance Considerations

This section addresses frontend and backend optimization strategies for handling large datasets of sections, offerings, and validation data.

### Frontend Performance

**Challenge:** With 200+ rooms across multiple colleges and 1000+ offerings with validation data, rendering all sections + cards + mini calendars can be slow.

**Mitigation Strategies:**

| Concern | Solution |
|---|---|
| **Rendering performance with many cards** | Virtualize only College accordion headers as top-level rows (TanStack Virtual). Inner course blocks use `content-visibility: auto` — no full card-level virtualization needed since nested accordion makes it impractical |
| **Metrics calculation overhead** | Metrics come from `validationSummary` (backend-computed, already in paginated response). Frontend reads via `useMemo` with `validationSummary` as dependency — no offering traversal |
| **Mini calendar rendering** | **Two-phase approach:** Phase 1 (instant) shows static summary from `validationSummary`. Phase 2 (async) fetches offering details and renders full grid on hover. Data cached per `sectionId` after first fetch |
| **Sorting/filtering slowness** | Debounce filter changes (300ms); perform sorting in backend if possible; use shallow filtering (status only) for quick responses |
| **Large college/course groups** | Collapse course groups by default; expand on demand; `content-visibility: auto` on course blocks for offscreen skipping |
| **Bundle size** | Tree-shake unused charting libraries; lazy-load mini calendar component |

**Frontend Caching:**
- Cache paginated response in TanStack Query with 2-minute stale time
- Cache offering details per `sectionId` (TanStack Query, separate from list)
- Cache aggregate stats with background refetch after mutations
- Use background refetch after mutations (optimistic updates)

### Backend Performance

**Challenge:** Calculating validation errors and conflicts for every offering on every request is expensive.

**Mitigation Strategies:**

| Concern | Solution |
|---|---|
| **Computing validation errors at query time** | Pre-compute during seeding; cache in database or calculated field; compute only if offerings changed |
| **Fetching all offerings for all sections** | Use efficient database query with `SELECT only needed fields`; index by `classSectionId` |
| **Conflict detection expensive** | Cache conflict results per offering; recompute on schedule changes only; use background job (Hangfire) for batch conflict detection |
| **Paginated response size** | Return only `validationSummary` (aggregate counts) per section — no full offering data. Full offering details fetched on demand via separate lazy endpoint |

**Database Indexing:**
- Index on `ClassSection.AcademicYearId` for filtering
- Index on `Offering.ClassSectionId` and `Offering.ConflictCount` for quick lookups
- Index on `Schedule.DayOfWeek, Schedule.StartTime` for conflict detection

### API Response Optimization

**Payload size:** The paginated response now includes only `validationSummary` per section (no full offerings). Full offering details with validation messages + conflicts are fetched on demand via a separate lazy endpoint.

**Mitigation:**
- Offering details are **not embedded** in the paginated response at all — fetched lazily via `GET /api/class-sections/{sectionId}/offerings`
- Aggregate stats come from a **separate endpoint** (`GET /api/class-sections/stats`), not embedded in the paginated response
- Compress JSON response (gzip)
- Mock conflict data for development (faker.js); real computation on production

### Recommendations

1. **Start with:** Pagination + lazy-loaded mini calendars + memoized metrics
2. **Monitor:** Frontend render time with React DevTools Profiler
3. **Optimize if slow:** Add virtual scrolling, background workers for conflict detection
4. **Test:** Load test with 500+ sections to identify bottlenecks

---

## 8. API Response Structure

### Overview of Three API Sources

| Endpoint | Purpose | Called When |
|---|---|---|
| `GET /api/class-sections/filter/{page}/{pageSize}` | Paginated section list with `validationSummary` per section | On page load, filter/sort/paginate changes |
| `GET /api/class-sections/stats?academicYearId={id}` | Full-dataset aggregate stats (always total, regardless of filters) | On page load and after mutations |
| `GET /api/class-sections/{sectionId}/offerings` | Full offering details with validation messages + conflicts | On user interaction (hover, expand, badge click) |

### 8.1 Paginated Section List

**Current Response (unchanged structure):**
```json
{
  "items": [ClassSection],
  "page": 1,
  "pageSize": 25,
  "totalCount": 312,
  "totalPages": 13
}
```

**Extended — `validationSummary` added per section:**
```json
{
  "items": [
    {
      "id": 1,
      "name": "BSCS 3A",
      "sectionCode": "3A",
      "status": { "value": 1, "name": "Draft" },
      "course": { "id": 1, "code": "BSCS", "name": "Computer Science" },
      "adviser": { "id": 5, "firstName": "Dr.", "lastName": "Smith" },
      "intendedYearLevel": 3,
      "academicTerm": { "id": 2, "termName": "1st Semester" },
      
      // Offerings returned as SUMMARY only (no validation/conflict arrays)
      "offerings": [
        {
          "id": 101,
          "snapshotSubjectCode": "DS",
          "snapshotSubjectTitle": "Data Structures",
          "teacher": { "id": 5, "firstName": "Dr.", "lastName": "Smith" },
          "room": { "roomNumber": "101" },
          "scheduleSummary": "MWF 08:00-09:30"
        }
      ],
      
      // COMPUTED: Aggregate stats for this section (backend-computed)
      "validationSummary": {
        "totalOfferings": 2,
        "offeringsWithErrors": 1,
        "offeringsWithConflicts": 1,
        "missingTeacherCount": 1,
        "missingRoomCount": 0,
        "missingScheduleCount": 1
      }
    }
  ],
  
  "page": 1,
  "pageSize": 25,
  "totalCount": 312,
  "totalPages": 13
}
```

**Note:** The `stats` object is NOT included here. Full-dataset stats come from the separate stats endpoint.

### 8.2 Aggregate Stats Endpoint

**`GET /api/class-sections/stats?academicYearId={id}`**

```json
{
  "totalDraft": 24,
  "totalOpen": 156,
  "totalCancelled": 8,
  "sectionsWithUnresolvedErrors": 12,
  "sectionsWithConflicts": 7,
  "unscheduledCount": 31
}
```

- Full-dataset numbers — **not affected by filters or pagination**
- Refetched after any status transition mutation

### 8.3 Offering Details Endpoint (Lazy Load)

**`GET /api/class-sections/{sectionId}/offerings`**

```json
{
  "offerings": [
    {
      "id": 101,
      "classSectionId": 1,
      "subjectId": 10,
      "snapshotSubjectCode": "DS",
      "snapshotSubjectTitle": "Data Structures",
      "teacher": { "id": 5, "firstName": "Dr.", "lastName": "Smith" },
      "room": { "id": 101, "roomNumber": "101", "building": { "name": "Main" } },
      "daysPerWeek": 2,
      "hoursPerDay": 1.5,
      "schedules": [
        { "id": 201, "dayOfWeek": "MON", "startTime": "08:00", "endTime": "09:30" }
      ],
      
      "validationMessages": [
        {
          "Severity": "Warning",
          "Code": "POTENTIAL_CONFLICT",
          "Message": "Offering overlaps with another section in the same room",
          "ComputedAt": "2025-06-10T15:30:00Z"
        }
      ],
      
      "conflicts": [
        {
          "id": "conflict-001",
          "type": "ROOM_DOUBLE_BOOKED",
          "severity": "Error",
          "message": "Room 101 double-booked on Monday 08:00-09:30",
          "day": "MON",
          "startTime": "08:00",
          "endTime": "09:30",
          "conflictingOfferingId": 202,
          "conflictingOffering": {
            "id": 202,
            "sectionCode": "2A",
            "subjectCode": "ENGL",
            "subjectTitle": "English 101",
            "teacher": { "id": 7, "firstName": "Dr.", "lastName": "Jones" },
            "room": { "id": 101, "roomNumber": "101", "building": { "name": "Main" } }
          }
        }
      ]
    }
  ],
  
  "validationSummary": {
    "totalOfferings": 1,
    "offeringsWithErrors": 0,
    "offeringsWithConflicts": 1,
    "missingTeacherCount": 0,
    "missingRoomCount": 0,
    "missingScheduleCount": 0
  }
}
```

- Called on user interaction (hover, expand card, click error/conflict badge)
- Cached per `sectionId` after first fetch
- Conflicts are only computed for **Draft sections** (Open sections are finalized — no conflict detection)

### Validation Message Codes Reference

Each offering includes a **flat `validationMessages` array** containing validation errors and warnings. The backend validation system uses standardized message codes:

| Code | Severity | Category | UI Display |
|---|---|---|---|
| `MISSING_TEACHER` | Error | Unresolved Error | "Offering missing teacher" |
| `MISSING_ROOM` | Error | Unresolved Error | "Offering missing room" |
| `NO_SCHEDULE` | Error | Unresolved Error | "Offering has no schedule" |
| `POTENTIAL_CONFLICT` | Warning | Validation | Per message text |
| Other codes | Warning / Info | Validation | Per message text |

**Frontend Metric Calculation:**
- **Unresolved Errors count** = Count all validation messages where `Severity === "Error"` across all offerings
- **Missing teacher/room/schedule detection** = Check for specific codes (MISSING_TEACHER, MISSING_ROOM, NO_SCHEDULE) in validationMessages array
- **Conflicts count** = Count all offerings with `conflicts` array length > 0

### Implementation Notes for API Changes

**Phase 1: Extend FilterClassSectionsPaginatedQuery Handler**
- The `filterClassSectionsPaginatedOptions` endpoint currently returns `PagedResult<ClassSectionDto>`
- Add `validationSummary` per section (computed aggregate — NOT full offering data)
- Keep offering summaries only (id, subject code, teacher name, room number, schedule pattern text)
- **Do NOT** embed `validationMessages[]` or `conflicts[]` in the paginated response

**Phase 2: Create Separate Aggregate Stats Endpoint**
- New handler: `GetClassSectionStatsQuery`
- Returns full-dataset counts: `totalDraft`, `totalOpen`, `totalCancelled`, `sectionsWithUnresolvedErrors`, `sectionsWithConflicts`, `unscheduledCount`
- Always full-dataset — not affected by filter/pagination parameters

**Phase 3: Create Separate Offering Details Endpoint**
- New handler: `GetClassSectionOfferingsQuery`
- Returns full offering data with `validationMessages[]` and `conflicts[]`
- Conflicts computed only for **Draft sections** (Open sections are finalized)
- Cached on the frontend per `sectionId`

**Phase 4: Update Frontend Parsing**
- Parse `validationSummary` from paginated response for list/metric display
- Fetch offering details on demand for mini calendar and conflict preview
- Count unresolved errors by filtering messages where `Severity === "Error"`
- Detect missing items by checking for specific codes ("MISSING_TEACHER", "MISSING_ROOM", "NO_SCHEDULE")
- **No additional client-side validation logic needed**—rely on backend validation computation

---

## Risk & Mitigation

| Risk | Mitigation |
|---|---|---|
| Large API response size (validation + conflicts) | Offering details are NOT embedded in paginated response — fetched lazily via separate endpoint |
| Performance with 300+ sections expanded | `content-visibility: auto` on course blocks; virtualize only college headers; lazy-render mini calendars |
| Metrics calculation overhead on frontend | Metrics served as `validationSummary` (backend-computed, pre-cached). No offering traversal on frontend |
| BULK actions on stale data | Optimistic UI + background refetch; validate before transitions via backend |
| Cognitive overload (too much info) | Progressive disclosure (collapse course groups by default) |
| Conflicting offerings missing from response | Offering details endpoint returns full conflict data; verify in integration tests |
| User selects mixed-status sections for batch | Show warning; disable batch buttons if non-Draft selected |
| Mini calendar blank on hover while data loads | Two-phase load: static summary immediately, full grid async with loading indicator |

---

## Important Notes

### Desktop-Only Design
**This page is designed exclusively for desktop and laptop screens (1024px and above).** Mobile and tablet views are not supported. Users on smaller devices should use the Room Scheduler page or Section Detail pages, which are optimized for various screen sizes.

### Scheduling Workflow Scope
This page manages the **Scheduling Workflow** only:
- **Draft → Open:** Admin initiates enrollment period
- **Draft → Cancelled:** Admin cancels sections before opening
- **Open → Cancelled:** Admin cancels sections after opening
- All other transitions (Open → Locked, Locked → Active, Active → Completed) are automated and managed outside this page per the Class Section Lifecycle.

### Validation vs. Conflicts
**Unresolved Errors (Validation):**
- Per-offering scope
- Missing teacher, room, or schedule
- Prevents "Open for Enrollment" action

**Conflicts:**
- Cross-section scope
- Scheduling conflicts with other sections' offerings
- ALSO prevents "Open for Enrollment" — both errors and conflicts block transitioning Draft → Open
- Resolved on Room Scheduler page or Section Detail page

---

## Alignment with Pending User Stories

This redesign addresses the following pending user stories from `docs/scheduling-architecture/pending/`:

| Story | Addressed By |
|---|---|
| **admin-conflict-dashboard** | Conflicts badge, stats bar, mini calendar hover |
| **per-teacher-conflict-view** | Section Detail page (out of scope for management page) |
| **per-room-conflict-view** | Room Scheduler page (separate design doc) |
| Validation errors (missing teacher/room/schedule) | Unresolved Errors badge, card issues row, stats bar |

---
