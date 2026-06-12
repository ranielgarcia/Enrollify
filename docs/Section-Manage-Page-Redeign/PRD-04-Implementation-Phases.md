# Class Sections Management Page — PRD

## PRD-04: Implementation Phases

---

### Phase 1: Scaffold & Data Layer (2-3 days)

**Goal:** New route, new files, backend API extensions for single-college scheduling, and query/mutation infrastructure.

| Task | Deliverable | Dependencies |
|---|---|---|
| Create new route `/scheduling/sections` with TanStack Router file | `src/routes/scheduling.sections.tsx` | None |
| Copy over `searchParams.ts` pattern with `collegeId`, `view`, `filters`, `sort` params | `src/page-components/sections-management-page-v2/searchParams.ts` | None |
| Create/extend backend college list endpoint for selector | Backend: `GET /api/colleges` | None |
| Create backend college-filtered sections endpoint (full list) | Backend: `GET /api/colleges/{collegeId}/class-sections` | None |
| Extend backend list response to include `validationSummary` per section | Backend: college-filtered list query | None |
| Create backend college-scoped aggregate stats endpoint | Backend: `GET /api/colleges/{collegeId}/class-sections/stats` | None |
| Create backend offering details lazy endpoint | Backend: `GET /api/class-sections/{sectionId}/offerings` | None |
| Create backend bulk operation endpoints (open, cancel, assign-adviser) | Backend: 3 POST endpoints | None |
| Update Draft → Open to block on Error-severity conflicts | Backend: `OpenClassSectionForEnrollment` | None |
| Implement cancel cleanup so cancelled data no longer participates in conflicts | Backend: cancel handler + conflict detection query filters | None |
| Update frontend API collection (`class-section-collection.ts`) with new queries/mutations | `src/api/collections/class-section-collection-v2.ts` | Backend endpoints |
| Regenerate API types (`npm run generate:api:win`) | `src/api/generated/api.ts` | Backend endpoints |
| Create page scaffold (`index.tsx`) with `useSuspenseQuery` for sections + stats + college selector | `index.tsx` | API collection |

### Phase 2: Card View & College Selector (2-3 days)

**Goal:** Card view with course grouping for the selected college, college selector, section cards, progress display.

| Task | Deliverable | Dependencies |
|---|---|---|
| Build `CollegeSelector` component (dropdown with college list) | `college-selector.tsx` | Phase 1 API |
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
| Build course-level `BatchActionsToolbar` (inside each expanded course group) | `batch-actions-toolbar.tsx` | Selection state |
| Build `BulkStatusTransitionDialog` with count summary + "X offerings will be freed" | `bulk-status-transition-dialog.tsx` | Phase 1 bulk endpoints |
| Build `BulkAdviserAssignDrawer` | `bulk-adviser-assign-drawer.tsx` | Phase 1 bulk endpoints |
| Add course-level group toolbar with scoped batch actions | `sections-card-view.tsx` (CourseGroupToolbar) | Phase 2 |
| Disable batch buttons when non-Draft sections are selected (with tooltip) | `batch-actions-toolbar.tsx` | Selection state |

### Phase 5: Conflict & Schedule Peek (2-3 days)

**Goal:** Mini calendar tooltip, conflict preview drawer, lazy offering loading.

| Task | Deliverable | Dependencies |
|---|---|---|
| Build `ConflictPreviewDrawer` | `conflict-preview-drawer.tsx` | Phase 1 offering details endpoint |
| Build `SectionCardMiniSchedule` — static summary on hover + async full grid load | `section-card-mini-schedule.tsx` | Phase 1 offering details endpoint |
| Wire lazy offering fetch into card hover/expand interaction | `section-card.tsx` → `section-card-mini-schedule.tsx` | Phase 1 |
| Make compound badge clickable → opens conflict preview drawer | `section-card.tsx`, `sections-table.tsx` | `ConflictPreviewDrawer` |

### Phase 6: Empty State, Polish & Performance (1-2 days)

**Goal:** Empty states, edge cases, performance optimization, accessibility.

| Task | Deliverable | Dependencies |
|---|---|---|
| Build `EmptyState` component (filtered + no-data variants) | `empty-state.tsx` | All phases |
| Wire empty state into card and table views | `sections-card-view.tsx`, `sections-table.tsx` | `EmptyState` |
| Add 300ms debounce on filter changes | `searchParams.ts` or `index.tsx` | None |
| Add `content-visibility: auto` to CourseGroup blocks | `sections-card-view.tsx` CSS | Phase 2 |
| Ensure course groups are collapsed by default for large colleges | `sections-card-view.tsx` | Phase 2 |
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

---

### Dependency Graph

```
Phase 1 (Scaffold & Single-College API)
    │
    ├──► Phase 2 (Card View & Selector) ──► Phase 4 (Batch Ops) ──► Phase 6 (Polish)
    │                                                         │
    └──► Phase 3 (Table View) ──► Phase 5 (Conflict) ────────┘
                                                              │
                                                              ▼
                                                      Phase 7 (Tests)
```

Phases 2 and 3 can be built in parallel after Phase 1 completes.
Phases 4 and 5 can be partially parallel once their dependencies are done.
Phase 6 (empty state, polish) should be last but can overlap with Phase 7.
