# DiceUI DataTable Migration Plan

## Problem
The current `DataTable` component (`components/data-table.tsx`, 188 lines) is a basic custom implementation with limited features: global search, column visibility toggle, and simple prev/next pagination. It lacks sortable columns, advanced filtering, per-column filters, page size selector, and URL state sync.

## Proposed Approach
Replace the existing DataTable with DiceUI's composable data table system (from [tablecn](https://github.com/sadmann7/tablecn)). This provides sortable column headers, advanced filtering (text/number/select/multi-select/date), URL-synced state via `nuqs`, pagination with page size selector, column pinning, and row selection — all built on the same `@tanstack/react-table` v8 already in use.

### Key Compatibility
- **@tanstack/react-table v8**: Already installed (v8.21.3) — same version DiceUI uses
- **@tanstack/react-router**: nuqs has an **experimental** `nuqs/adapters/tanstack-router` adapter
- **React 19, Tailwind v4, shadcn/ui**: All compatible
- **New dependency**: `nuqs` package

### Architecture
DiceUI's data table is a multi-file composable system:

```
components/data-table/
├── data-table.tsx              # Main table render (required)
├── data-table-column-header.tsx # Sortable column headers (optional)
├── data-table-pagination.tsx   # Pagination with page size (required)
├── data-table-toolbar.tsx      # Standard toolbar with basic filters (optional)
├── data-table-advanced-toolbar.tsx # Advanced filtering toolbar (optional)
├── data-table-filter-list.tsx  # Advanced filter list (optional)
├── data-table-sort-list.tsx    # Sort list popover (optional)
├── data-table-view-options.tsx # Column visibility toggle (optional)
config/
└── data-table.ts               # Filter operators config (required)
hooks/
└── use-data-table.ts           # Core hook managing URL state (required)
lib/
├── data-table.ts               # Utility functions (required)
└── parsers.ts                  # nuqs URL parsers (required)
types/
└── data-table.ts               # TypeScript types + module augmentation (required)
```

### Migration Strategy
**Incremental migration** — install DiceUI components alongside the old DataTable, migrate tables one at a time, then delete the old component. This avoids a risky big-bang rewrite.

## Current State (9 Tables)

| Table | Columns | Pagination | Custom Renderers | Actions |
|-------|---------|-----------|-----------------|---------|
| Courses | 10 | Client | truncateText | Edit, Delete |
| Colleges | 9 | Client | truncateText | Edit, Delete |
| Departments | 10 | Client | — | Edit, Delete |
| Buildings | 9 | Client | — | Edit, Delete |
| Subjects | 10 | **Server-side** | truncateText, toFixed | Edit, Delete |
| Rooms | 9 | Client | — | Edit, Delete |
| Room Types | 7 | Client | — | Edit, Delete |
| Teachers | 12 | Client | Avatar, Badges | Edit, Delete |
| Curriculums | 11 | Client | truncateText | Edit only |

All use `createColumnHelper<T>()`, `useReactTable()`, and pass `table` + `columns` to DataTable.

## Todos

### Phase 0 — Dependencies & Setup
1. **install-nuqs** — Install `nuqs` package
2. **nuqs-adapter** — Add `NuqsAdapter` from `nuqs/adapters/tanstack-router` in `__root.tsx` wrapping `<Outlet />`

### Phase 1 — Install DiceUI Components
3. **dt-types** — Create `types/data-table.ts` with TypeScript types and `@tanstack/react-table` module augmentation for column meta (label, variant, options, etc.)
4. **dt-config** — Create `config/data-table.ts` with filter operators config (text, numeric, date, select, multiSelect, boolean operators)
5. **dt-lib-parsers** — Create `lib/parsers.ts` with nuqs URL parsers for sorting and filter state
6. **dt-lib-utils** — Create `lib/data-table.ts` with utility functions (getColumnPinningStyle, getFilterOperators, getValidFilters)
7. **dt-hook** — Create `hooks/use-data-table.ts` — core hook managing pagination/sorting/filtering via nuqs URL state
8. **dt-component** — Create `components/data-table/data-table.tsx` — new DataTable component (replaces old one)
9. **dt-pagination** — Create `components/data-table/data-table-pagination.tsx` — pagination with page size selector
10. **dt-column-header** — Create `components/data-table/data-table-column-header.tsx` — sortable/hideable column headers
11. **dt-toolbar** — Create `components/data-table/data-table-toolbar.tsx` — toolbar with search, filters, view options
12. **dt-view-options** — Create `components/data-table/data-table-view-options.tsx` — column visibility popover

### Phase 2 — Migrate Tables (one at a time)
13. **migrate-room-types** — Migrate room types table (simplest: 7 cols, no custom renderers) — serves as reference implementation
14. **migrate-buildings** — Migrate buildings table
15. **migrate-colleges** — Migrate colleges table
16. **migrate-rooms** — Migrate rooms table
17. **migrate-departments** — Migrate departments table
18. **migrate-courses** — Migrate courses table
19. **migrate-curriculums** — Migrate curriculums table (edit only, no delete)
20. **migrate-subjects** — Migrate subjects table (server-side pagination — needs special handling)
21. **migrate-teachers** — Migrate teachers table (most complex: avatars, badges, 12 cols)

### Phase 3 — Cleanup & Verification
22. **delete-old-dt** — Delete old `components/data-table.tsx` after all tables migrated
23. **build-verify-dt** — Run full build verification
24. **commit-dt** — Commit all changes

## Notes
- nuqs TanStack Router adapter is **experimental** — doesn't cover TanStack Start, and complex serialization types have limitations. For our use case (pagination + basic sorting), this is fine.
- The `useDataTable` hook always enables `manualPagination: true`. For client-side tables, pass `pageCount: Math.ceil(data.length / pageSize)` and provide the full dataset — the hook handles slicing internally.
- Subjects table currently uses URL params for server-side pagination — this maps naturally to nuqs's URL state management, potentially simplifying that code.
- Each table migration follows the pattern: (1) add `id` + `meta` to columns, (2) swap `useReactTable` → `useDataTable`, (3) swap old `<DataTable>` → new `<DataTable>` with toolbar, (4) use `DataTableColumnHeader` for sortable headers.
- Teachers table has rich custom cell renderers (Avatar, Badge grid) — these carry over unchanged since DiceUI uses the same `cell` render pattern.
- Curriculums table uses a different permission pattern (`useAuthorization().checkPolicy()` vs `useTablePermissions()`) — preserve as-is during migration.
