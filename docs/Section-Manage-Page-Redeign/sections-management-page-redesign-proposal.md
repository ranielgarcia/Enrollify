# Class Sections Management Page — Redesign Proposal

> **Audience:** School administrator / Scheduler responsible for scheduling class sections for a specific college.
> **Goal:** Allow the scheduler to select a college and manage all its class sections (courses, sections, schedules), surface conflicts early, enable batch operations (Draft sections only), and reduce navigation overhead.

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
│  [College Selector: Engineering ▾]  [Academic Year: 2025-2026]               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  [📋 Draft: 8]  [📂 Open: 42]  [❌ Cancelled: 2]                            │
│  [⚠️ Unresolved Errors: 3]  [🔴 Conflicts: 2]  [🕐 Unscheduled: 5]          │
│                                                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  [● Card View]  [○ Table View]    │    Quick Filters: [All] [Draft] [Open] │
│                                          [Unresolved Errors] [Conflicts]   │
│                                                                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  College of Engineering                                                     │
│                                                                             │
│  ▸ BSCS — Computer Science  (4 sections)      [████████░░] 75% scheduled    │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ ☑ 3A   BSCS 3A                             █ Draft    ⚠️1/❌0        │  │
│  │    BSCS — Computer Science · 1st Sem AY 2025-26                      │  │
│  │    ...                                                                │  │
│  │    [Open for Enrollment]  [Cancel]  [View Details →]                 │  │
│  │                                                                       │  │
│  │ ☐ 3B   BSCS 3B                             █ Open     ✓0/✓0          │  │
│  │ ☐ 3C   BSCS 3C                             █ Open     ✓0/✓0          │  │
│  │                                                                       │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                                                             │
│  [☐ Select All]  [Assign Adviser ▾]  [Open for Enrollment]  [Cancel]       │
│                                                                             │
│  ▸ BSIT — Information Technology  (3 sections)  [██░░░░░░░░] 45%           │
│  ...                                                                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Component Details & Wireframes

### 4.1 Stats Bar

Displays aggregate counts **for the selected college** (Draft, Open, and Cancelled statuses only). Each stat is clickable to filter the list.

```
┌───────────────────────────────────────────────────────────────────────────────┐
│  [📋 Draft 8]  [📂 Open 42]  [❌ Cancelled 2]                                 │
│  [⚠️ Unresolved Errors 3]  [🔴 Conflicts 2]  [🕐 Unscheduled 5]               │
└───────────────────────────────────────────────────────────────────────────────┘
```

**Stat Definitions:**
- **Draft (X)** — Sections in Draft status (newly created, awaiting scheduling) **in the selected college**
- **Open (X)** — Sections opened for enrollment (scheduling complete) **in the selected college**
- **Cancelled (X)** — Sections that were cancelled before/after opening **in the selected college**
- **Unresolved Errors (X)** — Count of Draft sections whose enrollment-eligibility validation messages include at least one **Error-severity** item (e.g., adviser missing, offerings missing, teacher/room missing, insufficient/invalid schedules) **in the selected college**
- **Conflicts (X)** — Count of Draft sections **in the selected college** that have ≥1 offering with at least one **Error-severity** scheduling conflict with other sections' offerings. Conflicts are computed only for Draft sections; non-Draft sections are not evaluated for conflicts in this page.
- **Unscheduled (X)** — Count of Draft sections with 0 fully scheduled offerings (no offering with teacher + room + sufficient schedules) **in the selected college**

**Note:** Locked, Active, and Completed statuses are excluded from stats bar as they are managed outside the scheduling workflow.

- Each stat is a button that adds/removes a filter for that status
- Colors match `SectionStatusBadge` conventions (Draft=secondary, Open=green, Cancelled=red)
- Stats reflect **the selected college's data only** (updated when college selector changes)

**Data source:** API endpoint (`GET /api/colleges/{collegeId}/class-sections/stats?academicYearId={id}`) dedicated to college-scoped aggregate stats. `academicYearId` comes from the existing selected academic year context. Refreshed on page load, college selection change, and after mutations.

---

### 4.2 Course Grouping (Single College View)

Since only **one college is selected at a time**, sections are grouped by **Course** (program) within that college. Each course group is an accordion.

```
┌─ College of Engineering ──────────────────────────────────────────────────┐
│                                                                             │
│  ▸ BSCS — Computer Science (4 sections)        [████████░░] 80% scheduled   │
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
│  ▸ BSIT — Information Technology (3 sections)   [██░░░░░░░░] 45% scheduled   │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │  ...                                                                 │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ▸ BSEE — Electrical Engineering (5 sections)   [██████░░░░] 60% scheduled   │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │  ...                                                                 │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
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
  - `⚠️ X` = Unresolved errors count for the section (number of Error-severity enrollment-eligibility messages)
  - `❌ Y` = Conflicts count for the section (number of offerings with at least one Error-severity scheduling conflict)
- Amber color when only errors present; red when conflicts present (or both)
- Click to see details in drawer (see Conflict Preview Drawer)
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

The table maintains all existing filtering/sorting behavior via `nuqs` and `DataTableAdvancedToolbar`.

---

### 4.5 View Toggle, College Selector & Quick Filters

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  [College: Engineering ▾]  [Academic Year: 2025-2026]                       │
│  View: [Card ●] [Table ○]                                                   │
│                                                                             │
│  Quick Filters:                                                             │
│  [All] [Draft] [Open] [Cancelled] [Unresolved Errors] [Conflicts]          │
│  [Needs Attention ◉]  ← combines: Draft + Unresolved Errors + Conflicts    │
└─────────────────────────────────────────────────────────────────────────────┘
```

**College Selector:**
- Dropdown showing all colleges in the institution
- Selecting a college filters the displayed sections, courses, and stats to that college only
- Default: first college or last selected college (stored in `sessionStorage`)
- Refreshes stats bar, section list, and filtering on change

**View & Filters:**
- **Card/Table toggle** — persists choice in `sessionStorage`
- **Quick Filters** — preset filter configurations (scoped to selected college):
  - "Unresolved Errors" → Draft sections with at least one **Error-severity** enrollment-eligibility message
  - "Conflicts" → Draft sections with at least one **Error-severity** scheduling conflict
  - "Needs Attention" → Draft sections that either have unresolved errors OR have conflicts
- **Status filters:** Draft, Open, Cancelled only (Locked/Active/Completed excluded per scheduling scope)
- **Note:** Export functionality is not currently supported; data analysis happens in detail/room scheduler pages

---

### 4.6 Batch Operations Toolbar (Course-Level Only)

When one or more **Draft sections** are selected within a course group (via row checkboxes or "Select All" in that course):

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  [Inside each expanded course group]                                         │
│                                                                             │
│  ☑ 3 Draft sections selected  [Clear]                                       │
│  [Assign Adviser ▾]  [Open for Enrollment]  [Cancel]                       │
│  [Bulk Edit ▾]  [Capacity / Term / Year Level]                             │
│                                                                             │
│  ⓘ Batch operations only available for Draft sections                      │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Course-Level Batch Toolbar:**
- **Location:** Inside each expanded Course Group header
- **Scope:** Actions affect only sections selected within that specific course
- Each course group has its own independent batch selection and toolbar

**Behavior:**
- **Only Draft sections can be batch-operated.** If non-Draft sections are selected, batch buttons are disabled with tooltip: "Batch operations only available for Draft sections"
- Toolbar actions are scoped to that course's selections only (e.g., clicking "Assign Adviser" on Course A's toolbar affects only Course A's selected sections)
- Each action opens a bulk-action drawer similar to `BulkInitializeSectionsDrawer`
- Adviser assignment opens `SearchTeachersDialog` with multi-select
- Status transitions show a confirmation with count of affected sections

**Available Batch Actions (Draft only):**
| Action | Purpose |
|---|---|
| **Assign Adviser** | Bulk assign same adviser to multiple Draft sections in this course |
| **Open for Enrollment** | Transition selected Draft sections to Open status (validates eligibility per section) |
| **Cancel** | Cancel selected Draft sections. Shows confirmation dialog with "X offerings will be freed" (teacher/room assignments released via soft-delete cascade) |
| **Bulk Edit** | Edit capacity, term, or year level across multiple sections in this course |

**Constraints:**
- Cannot batch-operate on mixed statuses. Show warning if user selects both Draft and non-Draft sections.
- "Open All" (course-level) only affects Draft sections in that course. Eligible sections are validated individually by the backend.
- Since only one college is displayed, global cross-course batch operations are not needed.

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
- For Draft → Open: backend must validate **both** enrollment eligibility **and** scheduling conflicts. Any **Error-severity** conflict or enrollment-eligibility Error blocks the transition and shows an inline error.
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
The hover tooltip receives **conflict data from the offering details endpoint**. For each Error-severity conflict, the endpoint includes:
- Conflict type, severity, message, day, start/end time
- A list of `affectedOfferings` with subject, section, and room summaries

**Interaction:**
- Click "View Full Schedule" → navigates to section detail page's "Weekly Grid" tab
- Click conflicting offering box → navigates to conflicting section's detail page
- Tooltip auto-closes when mouse leaves card

**Performance & Loading Strategy (two-phase):**
1. **Phase 1 (instant):** Static summary text is rendered on hover immediately (e.g., "3 offerings, 1 conflict detected"). No data fetch — derived from the `validationSummary` already available in the college-filtered list response.
2. **Phase 2 (async):** Full mini weekly grid with conflict visualization is fetched from `GET /api/class-sections/{sectionId}/offerings` (offering details endpoint) and rendered once loaded. A subtle loading indicator shows during fetch.
- Mini calendar is **lazy-rendered** on hover (not pre-rendered for all cards)
- Offering details are **cached** per sectionId after first fetch (TanStack Query)
- No offering data is embedded in the college-filtered list response — keeps payload lean

All files live under a new directory `src/page-components/sections-management-page-v2/`. The existing `/sections` page and its components are **not modified** — the new page runs at a separate route (e.g., `/scheduling/sections`).

```
sections-management-page-v2/
├── index.tsx                           # Page root — college selector, view toggle, stats, grouping orchestration
├── searchParams.ts                     # nuqs parsers (collegeId, view, filters, sort) for this single-college page
├── college-selector.tsx                # Dropdown to select college from institution list
├── sections-table.tsx                  # Enhanced table view (new columns, inline actions)
├── sections-card-view.tsx              # Card view layout with course grouping (single college)
├── section-card.tsx                    # Single section card component
├── section-card-mini-schedule.tsx      # Mini weekly grid tooltip/hover card (async load)
├── stats-bar.tsx                       # College-scoped aggregate statistics bar (clickable filters)
├── quick-filters.tsx                   # Preset filter buttons
├── batch-actions-toolbar.tsx           # Course-level batch operations toolbar (no global bar)
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
├── college-collection.ts               # NEW or extend — fetch list of colleges for selector
src/api/models/
├── class-section.ts                    # Extend with SchedulingSummary type
├── college.ts                          # Ensure basic college model available
```

---

## 6. Implementation Phases

### Phase 1: Scaffold & Data Layer (2-3 days)
**Goal:** New route, new files, backend API extensions for college-filtered sections (full list per college), and query/mutation infrastructure.

| Task | Deliverable | Dependencies |
|---|---|---|
| Create new route `/scheduling/sections` with TanStack Router file | `src/routes/scheduling.sections.tsx` | None |
| Set up `searchParams.ts` with `collegeId`, `view`, `filters`, `sort` params | `searchParams.ts` | None |
| Create backend college-filtered sections endpoint that returns the full list of sections for a college | Backend: `GET /api/colleges/{collegeId}/class-sections` | None |
| Extend backend endpoint to include `validationSummary` per section | Backend: filtered sections query | None |
| Create backend college-scoped aggregate stats endpoint | Backend: `GET /api/colleges/{collegeId}/class-sections/stats` | None |
| Create backend offering details lazy endpoint | Backend: `GET /api/class-sections/{sectionId}/offerings` | None |
| Create backend bulk operation endpoints (open, cancel, assign-adviser) | Backend: 3 POST endpoints | None |
| Create/extend backend college list endpoint for selector | Backend: `GET /api/colleges` (basic list) | None |
| Update frontend API collection with new queries/mutations | `class-section-collection-v2.ts` | Backend endpoints |
| Regenerate API types (`npm run generate:api:win`) | `src/api/generated/api.ts` | Backend endpoints |
| Create page scaffold (`index.tsx`) with `useSuspenseQuery` for sections + stats + college selector | `index.tsx` | API collection |

### Phase 2: Card View & College Selector (2-3 days)
**Goal:** Card view with course grouping for a single selected college, college selector, section cards, progress display.

| Task | Deliverable | Dependencies |
|---|---|---|
| Build `CollegeSelector` component (dropdown with college list) | `college-selector.tsx` | Phase 1 college list API |
| Build `StatsBar` component with college-scoped clickable stat buttons | `stats-bar.tsx` | Phase 1 API |
| Build `QuickFilters` component | `quick-filters.tsx` | None |
| Build `SectionCard` component with all zones (header, adviser, progress, issues, actions) | `section-card.tsx` | Phase 1 data types |
| Build `SectionsCardView` with CourseGroup → SectionCard nesting (single college) | `sections-card-view.tsx` | `SectionCard` |
| Wire up college selector and view toggle in `index.tsx` | `index.tsx` | All above |
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
**Goal:** Batch select within course groups, course-level toolbars, bulk actions.

| Task | Deliverable | Dependencies |
|---|---|---|
| Build batch selection state management (local `Set<number>`, course-scoped) | `sections-card-view.tsx` + `sections-table.tsx` | Phases 2-3 |
| Build `BatchActionsToolbar` (course-level, inside each course group header) | `batch-actions-toolbar.tsx` | Selection state |
| Build `BulkStatusTransitionDialog` with count summary + "X offerings will be freed" | `bulk-status-transition-dialog.tsx` | Phase 1 bulk endpoints |
| Build `BulkAdviserAssignDrawer` | `bulk-adviser-assign-drawer.tsx` | Phase 1 bulk endpoints |
| Add course-level group toolbar with scoped batch actions | `sections-card-view.tsx` (CourseGroupToolbar) | Phase 2 |
| Disable batch buttons when non-Draft sections are selected (with tooltip) | `batch-actions-toolbar.tsx` | Selection state |
| Remove global floating toolbar (not needed for single-college view) | — | N/A |

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
| Add desktop-only viewport check with redirect suggestion banner | `index.tsx` | None |
| Add ARIA labels on progress bars, course group headers | All components | All phases |
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
Phase 1 (Scaffold & College-filtered API)
    │
    ├──► Phase 2 (Card View & Selector) ──► Phase 4 (Batch Ops) ──► Phase 6 (Polish)
    │                                                                      │
    └──► Phase 3 (Table View) ──────────► Phase 5 (Conflict Peek) ────────┘
                                                                           │
                                                                           ▼
                                                                   Phase 7 (Tests)
```

Phases 2 and 3 can be built in parallel after Phase 1. Phases 4 and 5 can be partially parallel once their dependencies are done.

---

## ASCII Wireframe: Final Page Mockup (Card View)

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│  Curriculum & Scheduling  >  Class Sections                                          │
├──────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                       │
│  [College: Engineering ▾]  [Academic Year: 2025-2026]                                │
│                                                                                       │
├──────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                       │
│  [📋 Draft: 8]  [📂 Open: 42]  [❌ Cancelled: 2]                                      │
│  [⚠️ Unresolved Errors: 3]  [🔴 Conflicts: 2]  [🕐 Unscheduled: 5]                   │
│                                                                                       │
├──────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                       │
│  [● Card View]  [○ Table View]    │    Quick Filters: [All] [Draft] [Open] [Cancel] │
│                                                  [Unresolved Errors] [Conflicts]    │
│                                                                                       │
├──────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                       │
│  College of Engineering                                                              │
│                                                                                       │
│    ▸ BSCS — Computer Science (4)                        [████████░░] 75% scheduled    │
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
│    │ [☐ Select All]  [Assign Adviser ▾]  [Open for Enrollment]  [Cancel]      │     │
│    │                                                                           │     │
│    └────────────────────────────────────────────────────────────────────────────┘     │
│                                                                                       │
│    ▸ BSIT — Information Technology (3)          [██░░░░░░░░] 45% scheduled           │
│    ┌────────────────────────────────────────────────────────────────────────────┐     │
│    │ ☑ 2A  BSIT 2A                             █ Draft    ⚠️2/❌1            │     │
│    │    BSIT — Information Tech · 1st Sem AY 2025-26                           │     │
│    │    Adviser: (unassigned)                                                  │     │
│    │    Scheduling: [██░░░░░░░░] 1 of 3 offerings scheduled (33%)              │     │
│    │    Issues: 2 offerings missing teacher · 1 missing room · 1 conflict      │     │
│    │    [Open for Enrollment]  [Cancel]  [View Details →]                     │     │
│    │                                                                           │     │
│    │ [☐ Select All]  [Assign Adviser ▾]  [Open for Enrollment]  [Cancel]      │     │
│    │                                                                           │     │
│    └────────────────────────────────────────────────────────────────────────────┘     │
│                                                                                       │
│    ▸ BSEE — Electrical Engineering (5)          [██████░░░░] 60% scheduled           │
│    ...                                                                                │
│                                                                                       │
└──────────────────────────────────────────────────────────────────────────────────────┘
```

---

## ASCII Wireframe: Final Page Mockup (Table View)

```
┌───────────────────────────────────────────────────────────────────────────────────────────┐
│  Curriculum & Scheduling  >  Class Sections                                              │
├───────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                            │
│  [College: Engineering ▾]  [Academic Year: 2025-2026]                                     │
│                                                                                            │
├───────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                            │
│  [📋 Draft: 8]  [📂 Open: 42]  [❌ Cancelled: 2]                                           │
│  [⚠️ Unresolved Errors: 3]  [🔴 Conflicts: 2]  [🕐 Unscheduled: 5]                        │
│                                                                                            │
├───────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                            │
│  [○ Card View]  [● Table View]  │ Quick Filters: [All] [Draft] [Open] [Cancelled]       │
│                                              [Unresolved Errors] [Conflicts]             │
│                                                                                            │
├───────────────────────────────────────────────────────────────────────────────────────────┤
│                          BSCS — Computer Science                                         │
│ ☐ │ Code │ Section      │ Status  │Progress│Errors/CF│ Adviser  │ Actions    │         │
├───┼──────┼──────────────┼─────────┼────────┼─────────┼──────────┼────────────┤         │
│ ☑ │ 3A   │ BSCS 3A      │ Draft   │ ████ 67% │ ⚠️1/❌0│ Dr.Smith │ [O][C][▸]  │         │
│   │      │ (1st Sem)    │         │ (2/3)   │         │          │ (has error)│         │
├───┼──────┼──────────────┼─────────┼────────┼─────────┼──────────┼────────────┤         │
│ ☐ │ 3B   │ BSCS 3B      │ Open    │ ██████100%│ ✓0/✓0 │ Dr.Jones │ [C][▸]      │         │
│   │      │ (1st Sem)    │ (read)  │ (3/3)   │         │          │ (no issues)│         │
├───┼──────┼──────────────┼─────────┼────────┼─────────┼──────────┼────────────┤         │
│ ☐ │ 3C   │ BSCS 3C      │ Open    │ ██████100%│ ✓0/✓0 │ Dr.Lee   │ [C][▸]      │         │
│   │      │ (1st Sem)    │ (read)  │ (3/3)   │         │          │ (no issues)│         │
├───┼──────┼──────────────┼─────────┼────────┼─────────┼──────────┼────────────┤         │
│ ☑ │ 3D   │ BSCS 3D      │ Draft   │ ██░░ 50%  │ ⚠️1/❌1│ (unassigned)│[O][C][▸]│         │
│   │      │ (1st Sem)    │         │ (1/2)   │         │          │ (has error)│         │
├───┴──────┴──────────────┴─────────┴────────┴─────────┴──────────┴────────────┤         │
│                          BSIT — Information Technology                                  │
├───┬──────┬──────────────┬─────────┬────────┬─────────┬──────────┬────────────┤         │
│ ☑ │ 2A   │ BSIT 2A      │ Draft   │ ██░░ 33%  │ ⚠️2/❌1│ (none)   │ [O][C][▸]  │         │
│   │      │ (1st Sem)    │         │ (1/3)   │         │          │ (has error)│         │
├───┼──────┼──────────────┼─────────┼────────┼─────────┼──────────┼────────────┤         │
│ ☐ │ 2B   │ BSIT 2B      │ Draft   │ ░░░░░░ 0%│ ⚠️3/❌0│ Dr.Jones │ [O][C][▸]  │         │
│   │      │ (1st Sem)    │         │ (0/3)   │         │          │ (has error)│         │
├───┼──────┼──────────────┼─────────┼────────┼─────────┼──────────┼────────────┤         │
│ ☐ │ 2C   │ BSIT 2C      │ Open    │ ██████100%│ ✓0/✓0 │ Dr.Smith │ [C][▸]      │         │
│   │      │ (1st Sem)    │ (read)  │ (3/3)   │         │          │ (no issues)│         │
├───┴──────┴──────────────┴─────────┴────────┴─────────┴──────────┴────────────┤         │
│                                                                                            │
│  Legend:                                                                                  │
│  ⚠️1/❌0 = 1 unresolved error, 0 conflicts    [O] = Open for Enrollment (Draft only)    │
│  ✓ = No issues                                [C] = Cancel (Draft and Open)              │
│  (read) = Read-only (non-Draft status)       [▸] = View Details                        │
│                                                                                            │
└───────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## Key Metrics to Surface

**All metrics are derived from the `validationSummary` object in the college-scoped response or the aggregate stats endpoint.** The backend pre-computes validation errors and conflict counts — the frontend reads these without traversing offering data.

| Metric | Where | Source |
|---|---|---|
| **Scheduling progress %** | Card progress bar, table column | `validationSummary.totalOfferings` vs backend's scheduled count |
| **Unresolved Errors count** | Card badge, table column | `validationSummary.offeringsWithErrors` (Error-severity eligibility issues) |
| **Missing teacher count** | Card "Issues" row | `validationSummary.missingTeacherCount` |
| **Missing room count** | Card "Issues" row | `validationSummary.missingRoomCount` |
| **Missing schedule count** | Card "Issues" row | `validationSummary.missingScheduleCount` |
| **Conflicts count** | Card badge, table column | `validationSummary.offeringsWithConflicts` (offerings with at least one Error-severity conflict) |
| **Unscheduled count** | Stats bar | Aggregate stats endpoint (college-scoped): `unscheduledCount` |
| **Schedule density** | Mini weekly calendar tooltip | Offering details endpoint (lazy, async) |

**Data Flow:**
```
College-Filtered API Response (validationSummary per section)
       +
College-Scoped Stats API (aggregate counts for selected college)
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

### New Endpoint: College-Filtered Sections (Full List Per College)

**`GET /api/colleges/{collegeId}/class-sections`** — Return sections for a specific college with validation summary:

```csharp
// NEW ENDPOINT: College-filtered list (returns full list for a college)

new response structure {
  items: ClassSection[], // with extended offerings
  
  validationSummary: {
    totalDraft,
    totalOpen,
    totalCancelled,
    sectionsWithUnresolvedErrors,    // count of sections with at least one Error-severity eligibility message
    sectionsWithConflicts,           // count of sections with at least one offering that has an Error-severity conflict
    unscheduledCount
  }
}

// Inside ClassSection offering summary (in college-filtered list):
offerings: Offering[] {
  id, classSectionId, subjectId,
  teacher?, room?,
  scheduleSummary: string // e.g., "MWF 08:00-09:30"
  
  // NO validationMessages[] or conflicts[] in list response
  // These are fetched lazily via separate endpoint when needed
}
```

### Additional New Endpoints

| Endpoint | Method | Purpose |
|---|---|---|
| `GET /api/colleges/{collegeId}/class-sections/stats` | GET | College-scoped aggregate stats (replaces the old cross-college stats) |
| `GET /api/class-sections/{sectionId}/offerings` | GET | Lazy-load offering details with validation messages and conflicts (shared with other pages) |
| `POST /api/class-sections/bulk/assign-adviser` | POST | Bulk assign adviser to Draft sections (college-scoped) |
| `POST /api/class-sections/bulk/open` | POST | Bulk transition Draft sections to Open (college-scoped) |
| `POST /api/class-sections/bulk/cancel` | POST | Bulk transition Draft or Open sections to Cancelled (soft-delete cascade) |

**Note:** No export endpoint needed; data analysis handled in Room Scheduler page.

### Status Transitions & Resource Cleanup

- **Draft → Open (single-section and bulk):**
  - `OpenClassSectionForEnrollment` (and its bulk counterpart) must validate both enrollment-eligibility messages and **Error-severity** conflicts.
  - If any Error-severity eligibility message or Error-severity conflict exists, the command returns `Invalid` and the status remains Draft.
  - Only Error-severity conflicts block; Warning/Info conflicts are advisory.
- **Cancel (single-section and bulk):**
  - `CancelClassSection` (and bulk cancel) must mark related offerings/schedules/assignments as inactive or unassigned so they no longer participate in conflict detection.
  - Conflict detection queries must exclude Cancelled sections (by status) and any inactive offerings/schedules.

---

## 7. Performance Considerations

This section addresses frontend and backend optimization strategies for handling course sections, offerings, and validation data for a single college at a time.

### Frontend Performance

**Challenge:** With a single college's sections (typically 50-200 sections) and 1000+ offerings with validation data, rendering course groups + cards + mini calendars should be performant.

**Mitigation Strategies:**

| Concern | Solution |
|---|---|
| **Rendering performance with many cards** | Course groups are collapsible; expand on demand. Use `content-visibility: auto` on course blocks for offscreen skipping. No college-level virtualization needed since only one college is shown |
| **Metrics calculation overhead** | Metrics come from `validationSummary` (backend-computed, already in response). Frontend reads via `useMemo` with `validationSummary` as dependency — no offering traversal |
| **Mini calendar rendering** | **Two-phase approach:** Phase 1 (instant) shows static summary from `validationSummary`. Phase 2 (async) fetches offering details and renders full grid on hover. Data cached per `sectionId` after first fetch |
| **Sorting/filtering slowness** | Debounce filter changes (300ms); perform sorting in backend if possible; use shallow filtering (status only) for quick responses |
| **Large course groups** | Collapse course groups by default; expand on demand; `content-visibility: auto` on course blocks for offscreen skipping |
| **Bundle size** | Tree-shake unused charting libraries; lazy-load mini calendar component |

**Frontend Caching:**
- Cache college-filtered response in TanStack Query with 2-minute stale time
- Cache offering details per `sectionId` (TanStack Query, separate from list)
- Cache college-scoped stats with background refetch after mutations
- Use background refetch after mutations (optimistic updates)

### Backend Performance

**Challenge:** Calculating validation errors and conflicts for every offering on every request is expensive, even for a single college.

**Mitigation Strategies:**

| Concern | Solution |
|---|---|
| **Computing validation errors at query time** | Pre-compute during seeding; cache in database or calculated field; compute only if offerings changed |
| **Fetching all offerings for a college** | Use efficient database query with `SELECT only needed fields`; index by `ClassSectionId` and `CollegeId` |
| **Conflict detection expensive** | Cache conflict results per offering; recompute on schedule changes only; use background job (Hangfire) for batch conflict detection |
| **College-filtered response size** | Return only `validationSummary` (aggregate counts) per section — no full offering data. Full offering details fetched on demand via separate lazy endpoint |

**Database Indexing:**
- Index on `ClassSection.CollegeId` for filtering by college
- Index on `ClassSection.AcademicYearId, CollegeId` for combined filtering
- Index on `Offering.ClassSectionId` and `Offering.ConflictCount` for quick lookups
- Index on `Schedule.DayOfWeek, Schedule.StartTime` for conflict detection

### API Response Optimization

**Payload size:** The college-filtered response includes only `validationSummary` per section (no full offerings). Full offering details with validation messages + conflicts are fetched on demand via a separate lazy endpoint.

**Mitigation:**
- Offering details are **not embedded** in the college-filtered response — fetched lazily via `GET /api/class-sections/{sectionId}/offerings`
- College-scoped stats come from a **separate endpoint** (`GET /api/colleges/{collegeId}/class-sections/stats`), not embedded in the filtered response
- Compress JSON response (gzip)
- Mock conflict data for development (faker.js); real computation on production

### Recommendations

1. **Start with:** Single-college full list + lazy-loaded mini calendars + memoized metrics
2. **Monitor:** Frontend render time with React DevTools Profiler
3. **Optimize if slow:** Add virtual scrolling, background workers for conflict detection
4. **Test:** Load test with 500+ sections to identify bottlenecks

---

## 8. API Response Structure

### Overview of Three API Sources

| Endpoint | Purpose | Called When |
|---|---|---|
| `GET /api/colleges/{collegeId}/class-sections` | College-filtered section list with `validationSummary` per section (full list) | On page load, college selection change, filter/sort changes |
| `GET /api/colleges/{collegeId}/class-sections/stats` | College-scoped aggregate stats (always for selected college) | On page load, college selection change, and after mutations |
| `GET /api/class-sections/{sectionId}/offerings` | Full offering details with validation messages + conflicts | On user interaction (hover, expand, badge click) |

### 8.1 College-Filtered Section List (No Pagination)

**New Response Structure:**
```json
{
  "items": [ClassSection],
  
  "validationSummary": {
    "totalDraft": 8,
    "totalOpen": 42,
    "totalCancelled": 2,
    "sectionsWithUnresolvedErrors": 3,
    "sectionsWithConflicts": 2,
    "unscheduledCount": 5
  }
}
```

**Extended per section:**
```json
{
  "items": [
    {
      "id": 1,
      "name": "BSCS 3A",
      "sectionCode": "3A",
      "status": { "value": 1, "name": "Draft" },
      "course": { "id": 1, "code": "BSCS", "name": "Computer Science" },
      "college": { "id": 10, "name": "College of Engineering" },
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
  
  // College-scoped stats (replaces old cross-college totals)
  "validationSummary": {
    "totalDraft": 8,
    "totalOpen": 42,
    "totalCancelled": 2,
    "sectionsWithUnresolvedErrors": 3,
    "sectionsWithConflicts": 2,
    "unscheduledCount": 5
  }
}
```

**Note:** College-scoped stats are embedded in the response for convenience.

### 8.2 College-Scoped Stats Endpoint

**`GET /api/colleges/{collegeId}/class-sections/stats?academicYearId={id}`**

```json
{
  "totalDraft": 8,
  "totalOpen": 42,
  "totalCancelled": 2,
  "sectionsWithUnresolvedErrors": 3,
  "sectionsWithConflicts": 2,
  "unscheduledCount": 5
}
```

- College-scoped numbers — reflects the **selected college only**
- Not affected by status/error filters (always shows full counts for the college)
- Refetched after any status transition mutation
- Refetched when college selection changes

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

      "conflicts": [
        {
          "id": "conflict-001",
          "type": "ROOM_DOUBLE_BOOKED",
          "severity": "Error",
          "message": "Room 101 double-booked on MON 08:00-09:30",
          "day": "MON",
          "startTime": "08:00:00",
          "endTime": "09:30:00",
          "affectedOfferings": [
            {
              "id": 202,
              "subject": { "code": "ENGL", "title": "English 101" },
              "section": { "id": 2, "name": "BSEE-2A" },
              "room": { "roomNumber": "101", "building": "Main" }
            }
          ]
        }
      ]
    }
  ],

  "validationMessages": {
    "classSectionId": 1,
    "classSectionValidationMessages": [
      {
        "severity": { "name": "Error", "value": 3 },
        "code": "CLASS_SECTION_ADVISER_REQUIRED",
        "message": "Class section must have an adviser assigned.",
        "computedAt": "2026-06-10T15:30:00Z"
      }
    ],
    "offeringsValidationMessages": {
      "101": [
        {
          "offeringId": 101,
          "severity": { "name": "Error", "value": 3 },
          "code": "SUBJECT_OFFERING_INSUFFICIENT_CLASS_SCHEDULES",
          "message": "Subject offering must have class schedules equal to expected days per week.",
          "computedAt": "2026-06-10T15:30:00Z"
        }
      ]
    }
  },
   
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

Enrollment-eligibility validation messages are stored as domain validation messages with standardized codes. Examples:

| Code | Severity | Category | UI Display |
|---|---|---|---|
| `CLASS_SECTION_ADVISER_REQUIRED` | Error | Unresolved Error | "Class section must have an adviser assigned" |
| `CLASS_SECTION_SUBJECT_OFFERINGS_MISSING` | Error | Unresolved Error | "Class section must have subject offerings" |
| `SUBJECT_OFFERING_TEACHER_REQUIRED` | Error | Unresolved Error | "Subject offering must have a teacher assigned" |
| `SUBJECT_OFFERING_ROOM_REQUIRED` | Error | Unresolved Error | "Subject offering must have a room assigned" |
| `SUBJECT_OFFERING_INSUFFICIENT_CLASS_SCHEDULES` | Error | Unresolved Error | "Subject offering must have class schedules equal to expected days per week" |
| `SUBJECT_OFFERING_INVALID_CLASS_SCHEDULES` | Error | Unresolved Error | "Subject offering has invalid class schedules" |
| `SUBJECT_OFFERING_DAYS_PER_WEEK_DEFAULT_VALUE` | Warning | Advisory | "Days per week is set to the default value of 1" |
| `SUBJECT_OFFERING_HOURS_PER_DAY_DEFAULT_VALUE` | Warning | Advisory | "Hours per day is set to the default value of 1" |
| `SUBJECT_OFFERING_MAX_NUMBER_OF_STUDENTS_DEFAULT_VALUE` | Warning | Advisory | "Max number of students is set to the default value of 0" |

**Frontend Metric Calculation (conceptual):**
- **Unresolved Errors count** = Provided by backend (`unresolvedErrorsCount` / `sectionsWithUnresolvedErrors`) based on Error-severity validation messages
- **Missing teacher/room/schedule detection** = Derived from validation codes like `SUBJECT_OFFERING_TEACHER_REQUIRED`, `SUBJECT_OFFERING_ROOM_REQUIRED`, `SUBJECT_OFFERING_INSUFFICIENT_CLASS_SCHEDULES`, `SUBJECT_OFFERING_INVALID_CLASS_SCHEDULES`
- **Conflicts count** = Provided by backend (`offeringsWithConflicts` / `sectionsWithConflicts`) and based only on **Error-severity** conflicts

### Implementation Notes for API Changes

**Phase 1: Create College-Filtered Sections Endpoint**
- New handler: `GetCollegeClassSectionsQuery`
- Returns all sections for a college (no server-side paging): `List<ClassSectionDto>` + embedded `validationSummary`
- URL: `GET /api/colleges/{collegeId}/class-sections?academicYearId={id}&statusFilter={status}&...`
- Keep offering summaries only (id, subject code, teacher name, room number, schedule pattern text)
- Embed college-scoped stats in response for convenience
- **Do NOT** embed `validationMessages[]` or `conflicts[]` in the college-filtered response

**Phase 2: Create College-Scoped Stats Endpoint**
- New handler: `GetCollegeClassSectionStatsQuery`
- Returns college-scoped counts: `totalDraft`, `totalOpen`, `totalCancelled`, `sectionsWithUnresolvedErrors`, `sectionsWithConflicts`, `unscheduledCount`
- URL: `GET /api/colleges/{collegeId}/class-sections/stats?academicYearId={id}`
- Always reflects the selected college — not affected by filter parameters

**Phase 3: Create Separate Offering Details Endpoint**
- New handler: `GetClassSectionOfferingsQuery` (shared with other pages)
- Returns full offering data plus enrollment-eligibility validation messages and conflicts
- Conflicts computed only for **Draft sections** (Open sections are finalized)
- Cached on the frontend per `sectionId`

**Phase 4: Update Frontend Parsing**
- Parse `validationSummary` from college-filtered response for list/metric display
- Fetch offering details on demand for mini calendar and conflict preview
- Treat a section as having unresolved errors when it has at least one **Error-severity** enrollment-eligibility message (backend-computed fields preferred)
- For detailed UI, use eligibility codes like `CLASS_SECTION_ADVISER_REQUIRED`, `SUBJECT_OFFERING_TEACHER_REQUIRED`, `SUBJECT_OFFERING_ROOM_REQUIRED`, `SUBJECT_OFFERING_INSUFFICIENT_CLASS_SCHEDULES`, `SUBJECT_OFFERING_INVALID_CLASS_SCHEDULES`
- **No additional client-side validation logic needed**—rely on backend validation computation

---

## Risk & Mitigation

| Risk | Mitigation |
|---|---|
| Large API response size (validation + conflicts) | Offering details are NOT embedded in college-filtered response — fetched lazily via separate endpoint |
| Performance with 100+ sections in a large college expanded | `content-visibility: auto` on course blocks; collapse course groups by default; lazy-render mini calendars |
| Metrics calculation overhead on frontend | Metrics served as `validationSummary` (backend-computed, pre-cached). No offering traversal on frontend |
| BULK actions on stale data | Optimistic UI + background refetch; validate before transitions via backend |
| Cognitive overload (too much info) | Progressive disclosure (collapse course groups by default; only one college shown at a time) |
| Conflicting offerings missing from response | Offering details endpoint returns full conflict data; verify in integration tests |
| User selects mixed-status sections for batch | Show warning; disable batch buttons if non-Draft selected |
| Mini calendar blank on hover while data loads | Two-phase load: static summary immediately, full grid async with loading indicator |
| College selector not representative of institution | Ensure college list is complete and up-to-date; handle college deletion gracefully (prevent selection if deleted) |
| Missing college change on stats refresh | Use `collegeId` from URL params to fetch/refresh stats; always synchronized with current selection |

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
- Enrollment-eligibility validation messages for a section and its offerings
- Error-severity issues such as adviser missing, offerings missing, teacher/room missing, insufficient or invalid schedules
- Prevent "Open for Enrollment" when any Error-severity message exists

**Conflicts:**
- Cross-section scope (teacher/room/section overlaps across sections in the same academic term)
- Detected via the conflict detection pipeline; only **Error-severity** conflicts block Draft → Open
- Cancelled sections (and their offerings/schedules) must be excluded from conflict detection once cancel frees assignments
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
