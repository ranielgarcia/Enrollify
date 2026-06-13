# Class Sections Management Page — PRD

## PRD-03: Technical Design

---

### 1. Page Route Strategy

- **New route:** `/scheduling/sections` (or `/sections-v2`) — does NOT modify the existing `/sections` page
- Old page remains at its existing route until manually deleted
- No shared state or components between old and new page

### 2. File Structure

All files live under `src/page-components/sections-management-page-v2/` (new directory):

```
sections-management-page-v2/
├── index.tsx                           # Page root — college selector, view toggle, stats, grouping orchestration
├── searchParams.ts                     # nuqs parsers (collegeId, view, filters, sort)
├── college-selector.tsx                # Dropdown to select college
├── sections-table.tsx                  # Enhanced table view (new columns, inline actions)
├── sections-card-view.tsx              # Card view layout with course grouping (single college)
├── section-card.tsx                    # Single section card component
├── section-card-mini-schedule.tsx      # Mini weekly grid tooltip/hover card
├── stats-bar.tsx                       # College-scoped aggregate statistics bar (clickable filters)
├── quick-filters.tsx                   # Preset filter buttons
├── batch-actions-toolbar.tsx           # Course-level batch operations toolbar
├── bulk-adviser-assign-drawer.tsx      # Bulk assign adviser drawer
├── bulk-status-transition-dialog.tsx   # Confirm bulk status change
├── conflict-preview-drawer.tsx         # Inline conflict summary drawer
├── inline-transition-buttons.tsx       # Status transition buttons — Draft(Open/Cancel), Open(Cancel), Others(Details)
├── empty-state.tsx                     # Empty state component (filtered + no-data variants)
├── section-status-badge.tsx            # Re-use existing or port
└── cancel-section-alert-dialog.tsx     # Re-use existing pattern or re-implement
```

### 3. Component Tree

```
Page (index.tsx)
├── CollegeSelector
├── StatsBar
│   └── StatButton[] (clickable → adds nuqs filter)
├── AcademicYearDisplay (read-only, from existing selected academic year context)
├── ViewToggle (card/table)
├── QuickFilters
├── [View Router]
│   ├── CardView
│   │   └── CourseGroup[]
│   │       ├── CourseGroupToolbar (course-scoped batch)
│   │       └── SectionCard[]
│   │           ├── InlineTransitionButtons
│   │           ├── MiniScheduleTooltip (lazy async)
│   │           └── Checkbox (disabled if non-Draft)
│   └── TableView
│       └── DataTable
│           └── Row[]
│               ├── Checkbox (disabled if non-Draft)
│               ├── InlineTransitionButtons
│               └── CompoundBadge (clickable → ConflictPreviewDrawer)
├── ConflictPreviewDrawer
├── BulkStatusTransitionDialog
├── BulkAdviserAssignDrawer
└── EmptyState (when list is empty)
```

### 4. API Endpoints

#### 4.1 College-Filtered Section List (Full List Per College)

**`GET /api/colleges/{collegeId}/class-sections?academicYearId={id}`** — Single-college list response with lightweight per-section aggregates

- Returns a full list of sections for the selected college (no server-side paging)
- Includes per-section `validationSummary` and a college-scoped summary block
- Offerings are **summary only** (no nested validation/conflict arrays)
- Full details (eligibility messages + schedules + conflicts) are lazy-loaded via the offering details endpoint

#### 4.2 College-Scoped Stats Endpoint

**`GET /api/colleges/{collegeId}/class-sections/stats?academicYearId={id}`**

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

- Always returns selected-college numbers regardless of active filters
- Refreshed via separate TanStack Query with background refetch after mutations

#### 4.3 New: Offering Details Endpoint (Lazy Load)

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
      "teacher": { ... },
      "room": { ... },
      "schedules": [...],
      "conflicts": [
        { "id": "c-001", "type": "ROOM_DOUBLE_BOOKED", "severity": "Error", ... }
      ]
    }
  ]
}
```

- Called on demand: when user expands card details, hovers for mini schedule, or clicks errors/conflicts badge
- Conflicts are computed only for Draft sections in this page
- Only **Error-severity** conflicts block Draft → Open

#### 4.4 Bulk Operation Endpoints

| Endpoint | Method | Purpose |
|---|---|---|
| `POST /api/class-sections/bulk/assign-adviser` | POST | Bulk assign adviser to Draft sections |
| `POST /api/class-sections/bulk/open` | POST | Bulk transition Draft sections → Open (validates eligibility) |
| `POST /api/class-sections/bulk/cancel` | POST | Bulk cancel Draft/Open sections (soft-delete cascade) |

All bulk endpoints:
- Accept `{ sectionIds: number[] }`
- Return `{ succeeded: number, failed: number, errors: { sectionId, message }[] }`
- Validate all sections are Draft (for open) or Draft/Open (for cancel) before processing

### 5. Data Flow

```
┌────────────────────────┐
│  Stats API             │ ← TanStack Query, 2-min stale, refetch on mutations
│  (/api/colleges/.../stats)│
└────────┬───────────────┘
         │ stats data
         ▼
┌────────────────────────┐
│  College Sections List │ ← TanStack Query, collegeId+filter params as query key
│  (/api/colleges/.../class-sections) │
└────────┬───────────────┘
         │ sections[] with validationSummary
         ▼
┌────────────────────────┐
│  Frontend Metrics      │ ← useMemo per section, derived from validationSummary
│  (progress %, etc.)    │     No full-offering traversal needed for list display
└────────┬───────────────┘
         │
         ▼ (on user interaction — hover, expand, click badge)
┌────────────────────────┐
│  Offering Details API  │ ← Lazy fetch, cached per sectionId
│  (/api/.../{id}/off.)  │
└────────┬───────────────┘
         │ full offering[] with validationMessages + conflicts
         ▼
    ┌───────────┬───────────────┐
    │ Mini       │ Conflict      │
    │ Calendar   │ Preview       │
    │ Tooltip    │ Drawer        │
    └───────────┴───────────────┘
```

### 6. Frontend State Management

| State | Mechanism | Details |
|---|---|---|
| **View mode** | `sessionStorage` | Card/Table toggle |
| **Filters, sort** | `nuqs` (`useQueryStates`) | URL-synced |
| **Section list** | TanStack Query `useSuspenseQuery` | Keys include `collegeId`, `academicYearId`, `filters`, `sort` |
| **Stats** | TanStack Query `useSuspenseQuery` | Separate query, refetched after mutations |
| **Offering details** | TanStack Query (lazy, `enabled` flag) | Fetched on demand per section; cached |
| **Batch selection** | Local `Set<number>` state | Course-level selection only |
| **Metrics (progress %, counts)** | `useMemo` | Derived from `validationSummary` |

### 7. Key Metrics Calculation

| Metric | Source | Calculation |
|---|---|---|
| **Scheduling progress %** | `validationSummary` | `offeringsWithCompleteSchedule / totalOfferings * 100` |
| **Unresolved Errors count** | `validationSummary` | `offeringsWithErrors` |
| **Missing teacher count** | `validationSummary` | `missingTeacherCount` |
| **Missing room count** | `validationSummary` | `missingRoomCount` |
| **Missing schedule count** | `validationSummary` | `missingScheduleCount` |
| **Conflicts count** | `validationSummary` | `offeringsWithConflicts` |
| **Unscheduled count** | API stats endpoint | Sections with 0 complete offerings |
| **Schedule density** | Offering Details (lazy) | Aggregate day/time slot map |

All metrics are derived from backend-computed data — no client-side validation logic.

### 8. Desktop-Only Constraint

- Page is designed exclusively for 1024px+ screens
- No mobile/tablet responsive layout
- Users on smaller devices should use Room Scheduler or Section Detail pages
- Implement with a viewport check and banner: "This page is optimized for desktop. Use [Room Scheduler] or [Section Detail] on smaller screens."

### 9. Performance Strategy

| Strategy | Implementation |
|---|---|
| **Virtual scrolling** | Virtualize only College accordion headers as top-level rows; use `content-visibility: auto` on inner course blocks |
| **Lazy offering details** | Fetch only on interaction (hover, expand, badge click); cache per `sectionId` |
| **Mini calendar** | Static summary text on hover instantly; async load full grid in background |
| **Metrics memoization** | `useMemo` with `validationSummary` dependency; no re-traversal of offerings |
| **Filter debounce** | 300ms debounce on filter changes |
| **Query stale time** | 2-minute stale time for section list; refetch after mutations |
| **Bundle size** | Tree-shake unused chart libs; lazy-load mini calendar component |
