# UI/UX Improvement Plan — Enrollify Frontend

> **Status: ✅ IMPLEMENTED** — All 16 improvement items have been completed and TypeScript compiles cleanly.

## 1. Executive Summary

Enrollify is a well-architected React/TypeScript enrollment system with strong foundational patterns — OpenAPI-driven type safety, policy-based authorization, Zod schema composition, and factory-based TanStack Query options. However, the codebase suffers from **significant code duplication (~5,950+ lines)**, **inconsistent authorization enforcement**, **accessibility gaps**, and **hardcoded values** that undermine scalability and UX polish.

**Key Strengths:**
- Excellent type inference from OpenAPI-generated types (9/10)
- Sophisticated policy-based authorization with extensible handler pattern
- Well-structured Zod schema composition with reusable primitives (`AuditInfoSchema`, `pagedResultSchema`, `dateTransformer`)
- Consistent three-layer page architecture (state → data → layout)
- Factory-pattern API collections with automatic cache invalidation

**Critical Gaps:**
- ~5,950 lines of duplicated code across form fields, authorization checks, delete dialogs, and table scaffolding
- Teachers module is incomplete (no API mutations, no authorization checks, uses dummy data)
- Zero accessibility support on interactive components (searchable selects, data table, loader)
- Three nearly-identical searchable-select variants sharing ~80% code
- No error boundaries, inconsistent loading states, no retry logic
- Hardcoded strings, spacing values, and navigation data throughout

**Overall Score: 7.9/10** — production-capable but needs systematic deduplication and polish to reach SaaS-quality standards.

---

## 2. Current State Assessment

### 2.1 Overall Architecture

The frontend follows a clean layered architecture:

```
src/
├── api/                    # Data layer
│   ├── collections/        # 11 TanStack Query option factories
│   ├── models/             # 16 Zod schemas with composition
│   ├── generated/          # OpenAPI-generated types
│   └── api-dtos/           # User context DTO
├── components/             # Shared UI components
│   ├── ui/                 # 25 Shadcn/ui primitives
│   └── theming/            # Theme provider + toggle
├── hooks/                  # Query/mutation factories + utilities
├── infrastructure/         # Auth, authorization, config, settings
├── lib/                    # Utilities (axios, date, text, obfuscator)
├── page-components/        # 8 management pages + curriculum builder
├── routes/                 # TanStack Router file-based routing
└── types/                  # Route type definitions
```

**Stack:** React 19, TypeScript 5.9, TanStack Router/Query/Table/Form, Shadcn/ui, TailwindCSS v4, Zod v4, Azure AD (MSAL), Axios.

### 2.2 Design Pattern Strengths

1. **OpenAPI → TypeScript type inference pipeline** eliminates manual DTO maintenance
2. **Factory-based query/mutation options** (`createQueryOptions`, `createMutationOptions`) provide consistent API layer
3. **Policy-based authorization** with requirement handlers mirrors ASP.NET Core's authorization model
4. **Zod schema composition** (`AuditInfoSchema.shape`, `SubjectSchema.pick(...)`) prevents model duplication
5. **Automatic cache invalidation** via mutation meta + `MutationCache.onSettled`
6. **Route-level authorization + data prefetching** in `beforeLoad` hooks

### 2.3 Notable Weaknesses

1. **Massive code duplication**: Form field wrappers (~4,800 lines), authorization state management (~350 lines), delete dialogs (~200 lines), table scaffolding (~120 lines)
2. **Inconsistent authorization**: Teachers table has ZERO authorization checks; Rooms delete dialog has commented-out loader; Subject form missing update policy check
3. **Three searchable-select variants** with ~80% identical code (ResizeObserver, Popover, CommandList)
4. **No error boundaries** or graceful failure handling anywhere in the app
5. **Hardcoded navigation data** (100+ lines in `app-sidebar.tsx`) — not configurable or feature-flag-friendly
6. **Token fetched on every API call** — no caching or refresh-on-expiry optimization
7. **Column definitions recreated every render** across all 8 tables — no `useMemo`
8. **Tab selection not persisted to URL** — lost on page reload (Subjects, Rooms pages)
9. **Mixed data fetching patterns** — some routes prefetch, some pages fetch lazily, some use `useSuspenseQuery`, some use `useQuery`
10. **Placeholder/dead code** — nav-user has non-functional "Upgrade to Pro", "Billing" items; `teacher-dummy-data.ts` exists

---

## 3. Component-Level Findings

### 3.1 Tables

**Files analyzed:** 8 table components across all management pages + 1 shared `DataTable`.

**Pattern:** All tables use `@tanstack/react-table` with `createColumnHelper<T>()`, render through shared `DataTable`, and accept `data`, `onEdit`, `onDelete` props.

**Findings:**

| Issue | Impact | Files Affected |
|-------|--------|----------------|
| **Authorization missing on Teachers** | Security risk — edit/delete buttons shown to all users | `teachers-table.tsx` |
| **Authorization logic duplicated** (~50 lines × 7 tables) | 350 lines of identical `useEffect` + `useState` + `Promise.all(checkPolicy(...))` | All tables except Teachers |
| **Column definitions not memoized** | Table re-initializes on every render; performance degrades with large datasets | All 8 tables |
| **Inconsistent prop types** | Teachers uses `Teacher[]`, Colleges uses `College[]` optional, Subjects uses `PagedResult<Subject>` | All tables |
| **Hardcoded action buttons** | Every table re-implements edit/delete buttons with identical markup | All 8 tables |
| **No loading skeleton** | Tables show empty or flash content during data fetch | All tables |
| **`DataTable` hardcoded strings** | "Search...", "Columns", "No results found." not configurable | `data-table.tsx` |
| **`DataTable` column header uses `column.id`** | Shows technical IDs like "firstName" instead of "First Name" | `data-table.tsx` |
| **`DataTable` missing ARIA** | No `aria-label` on search, no `aria-haspopup` on columns dropdown, no `aria-selected` on rows | `data-table.tsx` |
| **Mixed pagination modes** | Manual (Subjects) vs automatic (others) coexist with complex ternary logic | `data-table.tsx` |

### 3.2 Forms

**Files analyzed:** 8 form drawer components.

**Pattern:** Right-side `Drawer` with `@tanstack/react-form`, Zod validation, `FormMeta` submit pattern.

**Findings:**

| Issue | Impact | Files Affected |
|-------|--------|----------------|
| **Form field wrapper duplicated** (~40-50 lines per field × 5-15 fields × 8 forms) | ~4,800 lines of identical `<div><Label/><Input/>{errors}</div>` code | All 8 forms |
| **Inconsistent validation timing** | Subjects uses `onSubmit` validation; all others use `onChange` | `subject-form-drawer.tsx` |
| **Teachers form has no API mutation** | Form submits but no actual API call — parent handles (inconsistent with all other forms) | `teacher-form-drawer.tsx` |
| **Missing authorization in some forms** | Teachers form has no policy check; Subject form missing update policy | `teacher-form-drawer.tsx`, `subject-form-drawer.tsx` |
| **Error display inconsistent** | Teachers shows `text-red-800 text-sm`; others show `text-red-800` without `text-sm` | Various forms |
| **`max-w-sm` inconsistency** | Some field wrappers use `max-w-sm`, others use full width | Various forms |
| **FormMeta pattern repeated** | ~30 lines of identical form meta setup in every form | All 8 forms |
| **No field-level component abstraction** | Every form manually constructs Label + Input + error display | All 8 forms |

### 3.3 Layout & Page Composition

**Files analyzed:** 8 page index files, 3 tab components, root/portal routes.

**Pattern:** All pages follow identical three-section layout: Header (h1 + description) → Toolbar (Add button) → Content (Table/Tabs) → Dialogs.

**Findings:**

| Issue | Impact | Files Affected |
|-------|--------|----------------|
| **Page layout boilerplate duplicated** | ~200 lines repeated: header markup, OverlayLoader, outer wrapper | All 8 page index files |
| **State management boilerplate duplicated** | `isFormOpen`, `entityToEdit`, `entityToDelete` + 6 handlers repeated identically | All 8 page index files |
| **Inconsistent loading indicators** | Buildings/Colleges use `OverlayLoader`; Teachers uses none; all wrapped in Suspense | Various pages |
| **No error boundaries** | Any component error crashes entire page with no recovery UI | All pages |
| **Tab selection not in URL** | Lost on reload; no deep-linking support | Subjects, Rooms pages |
| **Hardcoded page titles/descriptions** | Cannot be driven by configuration or i18n | All pages |
| **Teachers page uses dummy data** | `teacher-dummy-data.ts` with hardcoded teachers; no real API integration | Teachers page |
| **Inconsistent data fetching** | Some routes prefetch in `beforeLoad`, some pages fetch lazily | Various pages |
| **`container-fluid` class** | Non-standard TailwindCSS class used in `page-header.tsx` and `page-content-container.tsx` | Layout components |

### 3.4 Shared Components & Primitives

**Files analyzed:** 13 shared components + 25 Shadcn/ui primitives.

**Findings:**

| Issue | Impact | Files Affected |
|-------|--------|----------------|
| **Three searchable-select variants with ~80% code duplication** | ResizeObserver logic, Popover setup, CommandList rendering all duplicated | `searchable-select.tsx`, `multi-searchable-select.tsx`, `searchable-select-with-custom-trigger.tsx` |
| **Loader component duplicates OverlayLoader** | Two loader implementations with different APIs | `loader.tsx`, `app-loading-overlay.tsx` |
| **Hardcoded navigation data** | 100+ lines of routes/icons in `app-sidebar.tsx`; not configurable | `app-sidebar.tsx` |
| **Avatar fallback "CN" hardcoded** | Should derive initials from user's name | `app-sidebar.tsx`, `nav-user.tsx` |
| **Nav-user placeholder items** | "Upgrade to Pro", "Billing", "Account" etc. are non-functional | `nav-user.tsx` |
| **`page-header.tsx` not customizable** | Hardcoded `z-10`, `h-16`, `py-4`; no className pass-through | `page-header.tsx` |
| **`page-content-container.tsx` inconsistent padding** | `pt-0` with `py-6` creates asymmetric spacing | `page-content-container.tsx` |
| **Layout components missing className prop** | Cannot be customized per-page without editing component | `app-container.tsx`, `page-header.tsx` |
| **All searchable selects missing ARIA** | No `role="combobox"`, no `aria-expanded`, no `aria-haspopup="listbox"`, no keyboard navigation | All 3 select components |
| **Loader missing accessibility** | No `role="status"`, no `aria-live`, no sr-only text | `loader.tsx` |

### 3.5 Hooks & State Management

**Files analyzed:** 4 hook files, 11 collection files, 16 model files, auth/authorization infrastructure.

**Findings:**

| Issue | Impact | Files Affected |
|-------|--------|----------------|
| **Token fetched on every API call** | Network overhead; unnecessary MSAL calls | `tokenFetcher.ts`, `create-query-options.ts`, `create-mutation-options.ts` |
| **No retry logic** | Transient failures (network timeouts) fail immediately | Query/mutation factories |
| **Authorization results not cached** | Same policy checked repeatedly across components per render | `AuthorizationService.ts`, all tables |
| **`checkPolicy` reference instability** | `useEffect` dependency may re-trigger on every parent render | All components using `useAuthorization` |
| **Subject search handles string fallback** | Lines 94-110 in `subject-collection.ts` parse string responses — backend contract unclear | `subject-collection.ts` |
| **Teacher model not using Zod** | Inconsistent with all other models that use Zod schemas | `teacher.ts` |
| **Query key factories use strings** | Prone to typos; no compile-time safety | All collection files |
| **`as any` in auth-config** | Type bypass on MSAL LogLevel | `auth-config.ts` |
| **Router context typed as `{}`** | Runtime access unsafe; should use `Partial<T>` | `main.tsx` |
| **No per-mutation error handler override** | Global-only error handling via MutationCache | `main.tsx` |
| **Stale time inconsistency** | Different collections use different stale times without clear rationale | Various collections |

---

## 4. UI/UX Enhancement Recommendations

### 4.1 Table Design Standards

**Goal:** Supabase-style data tables — clean, dense, scannable.

**A. Create `useTablePermissions` hook:**
```typescript
// hooks/use-table-permissions.ts
export function useTablePermissions(entityName: string) {
  const { checkPolicy } = useAuthorization();
  const [permissions, setPermissions] = useState({ canUpdate: false, canDelete: false });

  useEffect(() => {
    let mounted = true;
    Promise.all([
      checkPolicy(`canUpdate${entityName}`),
      checkPolicy(`canDelete${entityName}`),
    ]).then(([canUpdate, canDelete]) => {
      if (mounted) setPermissions({ canUpdate, canDelete });
    });
    return () => { mounted = false; };
  }, [checkPolicy, entityName]);

  return permissions;
}
```
**Impact:** Eliminates ~350 lines of duplicated authorization state management.

**B. Memoize column definitions in all tables:**
```typescript
const columns = useMemo(() => [
  columnHelper.accessor("code", { header: "Code" }),
  // ...
], [canUpdate, canDelete, onEdit, onDelete]);
```
**Impact:** Prevents table re-initialization on every render.

**C. Enhance `DataTable` with configurable strings and ARIA:**
- Add `searchPlaceholder`, `noResultsMessage`, `columnsButtonLabel` props
- Add `aria-label` to search input, `aria-haspopup` to columns dropdown
- Add `aria-selected` to selected rows
- Fix column header display to use `column.columnDef.header` instead of `column.id`

**D. Add skeleton loading states** to replace flash of empty content.

### 4.2 Form Design Standards

**Goal:** Eliminate form field boilerplate; consistent validation UX.

**A. Create `FormField` wrapper component:**
```typescript
// components/form-field.tsx
interface FormFieldProps {
  field: FieldApi<any, any, any, any>;
  label: string;
  type?: "text" | "number" | "textarea";
  placeholder?: string;
  className?: string;
}

export function FormField({ field, label, type = "text", placeholder, className }: FormFieldProps) {
  return (
    <div className={cn("grid w-full items-center gap-3", className)}>
      <Label htmlFor={field.name}>{label}</Label>
      <Input
        type={type}
        id={field.name}
        placeholder={placeholder ?? label}
        value={field.state.value}
        onChange={(e) => field.handleChange(type === "number" ? Number(e.target.value) : e.target.value)}
      />
      {!field.state.meta.isValid && (
        <em role="alert" className="text-destructive text-sm">
          {field.state.meta.errors.map((e) => e?.message).join(", ")}
        </em>
      )}
    </div>
  );
}
```
**Impact:** Eliminates ~4,800 lines of duplicated field markup. Each form drops from ~150-250 lines to ~50-100 lines.

**B. Standardize validation timing** to `onChange` across all forms (fix Subjects form).

**C. Standardize error text styling** to `text-destructive text-sm` (unify inconsistent `text-red-800`).

### 4.3 Page Layout Structure

**Goal:** Zero boilerplate CRUD pages; consistent layout primitives.

**A. Create `ManagementPageLayout` component:**
```typescript
// components/management-page-layout.tsx
interface ManagementPageLayoutProps {
  title: string;
  description: string;
  isLoading?: boolean;
  children: React.ReactNode;
}

export function ManagementPageLayout({ title, description, isLoading, children }: ManagementPageLayoutProps) {
  return (
    <main>
      <OverlayLoader isLoading={isLoading} text="Loading" size="sm" />
      <div className="p-4 md:p-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-foreground mb-2">{title}</h1>
          <p className="text-muted-foreground">{description}</p>
        </div>
        {children}
      </div>
    </main>
  );
}
```

**B. Create `useCrudState<T>()` hook:**
```typescript
// hooks/use-crud-state.ts
export function useCrudState<T>() {
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [entityToEdit, setEntityToEdit] = useState<T | undefined>();
  const [entityToDelete, setEntityToDelete] = useState<T | undefined>();

  const handleEdit = (entity: T) => { setEntityToEdit(entity); setIsFormOpen(true); };
  const handleDelete = (entity: T) => { setEntityToDelete(entity); };
  const handleFormOpenChange = (open: boolean) => { if (!open) setEntityToEdit(undefined); setIsFormOpen(open); };
  const handleDeleteOpenChange = (open: boolean) => { if (!open) setEntityToDelete(undefined); };

  return { isFormOpen, entityToEdit, entityToDelete, handleEdit, handleDelete, handleFormOpenChange, handleDeleteOpenChange };
}
```
**Impact:** Reduces each page by ~30-40 lines of state management boilerplate.

**C. Create generic `DeleteAlertDialog<T>` component** to replace 8 near-identical delete dialogs.

**D. Add `ErrorBoundary` component** wrapping all page content in portal route layout.

### 4.4 Design System Tokens & Patterns

**Goal:** Consistent spacing, typography, and interactive states aligned with Supabase aesthetic.

**A. Extract navigation data** from `app-sidebar.tsx` into a `navigation-config.ts` file.

**B. Fix avatar fallback** — derive initials from user's name instead of hardcoded "CN".

**C. Remove placeholder nav-user items** ("Upgrade to Pro", "Billing", etc.) or implement them.

**D. Consolidate three searchable-select components** into a composable base:
1. Extract `useElementWidth()` hook (shared ResizeObserver logic)
2. Create `SearchableSelectBase` with render prop for trigger customization
3. Build `SearchableSelect`, `MultiSearchableSelect`, and `CustomTriggerSelect` as thin wrappers
4. Add full ARIA support (`role="combobox"`, `aria-expanded`, `aria-haspopup="listbox"`)

**E. Unify loader components** — make `Loader` accept size/variant props; have `OverlayLoader` compose it.

**F. Make layout components customizable** — add `className` prop to `app-container.tsx`, `page-header.tsx`, `page-content-container.tsx`.

---

## 5. Implementation Guidance

### 5.1 Deduplication Architecture

The recommended abstractions form a component hierarchy:

```
ManagementPageLayout          ← New shared layout wrapper
├── useCrudState<T>()         ← New hook for form/delete state
├── useTablePermissions()     ← New hook for authorization state
├── FormField                 ← New shared form field component
├── DeleteAlertDialog<T>      ← New generic delete dialog
├── DataTable (enhanced)      ← Existing, with ARIA + configurable strings
└── SearchableSelectBase      ← New composable select base
    ├── SearchableSelect      ← Thin wrapper
    ├── MultiSearchableSelect ← Thin wrapper
    └── CustomTriggerSelect   ← Thin wrapper
```

### 5.2 Authorization Caching

Add memoization to `AuthorizationService.authorize()`:

```typescript
// Cache results per policy+context for 30 seconds
private cache = new Map<string, { result: AuthorizationResult; timestamp: number }>();
private TTL = 30_000;

async authorize(policyName: PolicyName, context: AuthorizationEvaluationContext): Promise<AuthorizationResult> {
  const key = `${policyName}:${JSON.stringify(context)}`;
  const cached = this.cache.get(key);
  if (cached && Date.now() - cached.timestamp < this.TTL) return cached.result;

  const result = await this.evaluatePolicy(policy, context);
  this.cache.set(key, { result, timestamp: Date.now() });
  return result;
}
```

### 5.3 Token Caching

Implement token cache with refresh-on-expiry in `tokenFetcher.ts`:

```typescript
let cachedToken: string | null = null;
let expiresAt = 0;

async function getCurrentAccessToken({ msalInstance, forceRefreshToken }) {
  if (cachedToken && Date.now() < expiresAt && !forceRefreshToken) return cachedToken;
  const response = await msalInstance.acquireTokenSilent(request);
  cachedToken = response.accessToken;
  expiresAt = response.expiresOn.getTime() - 60_000; // Refresh 1 min early
  return cachedToken;
}
```

### 5.4 Error Boundary

```typescript
// components/page-error-boundary.tsx
export function PageErrorBoundary({ error, reset }: { error: Error; reset: () => void }) {
  return (
    <div className="flex min-h-[400px] items-center justify-center">
      <Card className="max-w-md text-center">
        <CardHeader>
          <AlertCircle className="mx-auto h-12 w-12 text-destructive" />
          <CardTitle>Something went wrong</CardTitle>
          <CardDescription>{error.message}</CardDescription>
        </CardHeader>
        <CardFooter className="justify-center">
          <Button onClick={reset}>Try Again</Button>
        </CardFooter>
      </Card>
    </div>
  );
}
```

Register in TanStack Router routes via `errorComponent: PageErrorBoundary`.

### 5.5 Tab URL Persistence

```typescript
// In subjects-management-page/index.tsx
const navigate = useNavigate();
const { tab = "subjects" } = useSearch({ from: Route.fullPath });

<Tabs value={tab} onValueChange={(newTab) =>
  navigate({ search: { tab: newTab }, replace: true })
}>
```

---

## 6. Prioritized Improvement Roadmap

| Priority | Area | Action | Effort | Impact |
|----------|------|--------|--------|--------|
| P1 | Security | Add authorization checks to Teachers table | S | Fixes security gap |
| P1 | Security | Fix Teachers delete dialog — add mutation | S | Fixes non-functional delete |
| P1 | Security | Add update policy check to Subject form | S | Fixes missing auth |
| P1 | Security | Uncomment Rooms delete dialog OverlayLoader | S | Fixes inconsistent UX |
| P1 | Performance | Memoize column definitions in all 8 tables | S | Prevents re-renders |
| P1 | Architecture | Create `useTablePermissions` hook | S | Eliminates ~350 lines duplication |
| P1 | Architecture | Create `FormField` wrapper component | M | Eliminates ~4,800 lines duplication |
| P1 | Architecture | Create `useCrudState<T>()` hook | S | Eliminates ~300 lines duplication |
| P2 | Architecture | Create `ManagementPageLayout` component | S | Eliminates ~200 lines per page |
| P2 | Architecture | Create generic `DeleteAlertDialog<T>` | M | Replaces 8 near-identical files |
| P2 | Architecture | Consolidate 3 searchable-selects into composable base | M | Eliminates ~300 lines duplication |
| P2 | Architecture | Extract `useElementWidth` hook | S | Shared ResizeObserver logic |
| P2 | UX | Add `ErrorBoundary` component + route integration | S | Graceful failure handling |
| P2 | UX | Persist tab selection to URL (Subjects, Rooms) | S | State preserved across reloads |
| P2 | UX | Standardize loading states across all pages | M | Consistent loading UX |
| P2 | Performance | Add token caching in `tokenFetcher.ts` | S | Reduces MSAL calls |
| P2 | Performance | Add authorization result caching | S | Reduces redundant policy checks |
| P2 | Architecture | Extract navigation config from `app-sidebar.tsx` | S | Configurable nav |
| P2 | UX | Fix avatar fallback — derive user initials | S | Personalized UI |
| P2 | Code Quality | Remove placeholder nav-user items or implement them | S | Clean up dead code |
| P2 | Code Quality | Remove `teacher-dummy-data.ts` + implement real API | M | Complete Teachers module |
| P3 | Accessibility | Add ARIA to `DataTable` (search, columns, rows) | M | Screen reader support |
| P3 | Accessibility | Add ARIA to all searchable-select variants | M | Combobox accessibility |
| P3 | Accessibility | Add `role="status"` + sr-only to `Loader` | S | Loader accessibility |
| P3 | Accessibility | Add keyboard navigation to searchable selects | M | Keyboard-only users |
| P3 | Performance | Add retry logic with exponential backoff | S | Resilience |
| P3 | Architecture | Migrate Teacher model to Zod | S | Consistency |
| P3 | Architecture | Type-safe query key factories | M | Compile-time safety |
| P3 | UX | Add skeleton loaders for tables | M | Better perceived performance |
| P3 | UX | Standardize data fetching pattern (route prefetch vs page fetch) | M | Consistent loading behavior |
| P3 | Architecture | Make layout components accept `className` prop | S | Customizable per-page |
| P3 | Code Quality | Fix `container-fluid` non-standard Tailwind class | S | Standard CSS |
| P3 | Code Quality | Standardize form validation to `onChange` across all forms | S | Consistent behavior |

**Effort Key:** S = Small (< 1 session), M = Medium (1-2 sessions), L = Large (3+ sessions)

**Estimated total deduplication savings:** ~5,950 → ~1,500 lines (75% reduction)

---

## 7. Todos

The following todos track the implementation phases:

1. **security-quick-fixes** — P1 security fixes (Teachers auth, delete mutation, Subject auth, Rooms loader)
2. **table-performance** — Memoize column definitions across all 8 tables
3. **use-table-permissions-hook** — Extract `useTablePermissions` hook and refactor all tables
4. **form-field-component** — Create `FormField` wrapper and refactor all 8 forms
5. **use-crud-state-hook** — Create `useCrudState<T>()` hook and refactor all pages
6. **management-page-layout** — Create `ManagementPageLayout` and refactor all pages
7. **generic-delete-dialog** — Create `DeleteAlertDialog<T>` and replace 8 dialogs
8. **searchable-select-consolidation** — Consolidate 3 select components into composable base
9. **error-boundary** — Create `PageErrorBoundary` and integrate with routes
10. **tab-url-persistence** — Persist tab selection to URL on Subjects and Rooms pages
11. **loading-state-standardization** — Standardize loading indicators across all pages
12. **token-and-auth-caching** — Add token caching + authorization result caching
13. **navigation-config-extraction** — Extract nav data from sidebar + fix avatar fallback + cleanup
14. **accessibility-pass** — ARIA attributes on DataTable, searchable selects, Loader
15. **teachers-module-completion** — Remove dummy data, implement real API, add Zod model
16. **performance-resilience** — Retry logic, skeleton loaders, query key type safety
