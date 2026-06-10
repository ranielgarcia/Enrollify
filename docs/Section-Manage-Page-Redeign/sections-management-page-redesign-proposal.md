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
- **Conflicts (X)** — Count of Draft/Open/Cancelled sections with ≥1 offering that has scheduling conflicts with other sections' offerings
- **Unscheduled (X)** — Count of Draft/Open/Cancelled sections with 0 complete offerings (offerings lacking teacher+room+schedule)

**Note:** Locked, Active, and Completed statuses are excluded from stats bar as they are managed outside the scheduling workflow.

- Each stat is a button that adds/removes a filter for that status
- Colors match `SectionStatusBadge` conventions (Draft=secondary, Open=green, Cancelled=red)

**Data source:** Derived from the paginated response's summary metadata; frontend calculates progress and validation status.

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
- "Batch Select" checkbox (applies only to **Draft sections** in this course)

**Status & Actions Column:**

| Status | Actions | Notes |
|---|---|---|
| **Draft** | `[Open]` `[Cancel]` `[▸ Details]` | Fully editable; actions available for batch operations |
| **Open** | `[▸ Details]` | Read-only; no status transitions available on this page |
| **Locked** | `[▸ Details]` | Read-only (managed by enrollment deadline automation) |
| **Active** | `[▸ Details]` | Read-only (managed by term start automation) |
| **Completed** | `[▸ Details]` | Read-only (managed by term end automation) |
| **Cancelled** | `[▸ Details]` | Read-only; no further actions |

**Errors/Conflicts Column Display:**
- Format: `⚠️ X / ❌ Y` where:
  - `⚠️ X` = Unresolved errors count (missing teacher, room, or schedule across offerings)
  - `❌ Y` = Conflicts count (scheduling conflicts with other sections)
- Click to see details in drawer (see Section 4.9)
- Red left border on row if errors OR conflicts exist

---

### 4.3 Section Card

Each section renders as a compact card (used in **card view**). This replaces a flat table row with richer visuals.

```
Draft Section (Editable):
┌─────────────────────────────────────────────────────────────────┐
│ ☐ 3A                                         █ Draft  ⚠️1 ❌0   │
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

Open/Locked/Active/Completed Section (Read-Only):
┌─────────────────────────────────────────────────────────────────┐
│ ☐ 3A                                         █ Open   ✓0 ✓0    │
│ BSCS — Computer Science · 1st Semester AY 2025-2026             │
│ ─────────────────────────────────────────────────────────────── │
│ Adviser:     Dr. Smith                                          │
│ Scheduling:  [██████████]  3 of 3 offerings scheduled (100%)    │
│ Teachers:    Smith (DS), Jones (MATH), Lee (ENGL)              │
│ Rooms:       101 (MWF), 102 (TTh), 103 (Wed)                   │
│ Schedule:    MWF 08:00-09:00, TTh 10:00-11:30, Wed 14:00-15:00│
│ ─────────────────────────────────────────────────────────────── │
│ [View Details →]                                                │
│                                                                 │
│ ⓘ Hover to see weekly schedule grid                            │
└─────────────────────────────────────────────────────────────────┘
```

**Card sections:**

| Zone                    | Content                                                             |
| ----------------------- | ------------------------------------------------------------------- |
| Header checkbox         | For batch selection (Draft sections only)                           |
| Section code (`3A`)     | Bold, primary identifier                                            |
| Status badge            | Colored per `SectionStatusBadge`                                    |
| Error/Conflict badge    | `⚠️X` (unresolved errors) `❌Y` (conflicts); red if either > 0      |
| Subtitle                | Course name · Term                                                  |
| Adviser row             | Current adviser; change button only for Draft                       |
| Scheduling progress bar | Visual + fraction (e.g. "2 of 3 offerings" or "100%")              |
| Issues row              | Summary of missing teacher/room/schedule issues (Draft only)        |
| Room summary            | Compact room assignment per schedule pattern                        |
| Schedule summary        | Compact text showing day/time patterns; gaps indicated              |
| Action buttons          | **Draft:** [Open], [Cancel], [Details]; **Others:** [Details] only |
| Detail link             | Navigate to section detail page for full editing                   |

**Hover behaviour:** The card reveals a **mini weekly calendar** tooltip (see Section 4.8) showing the section's schedule grid plus conflicting offerings from other sections. This lets the scheduler visually check for overlaps without navigating.

**Visual Indicators:**
- **Red left border** if section has ≥1 unresolved error OR ≥1 conflict
- **Disabled state** for action buttons on non-Draft sections

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
│ 3B   │ BSCS 3B  │ Computer │ █ Open  │ 100% │ ✓0/✓0   │ Dr.Jones │ [▸]      │
│      │          │ Science  │ (read)  │ ████ │          │          │          │
│ 3C   │ BSCS 3C  │ Computer │ █ Open  │ 100% │ ✓0/✓0   │ Dr.Lee   │ [▸]      │
│      │          │ Science  │ (read)  │ ████ │          │          │          │
└──────┴──────────┴──────────┴─────────┴──────┴──────────┴──────────┴──────────┘

Legend:
  [O] = Open for Enrollment (Draft only)
  [C] = Cancel (Draft only)
  [▸] = View Details
  ⚠️1/❌0 = 1 unresolved error, 0 conflicts
```

**New/enhanced columns:**

| Column            | Description                                                             |
| ----------------- | ----------------------------------------------------------------------- |
| **Sched%**        | Visual progress bar + percentage (0-100%); hover for offering breakdown |
| **Errors/Conflicts** | Format: `⚠️X/❌Y` where X=unresolved errors, Y=conflicts; click for details |
| **Actions**       | Status buttons (Draft: [Open][Cancel], Others: [Details]); always visible |
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

**Behavior:**
- Appears as a floating bar (sticky at bottom of viewport or below header)
- **Only Draft sections can be batch-operated**
- If non-Draft sections are selected, show tooltip: "Batch operations only available for Draft sections"
- Each action opens a bulk-action drawer similar to `BulkInitializeSectionsDrawer`
- Adviser assignment opens `SearchTeachersDialog` with multi-select
- Status transitions show a confirmation with count of affected sections

**Available Batch Actions (Draft only):**
| Action | Purpose |
|---|---|
| **Assign Adviser** | Bulk assign same adviser to multiple Draft sections |
| **Open for Enrollment** | Transition selected Draft sections to Open status |
| **Cancel** | Cancel selected Draft sections (confirm with count) |
| **Bulk Edit** | Edit capacity, term, or year level across multiple sections |

**Constraint:** Cannot batch-operate on mixed statuses. Show warning if user selects both Draft and non-Draft sections.

---

### 4.7 Inline Status Transitions

**This page only supports transitions for Draft sections.** All other statuses are managed outside the scheduling workflow and are read-only.

```
[Draft]      → [Open for Enrollment]  [Cancel]  [View Details]
[Open]       → [View Details] (read-only)
[Locked]     → [View Details] (read-only, managed by enrollment deadline)
[Active]     → [View Details] (read-only, managed by term start)
[Completed]  → [View Details] (read-only, managed by term end)
[Cancelled]  → [View Details] (read-only)
```

**Class Section Lifecycle Context:**
Per `docs/design-decisions/class-section-lifecycle.md`:
- **Draft → Open:** Admin opens enrollment period (happens on this page)
- **Draft → Cancelled:** Admin cancels before opening (happens on this page)
- **Open → Locked:** Automatic when enrollment deadline passes (not on this page)
- **Locked → Active:** Automatic when term begins (not on this page)
- **Active → Completed:** Automatic when term ends (not on this page)

**Implementation Notes:**
- Use the existing transition mutations: `openClassSectionOptions(sectionId)`, `cancelClassSectionOptions(sectionId)`
- Show confirmation alert dialog (reuse `CancelSectionAlertDialog` pattern) before transitioning
- After successful transition, refetch section data and show toast notification
- Disable buttons for non-Draft sections (gray out visually)

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
The hover tooltip receives **conflicting offerings data from the API response**. For each offering with conflicts, the API response includes:
- `conflictingOfferingId`, `conflictingSection`, `conflictingTeacher`, `conflictingRoom`
- Time range of overlap
- Conflict severity

**Interaction:**
- Click "View Full Schedule" → navigates to section detail page's "Weekly Grid" tab
- Click conflicting offering box → navigates to conflicting section's detail page
- Tooltip auto-closes when mouse leaves card

**Performance Note:**
- Mini calendar is **lazy-rendered** on hover (not pre-rendered for all cards)
- Calendar data comes from the paginated API response (already fetched)
- No additional API call needed; reuse offerings data with conflict info

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
│  [📋 Draft: 24]  [📂 Open: 156]  [❌ Cancelled: 8]                                    │
│  [⚠️ Unresolved Errors: 12]  [🔴 Conflicts: 7]  [🕐 Unscheduled: 31]                 │
│                                                                                       │
├──────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                       │
│  [● Card View]  [○ Table View]    │    Quick Filters: [All] [Draft] [Open] [Cancel] │
│                          [Unresolved Errors] [Conflicts] [Needs Attention]           │
│                                                                                       │
├──────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                       │
│  ▸ College of Engineering ────────────────────────────────────────────────────── ▼   │
│                                                                                       │
│    ○ BSCS — Computer Science (4)                        [████████░░] 75% scheduled    │
│    ┌────────────────────────────────────────────────────────────────────────────┐     │
│    │ ☑ 3A  BSCS 3A                             █ Draft    ⚠️ 1 ❌ 0            │     │
│    │    BSCS — Computer Science · 1st Sem AY 2025-26                          │     │
│    │    Adviser: Dr. Smith                                                     │     │
│    │    Scheduling: [████████░░] 2 of 3 offerings scheduled                    │     │
│    │    Issues: 1 offering missing room · 1 conflict detected                  │     │
│    │    Rooms: 101 (MWF), 102 (TTh)  [missing for 1]                          │     │
│    │    Schedule: MWF 08:00-09:00, TTh 10:00-11:30, ? 1 offering              │     │
│    │    [Open for Enrollment]  [Cancel]  [View Details →]  ⓘ Hover for schedule │     │
│    │                                                                           │     │
│    │ ☐ 3B  BSCS 3B                             █ Open     ✓ 0 ✓ 0             │     │
│    │    BSCS — Computer Science · 1st Sem AY 2025-26                          │     │
│    │    Adviser: Dr. Jones                                                     │     │
│    │    Scheduling: [██████████] 3 of 3 offerings scheduled (100%)             │     │
│    │    Teachers: Smith (DS), Jones (MATH), Lee (ENGL)                         │     │
│    │    Rooms: 101 (MWF), 102 (TTh), 201 (Wed)                                 │     │
│    │    Schedule: MWF 08:00-09:00, TTh 10:00-11:30, Wed 14:00-15:00           │     │
│    │    [View Details →]  ⓘ Hover for schedule                                │     │
│    │                                                                           │     │
│    │ ☐ 3C  BSCS 3C                             █ Open     ✓ 0 ✓ 0             │     │
│    │    ...                                                                    │     │
│    │    [View Details →]                                                       │     │
│    │                                                                           │     │
│    │ ☑ 2A  BSIT 2A                             █ Draft    ⚠️ 2 ❌ 1            │     │
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
│ ☐ │ 3B   │ BSCS 3B      │ Comp Sci │ Open    │ ██████100%│ ✓0/✓0 │ Dr.Jones │ [▸]        │
│   │      │ (1st Sem)    │ (BSCS)   │ (read)  │ (3/3)   │         │          │ (no issues)│
├───┼──────┼──────────────┼──────────┼─────────┼────────┼─────────┼──────────┼────────────┤
│ ☐ │ 3C   │ BSCS 3C      │ Comp Sci │ Open    │ ██████100%│ ✓0/✓0 │ Dr.Lee   │ [▸]        │
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
│  ✓ = No issues                                [C] = Cancel (Draft only)                  │
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

**All calculations are performed on the frontend** using data from the paginated API response.

| Metric | Where | Calculation Formula |
|---|---|---|
| **Scheduling progress %** | Card progress bar, table column | `(count of offerings with teacher + room + ≥1 schedule) / total offerings * 100` |
| **Unresolved Errors count** | Card badge, stats bar, table "Errors" column | Count all offerings where `validationMessages.some(msg => msg.Severity === "Error")` |
| **Missing teacher count** | Card "Issues" row | Count offerings where `validationMessages.some(msg => msg.Code === "MISSING_TEACHER")` |
| **Missing room count** | Card "Issues" row | Count offerings where `validationMessages.some(msg => msg.Code === "MISSING_ROOM")` |
| **Missing schedule count** | Card "Issues" row | Count offerings where `validationMessages.some(msg => msg.Code === "NO_SCHEDULE")` |
| **Conflicts count** | Card badge, stats bar, table "Conflicts" column | Count of offerings with `conflicts` array length > 0 (from API) |
| **Unscheduled count** | Quick filter, stats bar | Count of sections with `offerings.length === 0` OR all offerings lack complete schedule |
| **Schedule density** | Mini weekly calendar tooltip | Aggregate all offerings' schedules into (day, time slot) map for visual display |

**Data Flow:**
```
API Response (offerings with flat validationMessages[] + conflicts[])
    ↓
Frontend parses offerings per section
    ↓
Calculate metrics (progress %, errors count, missing items count, conflicts count)
    ↓
Display on card, table, stats bar, mini calendar
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
| `POST /api/class-sections/bulk/cancel` | POST | Bulk transition Draft sections to Cancelled |

**Note:** No export endpoint needed; data analysis handled in Room Scheduler page.

---

## 7. Performance Considerations

This section addresses frontend and backend optimization strategies for handling large datasets of sections, offerings, and validation data.

### Frontend Performance

**Challenge:** With 200+ rooms across multiple colleges and 1000+ offerings with validation data, rendering all sections + cards + mini calendars can be slow.

**Mitigation Strategies:**

| Concern | Solution |
|---|---|
| **Rendering performance with many cards** | Use React virtualization library (react-window or TanStack Virtual) to render only visible cards in viewport; lazy-load card details on expand |
| **Metrics calculation overhead** | Memoize metric calculations per section using `useMemo`; avoid recalculating on every render |
| **Mini calendar rendering** | Lazy-render mini calendar ONLY on hover; pre-render only when tooltip becomes visible; reuse offerings data already in memory |
| **Sorting/filtering slowness** | Debounce filter changes (300ms); perform sorting in backend if possible; use shallow filtering (status only) for quick responses |
| **Large college/course groups** | Collapse course groups by default; expand on demand; virtualize rows within expanded groups |
| **Bundle size** | Tree-shake unused charting libraries; lazy-load mini calendar component |

**Frontend Caching:**
- Cache paginated response in TanStack Query with 2-minute stale time
- Use background refetch after mutations (optimistic updates)
- Store calculation results (progress %, errors count) in `useMemo` with dependency on offerings

### Backend Performance

**Challenge:** Calculating validation errors and conflicts for every offering on every request is expensive.

**Mitigation Strategies:**

| Concern | Solution |
|---|---|
| **Computing validation errors at query time** | Pre-compute during seeding; cache in database or calculated field; compute only if offerings changed |
| **Fetching all offerings for all sections** | Use efficient database query with `SELECT only needed fields`; index by `classSectionId` |
| **Conflict detection expensive** | Cache conflict results per offering; recompute on schedule changes only; use background job (Hangfire) for batch conflict detection |
| **Paginated response size** | Include only summary counts in stats; return full offering data (with errors/conflicts) only for current page |

**Database Indexing:**
- Index on `ClassSection.AcademicYearId` for filtering
- Index on `Offering.ClassSectionId` and `Offering.ConflictCount` for quick lookups
- Index on `Schedule.DayOfWeek, Schedule.StartTime` for conflict detection

### API Response Optimization

**Payload size:** The extended response with validation errors + conflicts per offering will be larger (~2-3x current).

**Mitigation:**
- Paginate offering details; return only offering summary for non-current-page sections
- Compress JSON response (gzip)
- Return conflicts only if `includeConflicts=true` query parameter
- Mock conflict data for development (faker.js); real computation on production

### Recommendations

1. **Start with:** Pagination + lazy-loaded mini calendars + memoized metrics
2. **Monitor:** Frontend render time with React DevTools Profiler
3. **Optimize if slow:** Add virtual scrolling, background workers for conflict detection
4. **Test:** Load test with 500+ sections to identify bottlenecks

---

## 8. API Response Structure

### Current vs. Proposed

**Current `filterClassSectionsPaginatedOptions` Response:**
```json
{
  "items": [ClassSection],
  "page": 1,
  "pageSize": 25,
  "totalCount": 312,
  "totalPages": 13
}
```

**Proposed Extended Response:**
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
      
      // NEW: Offerings with validation errors + conflicts
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
          
          // NEW FIELD: Validation messages (per-offering array)
          "validationMessages": [
            {
              "Severity": "Warning",
              "Code": "POTENTIAL_CONFLICT",
              "Message": "Offering overlaps with another section in the same room",
              "ComputedAt": "2025-06-10T15:30:00Z"
            }
          ],
          
          // NEW FIELD: Conflicts with other sections
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
        },
        {
          "id": 102,
          "classSectionId": 1,
          "subjectId": 11,
          "snapshotSubjectCode": "MATH",
          "snapshotSubjectTitle": "Calculus I",
          "teacher": null,  // MISSING TEACHER
          "room": { "id": 102, "roomNumber": "102", "building": { "name": "Main" } },
           "daysPerWeek": 3,
           "hoursPerDay": 1,
           "schedules": [],  // MISSING SCHEDULE
           
           // Validation messages for this offering (array of validation errors)
           "validationMessages": [
             {
               "Severity": "Error",
               "Code": "MISSING_TEACHER",
               "Message": "Offering is missing assigned teacher",
               "ComputedAt": "2025-06-10T15:30:00Z"
             },
             {
               "Severity": "Error",
               "Code": "NO_SCHEDULE",
               "Message": "Offering has no scheduled classes",
               "ComputedAt": "2025-06-10T15:30:00Z"
             }
           ],
           
           "conflicts": []  // No conflicts for unscheduled offering
         }
      ],
      
      // NEW FIELD: Aggregate stats for this section
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
  
  // NEW FIELD: Page-level stats
  "stats": {
    "totalDraft": 24,
    "totalOpen": 156,
    "totalCancelled": 8,
    "sectionsWithUnresolvedErrors": 12,  // Count with ≥1 offering having errors
    "sectionsWithConflicts": 7,           // Count with ≥1 offering having conflicts
    "unscheduledCount": 31                // Sections with 0 offerings or no schedules
  },
  
  "page": 1,
  "pageSize": 25,
  "totalCount": 312,
  "totalPages": 13
}
```

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
- The `filterClassSectionsPaginatedOptions` endpoint (in `FilterClassSectionsPaginatedQuery`) currently returns `PagedResult<ClassSectionDto>` 
- **Add validation messages:** Fetch `ClassSectionEnrollmentEligibilityValidationMessages` for each offering and flatten into a simple array per offering
- **Add conflicts:** Include `ConflictResult[]` per offering (if available; pre-computed or fetched from a separate service)
- **Return structure:** Add flat `validationMessages[]` array and `conflicts[]` array directly on each offering object
- Ensure conflict detection runs pre-query (cached or computed)

**Phase 2: Add Aggregate Stats to Response**
- Calculate stats at response time from the paginated results:
  - `totalDraft`, `totalOpen`, `totalCancelled` = count sections by status
  - `sectionsWithUnresolvedErrors` = count sections with ≥1 offering containing validation messages where Severity="Error"
  - `sectionsWithConflicts` = count sections with ≥1 offering with conflicts
  - `unscheduledCount` = count sections with 0 offerings OR all offerings have 0 schedules
- Include stats object in paginated response

**Phase 3: Update Frontend Parsing**
- Parse `validationMessages[]` array from each offering (flat structure, no nested dictionary)
- Count unresolved errors by filtering messages where `Severity === "Error"`
- Detect missing items by checking for specific codes ("MISSING_TEACHER", "MISSING_ROOM", "NO_SCHEDULE")
- Use conflicts array for conflict badge and mini calendar display
- **No additional client-side validation logic needed**—rely on backend validation computation

---

## Risk & Mitigation

| Risk | Mitigation |
|---|---|
| Large API response size (validation + conflicts) | Paginate; compress; include only summary counts initially |
| Performance with 300+ sections expanded | Lazy-load course groups; virtual scroll; lazy-render mini calendars on hover only |
| Metrics calculation overhead on frontend | Memoize calculations; cache results; avoid recalculation on re-render |
| BULK actions on stale data | Optimistic UI + background refetch; validate before transitions |
| Cognitive overload (too much info) | Progressive disclosure (collapse course groups by default) |
| Conflicting offerings missing from response | API must return conflicting offering details for mini calendar display; verify in integration tests |
| User selects mixed-status sections for batch | Show warning; disable batch buttons if non-Draft selected |

---

## Important Notes

### Desktop-Only Design
**This page is designed exclusively for desktop and laptop screens (1024px and above).** Mobile and tablet views are not supported. Users on smaller devices should use the Room Scheduler page or Section Detail pages, which are optimized for various screen sizes.

### Scheduling Workflow Scope
This page manages the **Scheduling Workflow** only:
- **Draft → Open:** Admin initiates enrollment period
- **Draft → Cancelled:** Admin cancels sections before opening
- All other transitions (Open → Locked, Locked → Active, Active → Completed) are automated and managed outside this page per the Class Section Lifecycle.

### Validation vs. Conflicts
**Unresolved Errors (Validation):**
- Per-offering scope
- Missing teacher, room, or schedule
- Prevents "Open for Enrollment" action

**Conflicts:**
- Global scope (cross-section)
- Scheduling conflicts with other sections' offerings
- Does NOT prevent "Open for Enrollment" (warnings only)
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
