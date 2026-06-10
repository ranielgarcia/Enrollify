# Class Sections Management Page — PRD

## PRD-02: Functional Requirements

---

### FR-01: View Toggle (Card / Table)

| ID | Requirement | Priority |
|---|---|---|
| FR-01.1 | User can toggle between **Card View** and **Table View** via a toggle control | P0 |
| FR-01.2 | Toggle choice is persisted in `sessionStorage` (survives page refresh, not cross-tab) | P1 |
| FR-01.3 | Card view is the default | P0 |
| FR-01.4 | Academic Year selector is visible in all views | P0 |

### FR-02: Stats Bar

| ID | Requirement | Priority |
|---|---|---|
| FR-02.1 | Stats bar displays aggregate counts for Draft, Open, Cancelled, Unresolved Errors, Conflicts, and Unscheduled | P0 |
| FR-02.2 | Each stat is clickable — applies a filter for that category | P0 |
| FR-02.3 | Stats reflect full-dataset numbers regardless of current page or active filters | P0 |
| FR-02.4 | Stats come from a **separate API endpoint** (not derived from paginated response) | P0 |
| FR-02.5 | Locked, Active, and Completed statuses are excluded from stats bar | P1 |

**Stat definitions:**
- **Draft (X)** — Sections in Draft status
- **Open (X)** — Sections opened for enrollment
- **Cancelled (X)** — Sections that were cancelled
- **Unresolved Errors (X)** — Sections with ≥1 offering missing teacher OR room OR schedule
- **Conflicts (X)** — Sections with ≥1 offering having scheduling conflicts with other offerings
- **Unscheduled (X)** — Sections with 0 complete offerings (offerings lacking teacher+room+schedule)

### FR-03: Quick Filters

| ID | Requirement | Priority |
|---|---|---|
| FR-03.1 | Quick filter buttons: All, Draft, Open, Cancelled, Unresolved Errors, Conflicts, Needs Attention | P0 |
| FR-03.2 | "Needs Attention" = Draft status OR has unresolved errors OR has conflicts | P0 |
| FR-03.3 | Quick filters apply as URL-synced search params (via `nuqs`) | P0 |
| FR-03.4 | Active filter is visually highlighted | P1 |
| FR-03.5 | Multiple filters can be combined (e.g., Draft + Conflicts) | P1 |

### FR-04: College / Course Grouping (Card View)

| ID | Requirement | Priority |
|---|---|---|
| FR-04.1 | Sections are grouped by College (accordion header) then by Course (sub-group) | P0 |
| FR-04.2 | College headers show: college name, total section count, aggregate scheduling progress bar | P0 |
| FR-04.3 | Course headers show: course code + name, section count, scheduling progress bar | P0 |
| FR-04.4 | Course groups are collapsed by default; expand on click | P0 |
| FR-04.5 | Each course group has a **group actions bar** (batch select, Assign Adviser, Open All, Cancel) scoped to that course | P1 |

### FR-05: Section Card

| ID | Requirement | Priority |
|---|---|---|
| FR-05.1 | Each section renders as a card with: checkbox, section code, status badge, error/conflict badge, subtitle, adviser row, scheduling progress bar, issues row, room summary, schedule summary, action buttons | P0 |
| FR-05.2 | Draft card shows `[Open]` `[Cancel]` `[Details]` buttons | P0 |
| FR-05.3 | Open card shows `[Cancel]` `[Details]` buttons (lifecycle permits Open → Cancelled) | P0 |
| FR-05.4 | Locked/Active/Completed card shows only `[Details]` | P0 |
| FR-05.5 | Checkbox is **disabled** for non-Draft sections | P0 |
| FR-05.6 | **Red left border** if section has ≥1 unresolved error OR ≥1 conflict | P1 |
| FR-05.7 | Action buttons disabled for non-Draft sections (grayed out) | P1 |
| FR-05.8 | Adviser row shows "Change" button only for Draft sections | P1 |
| FR-05.9 | Error/conflict badge is clickable → opens conflict-preview drawer | P2 |

### FR-06: Enhanced Table View

| ID | Requirement | Priority |
|---|---|---|
| FR-06.1 | Table columns: checkbox, Code, Section, Course, Status, Progress (Sched%), Errors/Conflicts, Adviser, Actions | P0 |
| FR-06.2 | New columns: Progress bar + percentage, Errors/Conflicts compound badge, inline action buttons | P0 |
| FR-06.3 | Red left border on rows with errors OR conflicts | P1 |
| FR-06.4 | Gray background on non-Draft (read-only) rows | P1 |
| FR-06.5 | Disabled action buttons for non-Draft status transitions | P1 |
| FR-06.6 | Existing filtering/sorting/pagination via `nuqs` and `DataTableAdvancedToolbar` is maintained | P0 |

### FR-07: Inline Status Transitions

| ID | Requirement | Priority |
|---|---|---|
| FR-07.1 | Status transitions happen directly on cards and table rows (no detail page navigation) | P0 |
| FR-07.2 | **Draft → Open:** triggers eligibility validation; if passes, transitions and refreshes | P0 |
| FR-07.3 | **Draft/Open → Cancelled:** shows confirmation dialog with impact summary | P0 |
| FR-07.4 | After transition: card stays in place, status badge updates, action buttons update, stats bar refreshes | P0 |
| FR-07.5 | Use existing transition mutations (`openClassSectionOptions`, `cancelClassSectionOptions`) | P1 |
| FR-07.6 | Show toast notification on success; show inline error on failure | P1 |

### FR-08: Conflict & Error Display

| ID | Requirement | Priority |
|---|---|---|
| FR-08.1 | Errors and conflicts are displayed as a compound badge: `⚠️X/❌Y` (both card + table) | P0 |
| FR-08.2 | Amber color when only errors present; red when conflicts present (or both) | P1 |
| FR-08.3 | Badge is clickable → opens conflict-preview drawer | P1 |
| FR-08.4 | **Both** unresolved errors AND conflicts prevent "Open for Enrollment" | P0 |
| FR-08.5 | Open sections do NOT have conflict calculation performed; only Draft sections calculate conflicts against existing Open sections | P0 |
| FR-08.6 | Validation error codes: `MISSING_TEACHER`, `MISSING_ROOM`, `NO_SCHEDULE` (Error severity); `POTENTIAL_CONFLICT` (Warning severity) | P0 |

### FR-09: Mini Weekly Calendar (Hover Tooltip)

| ID | Requirement | Priority |
|---|---|---|
| FR-09.1 | Hovering over a card triggers a mini weekly calendar tooltip | P1 |
| FR-09.2 | **Immediate feedback:** static summary shown instantly on hover | P1 |
| FR-09.3 | **Async detail:** full mini-weekly grid with conflict visualization loads asynchronously | P1 |
| FR-09.4 | Conflicting offerings shown with red border | P2 |
| FR-09.5 | Click "View Full Schedule" → navigates to section detail page | P2 |
| FR-09.6 | Click conflicting offering → navigates to conflicting section's detail page | P2 |
| FR-09.7 | Tooltip auto-closes when mouse leaves card | P1 |

### FR-10: Batch Operations

| ID | Requirement | Priority |
|---|---|---|
| FR-10.1 | Floating sticky toolbar appears when ≥1 Draft section is selected | P0 |
| FR-10.2 | Toolbar shows: selection count, `[Assign Adviser]`, `[Open for Enrollment]`, `[Cancel]`, `[Bulk Edit ▾]` | P0 |
| FR-10.3 | **Only Draft sections** can be batch-operated | P0 |
| FR-10.4 | If non-Draft sections selected, batch buttons are disabled with tooltip: "Batch operations only available for Draft sections" | P1 |
| FR-10.5 | **Course-level toolbar** actions are scoped to that course's selections only | P0 |
| FR-10.6 | **Global toolbar** actions affect all selected sections across all courses on current page | P0 |
| FR-10.7 | Batch operations on mixed-status selections show a warning | P1 |
| FR-10.8 | Bulk Cancel shows confirmation dialog detailing "X offerings will be freed" (teacher/room assignments) | P1 |
| FR-10.9 | Bulk Cancel performs soft-delete cascade (frees teachers/rooms) | P1 |

### FR-11: Empty State

| ID | Requirement | Priority |
|---|---|---|
| FR-11.1 | When filters return zero results, show empty state message: "No sections match your current filters" | P1 |
| FR-11.2 | Empty state suggests: "Try: Widen your status filter, Clear Conflicts filter, Select a different year" | P1 |
| FR-11.3 | Empty state includes `[Clear All Filters]` button that resets `nuqs` search params | P1 |
| FR-11.4 | Stats bar and year selector remain visible when list is empty | P1 |
| FR-11.5 | If no sections exist at all (not just filtered): "No sections have been created for this academic year. [Bulk Initialize →]" | P2 |

### FR-12: Lifecycle Scope

| ID | Requirement | Priority |
|---|---|---|
| FR-12.1 | Page manages **Draft** and **Open** ↔ **Cancelled** transitions only | P0 |
| FR-12.2 | Locked → Active → Completed transitions are automated (managed outside this page) | P0 |
| FR-12.3 | Scheduling workflow scope: Draft → Open (admin initiates), Draft/Open → Cancelled (admin cancels) | P0 |
