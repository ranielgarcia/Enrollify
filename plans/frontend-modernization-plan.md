# Frontend UI/UX Modernization Plan — Enrollify

> Comprehensive analysis and phased roadmap to transform Enrollify into a modern, polished SaaS application.

---

## 1. Executive Summary

### Overall Assessment

Enrollify's frontend is architecturally strong — React + TanStack Router + TanStack Query + Zod + Shadcn/UI form a solid, modern stack. The code is well-organized with clear separation of concerns, feature-based folders, and consistent patterns (query factories, policy-based auth, CRUD page scaffolding). However, the **visual design, interaction polish, and overall UX maturity lag significantly behind the technical foundation**. The application feels like a functional admin panel rather than a refined SaaS product. There is no dashboard, no data visualization, placeholder pages, missing empty/success states, and heavy CRUD scaffolding duplication across 11+ management pages.

### Current UI Maturity Score: **4.5 / 10**

### Key Strengths

- **Solid technical stack:** React 19, TanStack Router, TanStack Query, Zod, Tailwind v4, Shadcn/UI
- **Consistent architecture:** Every CRUD page follows the same three-layer pattern (route → page component → reusable primitives)
- **Dark mode support:** Full OKLCH-based light/dark theme via CSS variables
- **Authorization model:** Fine-grained policy-based access control wired into navigation and components
- **Data fetching:** Suspense-based queries with 24-hour cache, auto-invalidation, and persistent query client
- **Type safety:** Double validation (OpenAPI codegen + Zod runtime schemas)
- **Form handling:** TanStack Form with Zod validation, reusable form field components
- **Data tables:** Feature-rich with sorting, faceted filtering, pagination, column pinning, and skeleton loading

### Key Weaknesses

- **Home page is a placeholder** — renders only `<h1>Home page</h1>`
- **No dashboard or analytics** — zero data visualization, KPIs, or at-a-glance summaries
- **Massive code duplication** — ~5,950 lines of near-identical CRUD scaffolding across 11 pages
- **No empty states** — tables render blank when no data exists; no guidance or CTAs
- **Inconsistent visual density** — pages feel like spreadsheets rather than designed interfaces
- **No success states** — mutations show toasts only; no in-context confirmation
- **Limited visual hierarchy** — flat layouts with no cards, sections, or progressive disclosure
- **Accessibility gaps** — missing aria-sort, skip-to-content, loading announcements, icon button labels
- **No responsive typography** — fixed font sizes regardless of viewport
- **Three different searchable-select variants** — duplicated logic with inconsistent APIs

---

## 2. UX Audit

### 2.1 Home Page (`/portal/home`)

**Current state:** Renders `<h1>Home page</h1>` — completely empty.

**Issues:**
- Zero utility — users land here and must immediately navigate away
- No orientation or context for first-time users
- No system-wide status, recent activity, or shortcuts
- Wasted screen real estate

**Recommended improvements:**
- Build a full **dashboard** with KPI cards (total students, sections, courses, teachers)
- Add **quick-action tiles** (Create Section, View Schedule, Manage Buildings)
- Show **enrollment timeline** widget with current academic year/term context
- Display **recent activity feed** (last created/updated records)
- Add **system health indicators** (incomplete sections, rooms without schedules)
- Include a **getting started checklist** for first-time setup

**Expected impact:** 🔴 Critical — transforms the first impression and daily landing experience.

---

### 2.2 Management Pages (Buildings, Colleges, Departments, Courses, Rooms, Subjects, Teachers)

**Current state:** All 7+ management pages share an identical layout: page title + description + "Create" button → data table → form drawer → delete dialog. Each is a standalone copy of this scaffolding.

**Issues:**
- **Visual monotony** — every page looks identical; no contextual differentiation
- **No summary metrics** — user can't glance at total count, recent additions, or status breakdowns
- **No empty states** — new installations show blank tables with no guidance
- **Dense data tables** — 9+ columns on buildings; overwhelming on first visit
- **Action discoverability** — edit/delete buttons buried in the last column
- **No bulk operations** — users must edit/delete items one at a time
- **Form drawers** are narrow (default `sm:max-w-sm`); complex forms feel cramped
- **Delete confirmation** is a generic alert dialog with no context about impact (e.g., "This building has 12 rooms")

**Recommended improvements:**
- Add **summary stat cards** above each table (total count, recently added, breakdown by category)
- Implement a **reusable EmptyState component** with illustration, message, and CTA button
- Reduce default visible columns; use **column visibility toggles** to let users customize
- Add **row hover actions** (edit/delete as icon buttons on hover) instead of a fixed actions column
- Support **bulk selection and operations** (multi-select → bulk delete, bulk export)
- Widen form drawers to `sm:max-w-lg` or `sm:max-w-xl` for complex forms
- Add **contextual delete warnings** showing dependent record counts
- Extract a **generic CrudPage component** to eliminate ~5,000 lines of duplication

**Expected impact:** 🔴 High — these are the most-used pages in the application.

---

### 2.3 Academic Year Management Page

**Current state:** Redesigned with hero card pattern, icon chips, status badges, and structured sections. This is the **most polished page** in the application.

**Issues:**
- Serves as an outlier — no other page matches this level of design
- The redesign pattern is not abstracted into reusable components
- Timeline visualization could be stronger (Gantt-style or calendar view)

**Recommended improvements:**
- **Extract the hero-card, icon-chip, and status-badge patterns** into shared components
- Apply this design language to all management pages as a baseline
- Add a **visual timeline** component for academic year periods
- Consider a **Kanban-style view** for term status management (Draft → Active → Completed)

**Expected impact:** 🟡 Medium — already well-designed; main value is in extracting reusable patterns.

---

### 2.4 Curriculum Builder Page

**Current state:** Complex page for managing curriculum structures (year levels, semesters, subjects). Uses nested data with tree-like relationships.

**Issues:**
- **High cognitive load** — users must mentally map year-level → semester → subject relationships
- **No drag-and-drop** — adding subjects requires form-based input for each
- **No visual curriculum map** — no at-a-glance view of the full curriculum structure
- **Dense information** — all data presented in flat lists

**Recommended improvements:**
- Implement a **visual curriculum map** (grid/matrix view: rows = year levels, columns = semesters, cells = subjects)
- Add **drag-and-drop** for reordering subjects within semesters
- Use **progressive disclosure** — collapse year levels by default, expand on click
- Add **curriculum completeness indicator** (progress bar showing required vs. assigned units)
- Implement **subject search-and-add** inline (type-ahead to add subjects directly)

**Expected impact:** 🔴 High — this is the most complex workflow in the application.

---

### 2.5 Section Detail Page

**Current state:** Shows section information with a weekly schedule grid. Most complex page with scheduling UI including time slots, day-of-week columns, and room/teacher assignments.

**Issues:**
- **Schedule grid is plain** — uses a basic HTML table with minimal styling
- **No visual time blocks** — schedule entries are text-only, not visual blocks
- **Conflicts not highlighted** — room/teacher scheduling conflicts are not visually indicated
- **No drag-to-schedule** — adding schedule entries requires form input

**Recommended improvements:**
- Implement a **visual weekly calendar** (Google Calendar-style with colored time blocks)
- Add **drag-and-drop scheduling** for time block creation and movement
- Implement **conflict detection visualization** (red highlights for overlapping schedules)
- Add a **mini-map sidebar** showing the full week at a glance
- Use **color coding by subject** for quick visual scanning

**Expected impact:** 🔴 High — scheduling is a core workflow and benefits most from visual interaction.

---

### 2.6 Sections Management Page

**Current state:** Data table listing all class sections with filtering by course, academic term, and year level.

**Issues:**
- **Flat table view** — sections are listed linearly without grouping
- **No schedule preview** — user must navigate to detail page to see any schedule information
- **No capacity visualization** — enrollment count vs. capacity not visually represented
- **No status indicators** — sections don't show whether they're fully scheduled, partially scheduled, or empty

**Recommended improvements:**
- Add **grouping by course** with collapsible sections
- Show **inline schedule preview** (compact weekly grid in expanded row)
- Add **capacity bar** (progress bar showing enrolled/capacity)
- Implement **status badges** (Fully Scheduled, Needs Teachers, Needs Rooms, Empty)
- Add **quick-assign actions** (assign teacher, assign room from the list view)

**Expected impact:** 🟡 Medium — improves workflow efficiency for registrars.

---

### 2.7 Teachers Management Page

**Current state:** Partially implemented — has the table and data display but mutations (create, edit, delete) are incomplete.

**Issues:**
- **Incomplete functionality** — CRUD operations not fully wired
- **No teacher profile view** — just tabular data
- **No schedule visibility** — can't see teacher's assigned sections/load

**Recommended improvements:**
- Complete CRUD functionality
- Add **teacher profile card** with schedule overview
- Show **teaching load summary** (units assigned vs. maximum)
- Add **availability grid** (weekly time block availability for scheduling)

**Expected impact:** 🔴 High — incomplete module blocks core workflow.

---

### 2.8 Login Page

**Current state:** Uses MSAL (Microsoft Authentication Library) redirect — minimal custom UI.

**Issues:**
- **No branded login experience** — relies entirely on Microsoft's login UI
- **No pre-login landing page** — users see nothing before redirect

**Recommended improvements:**
- Create a **branded login page** with institution logo, tagline, and a "Sign in with Microsoft" button
- Add a **loading state** during auth redirect
- Show **enrollment-related imagery** or hero illustration
- Include **footer** with support links, privacy policy, terms

**Expected impact:** 🟡 Medium — first touchpoint for every user session.

---

## 3. Visual Design Audit

### 3.1 Design Inconsistencies

| Area | Issue | Severity |
|---|---|---|
| **Page titles** | `text-2xl font-bold text-slate-900` — uses hardcoded `slate-900` instead of `foreground` token; breaks in dark mode | 🔴 High |
| **Button variants** | Some pages use `variant="outline"` for create, others use `variant="default"` | 🟡 Medium |
| **Spacing** | Management pages use `px-4 lg:px-6`; other pages use `p-4 md:p-8`; no consistent system | 🟡 Medium |
| **Icon sizing** | Page header icons are `h-12 w-12`; nav icons are `h-4 w-4`; sidebar icons vary | 🟠 Low |
| **Card usage** | Academic year page uses cards; all other pages use flat layouts — inconsistent information hierarchy | 🔴 High |
| **Color application** | Primary purple used for both interactive (buttons) and decorative (icons) — no semantic distinction | 🟡 Medium |
| **Table density** | Some tables show 9 columns; others show 6 — no consistent column count strategy | 🟡 Medium |
| **Form layout** | All forms stack vertically in narrow drawers; no multi-column layouts for related fields | 🟡 Medium |

### 3.2 Typography Issues

- **No typographic scale** — sizes are ad-hoc (`text-sm`, `text-2xl`, `text-3xl`) with no defined hierarchy
- **No responsive scaling** — all text sizes are fixed regardless of viewport width
- **Body text defaults** — no explicit line-height or letter-spacing tuning for readability
- **Table text** — uses `text-sm` universally; header differentiation is only through `font-medium`
- **Monospace usage** — numeric data (IDs, capacities) not using tabular or monospace figures
- **Missing heading levels** — pages jump from `h2` to body text; no `h3` or `h4` for subsections

### 3.3 Color Issues

- **Monochromatic purple** — primary, accent, ring, chart colors are all purple hue (~293° OKLCH); lacks warmth and variety
- **No semantic color system** — no `success`, `warning`, `info` tokens; only `destructive` exists
- **Muted foreground contrast** — `oklch(0.552 ...)` may fail WCAG AA against white backgrounds
- **Chart colors** — 5 chart tokens are all shades of purple; indistinguishable for color-blind users
- **No surface elevation** — cards, popovers, and modals use the same background color; no shadow/elevation hierarchy
- **Sidebar** — nearly identical to main content background; no visual separation

### 3.4 Layout Issues

- **No max-width constraint** — content stretches full-width on ultrawide monitors (2560px+)
- **Data tables dominate** — tables fill 90%+ of every management page with no visual breathing room
- **No section grouping** — related content is not visually grouped into cards or panels
- **Header area** — title + button row is minimal; no room for stats, filters, or secondary actions
- **Footer absent** — no global footer with version info, help links, or status

---

## 4. Design System Proposal

### 4.1 Typography Scale

Adopt a modular scale (1.250 ratio — "Major Third") anchored at 16px base:

| Token | Size | Weight | Line Height | Usage |
|---|---|---|---|---|
| `display-lg` | 36px / 2.25rem | 700 | 1.2 | Dashboard hero numbers |
| `display-sm` | 30px / 1.875rem | 700 | 1.2 | Page titles |
| `heading-lg` | 24px / 1.5rem | 600 | 1.3 | Section headings |
| `heading-sm` | 20px / 1.25rem | 600 | 1.3 | Card titles, dialog titles |
| `subheading` | 16px / 1rem | 600 | 1.4 | Table group headers |
| `body-lg` | 16px / 1rem | 400 | 1.5 | Primary body text |
| `body-sm` | 14px / 0.875rem | 400 | 1.5 | Secondary body text, table cells |
| `caption` | 12px / 0.75rem | 500 | 1.4 | Labels, metadata, timestamps |
| `overline` | 11px / 0.6875rem | 600 | 1.4 | Category labels, uppercase tags |

**Responsive behavior:**
- `display-lg`: 28px on mobile, 36px on desktop
- `display-sm`: 24px on mobile, 30px on desktop
- All others remain fixed

### 4.2 Spacing Scale

Adopt a 4px base grid with named tokens:

| Token | Value | Usage |
|---|---|---|
| `space-0` | 0px | Reset |
| `space-1` | 4px | Inline icon gaps |
| `space-2` | 8px | Compact padding, tag gaps |
| `space-3` | 12px | Input padding, small gaps |
| `space-4` | 16px | Standard padding, card gaps |
| `space-5` | 20px | Section gaps |
| `space-6` | 24px | Card padding, page section gaps |
| `space-8` | 32px | Major section separators |
| `space-10` | 40px | Page-level top/bottom padding |
| `space-12` | 48px | Hero section spacing |
| `space-16` | 64px | Landing page spacing |

**Page layout standard:**
- Page horizontal padding: `space-6` (24px) on mobile, `space-8` (32px) on desktop
- Content max-width: `1440px` centered
- Section gap: `space-8` (32px)
- Card gap: `space-4` (16px)

### 4.3 Color System

Expand the current monochromatic purple into a multi-hue semantic system:

#### Brand Colors

| Token | Light Mode | Dark Mode | Usage |
|---|---|---|---|
| `--primary` | Purple 600 | Purple 400 | Primary actions, links, active states |
| `--primary-hover` | Purple 700 | Purple 300 | Hover states |
| `--primary-subtle` | Purple 50 | Purple 950 | Backgrounds, highlights |

#### Semantic Colors

| Token | Light Mode | Dark Mode | Usage |
|---|---|---|---|
| `--success` | Green 600 | Green 400 | Confirmations, positive states |
| `--success-subtle` | Green 50 | Green 950 | Success backgrounds |
| `--warning` | Amber 500 | Amber 400 | Warnings, attention states |
| `--warning-subtle` | Amber 50 | Amber 950 | Warning backgrounds |
| `--info` | Blue 500 | Blue 400 | Informational, neutral highlights |
| `--info-subtle` | Blue 50 | Blue 950 | Info backgrounds |
| `--destructive` | Red 600 | Red 400 | Errors, dangerous actions |
| `--destructive-subtle` | Red 50 | Red 950 | Error backgrounds |

#### Surface Elevation

| Token | Light Mode | Dark Mode | Usage |
|---|---|---|---|
| `--surface-0` | White | Gray 950 | Page background |
| `--surface-1` | Gray 50 | Gray 900 | Card backgrounds, sidebar |
| `--surface-2` | Gray 100 | Gray 850 | Nested cards, table headers |
| `--surface-3` | Gray 200 | Gray 800 | Active/selected backgrounds |

#### Chart Colors

Replace monochromatic purple with a **categorically distinct** palette:

| Token | Color | Purpose |
|---|---|---|
| `--chart-1` | Purple 500 | Primary metric |
| `--chart-2` | Teal 500 | Secondary metric |
| `--chart-3` | Amber 500 | Tertiary metric |
| `--chart-4` | Rose 500 | Quaternary metric |
| `--chart-5` | Blue 500 | Quinary metric |

### 4.4 Component Standards

#### Buttons

| Variant | Usage | Appearance |
|---|---|---|
| `primary` | Main actions (Create, Save, Submit) | Filled with primary color |
| `secondary` | Supporting actions (Cancel, Filter) | Subtle background |
| `outline` | Tertiary actions (Export, View All) | Border only |
| `ghost` | In-context actions (Edit, row actions) | No border, text-only |
| `destructive` | Dangerous actions (Delete) | Red filled |
| `link` | Navigation within text | Underlined text |

**Sizing:** `sm` (32px), `default` (36px), `lg` (40px)  
**Icon buttons:** Always include `aria-label`; use tooltip for context.

#### Cards

Standardize a `Card` component with:
- `surface-1` background (subtle elevation)
- `border` border
- `space-6` padding
- `var(--radius)` border-radius
- Optional: header (title + description + action), body, footer

#### Data Tables

- **Default visible columns:** Max 6 on desktop, 3 on mobile
- **Column visibility:** User-togglable via dropdown
- **Row height:** 48px minimum for touch targets
- **Hover state:** `surface-2` background on row hover
- **Selection:** Checkbox column for bulk operations
- **Actions:** Icon-only buttons on row hover (edit, delete, more)
- **Empty state:** Illustrated component with message and CTA
- **Loading state:** Skeleton with matching column structure
- **Pagination:** Bottom bar with page size selector and total count

#### Forms

- **Layout:** Single column for simple forms; two-column grid for related fields (e.g., first/last name)
- **Drawer width:** `sm:max-w-lg` minimum; `sm:max-w-xl` for complex forms
- **Section grouping:** Use `fieldset` with subtle separator between groups
- **Required indicator:** Asterisk on label (already implemented)
- **Error placement:** Below field (already implemented)
- **Submit button:** Bottom-right, full-width on mobile

### 4.5 Iconography

Current: Lucide React icons centralized in `ModuleIcons` config — **good pattern, keep it**.

Standards:
- **Size:** 16px (`h-4 w-4`) inline, 20px (`h-5 w-5`) in buttons, 24px (`h-6 w-6`) in nav
- **Color:** `currentColor` to inherit text color
- **Stroke width:** 1.5px (Lucide default) — slightly reduce to 1.25px for a lighter feel
- **Decorative icons:** Use `aria-hidden="true"`
- **Interactive icons:** Require `aria-label`

### 4.6 Elevation / Shadows

| Token | Shadow | Usage |
|---|---|---|
| `shadow-none` | none | Flat elements |
| `shadow-xs` | `0 1px 2px rgba(0,0,0,0.04)` | Cards, inputs |
| `shadow-sm` | `0 2px 4px rgba(0,0,0,0.06)` | Dropdowns, hovering cards |
| `shadow-md` | `0 4px 12px rgba(0,0,0,0.08)` | Popovers, tooltips |
| `shadow-lg` | `0 8px 24px rgba(0,0,0,0.12)` | Modals, drawers |
| `shadow-xl` | `0 16px 48px rgba(0,0,0,0.16)` | Command palette, overlays |

### 4.7 Border Radius Standards

| Token | Value | Usage |
|---|---|---|
| `radius-sm` | 6px | Tags, badges, small chips |
| `radius-md` | 8px | Buttons, inputs, cards (default) |
| `radius-lg` | 12px | Large cards, modals |
| `radius-xl` | 16px | Hero cards, featured content |
| `radius-full` | 9999px | Avatars, circular buttons |

---

## 5. Component Redesign Plan

### 5.1 Dashboard (Home Page)

**Current state:** `<h1>Home page</h1>` — empty placeholder.

**Proposed redesign:**
- **Hero welcome banner** with user name, role, and current academic context
- **4× KPI stat cards** in a responsive grid (Total Students, Active Sections, Courses, Teachers)
- **Enrollment timeline widget** showing current position in the academic year
- **Quick actions grid** (6–8 tiles linking to common workflows)
- **Recent activity feed** (last 10 created/modified records with timestamps)
- **Alerts/notifications panel** (incomplete sections, scheduling conflicts, pending approvals)

**Reasoning:** The home page is every user's daily starting point. A data-rich dashboard dramatically reduces navigation time and provides system-wide awareness — a hallmark of modern SaaS tools (Stripe Dashboard, Vercel, Linear).

**Priority:** 🔴 P0 — Critical

---

### 5.2 Management Page Layout

**Current state:** Title + description + icon → separator → full-width table.

**Proposed redesign:**
- **Stat bar** above the table (3–4 inline metric cards showing total count, recent additions, category breakdowns)
- **Toolbar** with search, filters, view toggle (table/card/list), and create button
- **Responsive table** with row hover actions, selectable rows, and column visibility controls
- **Empty state** component when zero records exist
- **Content max-width** of 1440px centered with comfortable side margins

**Reasoning:** Modern data management UIs (Retool, Azure Portal, Notion databases) provide context above the data and tools inline with the data. The current layout provides no context and no inline tools.

**Priority:** 🔴 P0 — Critical (applies to 7+ pages)

---

### 5.3 Data Table

**Current state:** Full-featured Shadcn data table with sorting, filtering, pagination, column pinning, and skeleton loading.

**Proposed redesign:**
- **Reduce default columns** to 5–6; move audit columns (Created At, Updated At, etc.) behind column visibility toggle
- **Row hover actions** — show edit/delete/more buttons on row hover instead of permanent actions column
- **Bulk selection** — add checkbox column with select-all and bulk action toolbar
- **Empty state** — custom EmptyState component with illustration, friendly message, and CTA button
- **Sticky header** — keep column headers visible during scroll
- **Row click** — navigate to detail page on row click (where applicable)
- **Compact/comfortable density toggle** — let users choose data density

**Reasoning:** Data tables are the backbone of the application. Making them more interactive, less cluttered, and more flexible directly improves daily usability.

**Priority:** 🔴 P0 — Critical

---

### 5.4 Form Drawers

**Current state:** Narrow right-side drawers (`sm:max-w-sm` / ~384px) with vertically stacked form fields.

**Proposed redesign:**
- **Increase default width** to `sm:max-w-lg` (512px) or `sm:max-w-xl` (576px)
- **Multi-column layouts** for related fields (e.g., building + room, first + last name)
- **Form sections** with subtle dividers and section titles
- **Sticky footer** with submit/cancel buttons always visible
- **Unsaved changes warning** when closing with dirty form state
- **Progressive loading** — show form immediately, load dependent data (dropdowns) async with skeletons
- **Success state** — brief in-drawer confirmation before auto-closing

**Reasoning:** Forms are the primary data input mechanism. More space, better organization, and interaction polish significantly reduce errors and improve speed.

**Priority:** 🟡 P1 — High

---

### 5.5 Delete Confirmation Dialogs

**Current state:** 8 near-identical copies of a generic AlertDialog with title, description, and confirm/cancel buttons.

**Proposed redesign:**
- **Consolidate into one generic `DeleteConfirmationDialog`** component
- **Show impact summary** — "This will also delete 12 rooms associated with this building"
- **Require type-to-confirm** for high-impact deletions (e.g., deleting a curriculum)
- **Destructive button styling** with loading state during mutation

**Reasoning:** Reduces ~400 lines of duplicated code and adds safety for irreversible actions.

**Priority:** 🟡 P1 — High

---

### 5.6 Navigation (Sidebar)

**Current state:** Collapsible sidebar with two groups (Master Data, Curriculum & Scheduling), each containing sub-items. Uses Shadcn Sidebar with icon-collapse mode.

**Proposed redesign:**
- **Add search/command palette** — global search accessible via `Cmd+K` or sidebar search input
- **Add recently visited** section (last 3–5 pages for quick access)
- **Badge counts** on nav items (e.g., "Sections (42)")
- **Visual active state** — left border accent on active item (Linear-style)
- **Group collapse memory** — persist collapsed/expanded state per user
- **Bottom utility section** — settings, help/docs, keyboard shortcuts

**Reasoning:** Navigation is used on every interaction. Small improvements compound into major time savings.

**Priority:** 🟡 P1 — High

---

### 5.7 Searchable Select Components

**Current state:** Three variants — `SearchableSelect`, `MultiSearchableSelect`, and a custom-trigger variant — with duplicated search/filter logic.

**Proposed redesign:**
- **Consolidate into one `Combobox` component** with props: `multiple`, `creatable`, `async`
- **Unified API:** `value`, `onChange`, `options`, `onSearch`, `renderOption`, `renderValue`
- **Async data loading** with debounced search and loading skeleton
- **Create inline** — allow users to create new options without leaving the field
- **Keyboard navigation** — full arrow-key support with typeahead
- **Group support** — visually group options by category

**Reasoning:** Eliminates code duplication, provides a consistent developer API, and improves the end-user experience with a single well-tested component.

**Priority:** 🟡 P1 — High

---

### 5.8 Loading Overlay

**Current state:** Full-screen fixed overlay with animated spinner and optional text. Blocks all interaction.

**Proposed redesign:**
- **Replace full-screen overlay** with inline loading indicators where possible
- **Use progress bars** for known-duration operations
- **Add `aria-live="polite"` and `role="status"`** for accessibility
- **Skeleton screens** — prefer skeleton loading (already exists for tables) over spinners
- **Lazy section loading** — load page chrome immediately, stream data sections independently

**Reasoning:** Full-screen overlays create perceived slowness. Skeleton screens and progressive loading (Notion, Linear pattern) feel significantly faster.

**Priority:** 🟡 P1 — High

---

### 5.9 Breadcrumb Navigation

**Current state:** Dynamic breadcrumbs in the header based on route matching. Hidden on mobile.

**Proposed redesign:**
- **Show on mobile** — use a compact "Back" button + current page name on small screens
- **Add page context actions** inline with breadcrumbs (e.g., "Share", "Export")
- **Truncate long paths** with an ellipsis popover for deep nesting

**Reasoning:** Breadcrumbs are a critical wayfinding tool. Hiding them on mobile degrades the mobile experience.

**Priority:** 🟠 P2 — Medium

---

### 5.10 Curriculum Builder

**Current state:** Form-based interface for managing curriculum structure (year levels, semesters, subjects).

**Proposed redesign:**
- **Visual curriculum grid** — matrix view with year levels as rows, semesters as columns, subjects as cards in each cell
- **Drag-and-drop** subject cards between semesters
- **Subject card** showing name, units, prerequisites badge, and hours
- **Curriculum completeness tracker** — progress bar for total units vs. program requirements
- **Inline subject search** — type-ahead to search and add subjects directly into a cell
- **Prerequisite visualization** — draw lines/arrows between prerequisite subjects across semesters

**Reasoning:** Curriculum design is fundamentally spatial — it's a matrix of years × semesters × subjects. A visual representation dramatically reduces cognitive load compared to form-based input.

**Priority:** 🟡 P1 — High

---

### 5.11 Section Schedule View

**Current state:** Plain HTML table showing time slots across days of the week.

**Proposed redesign:**
- **Calendar-style weekly view** with colored time blocks (Google Calendar / Timetable pattern)
- **Time blocks** showing subject name, room, teacher, and time range
- **Drag-to-create** new schedule entries by clicking and dragging on empty time slots
- **Drag-to-resize** for adjusting time block duration
- **Conflict highlighting** — red overlay when a room or teacher has overlapping assignments
- **Color-coded by subject** for quick visual scanning
- **Mini weekly overview** in the sidebar showing all assigned vs. unassigned slots

**Reasoning:** Schedule management is the most interaction-heavy workflow. A visual, drag-based interface is dramatically faster than form-based entry — this is the biggest UX improvement opportunity in the entire application.

**Priority:** 🔴 P0 — Critical

---

## 6. Quick Wins

Improvements that can be implemented in **less than one day** each:

| # | Improvement | Effort | Impact |
|---|---|---|---|
| 1 | Fix `text-slate-900` hardcoded colors → use `text-foreground` token | 1 hour | Dark mode fix |
| 2 | Add `aria-live="polite"` and `role="status"` to `OverlayLoader` | 30 min | Accessibility |
| 3 | Add `aria-label` to all icon-only buttons throughout the app | 2 hours | Accessibility |
| 4 | Add `aria-sort` attribute to sortable table column headers | 1 hour | Accessibility |
| 5 | Add a skip-to-content link in the root layout | 30 min | Accessibility |
| 6 | Create a generic `EmptyState` component and apply to all tables | 3 hours | UX |
| 7 | Add `success`, `warning`, `info` CSS color tokens to `index.css` | 1 hour | Design system |
| 8 | Widen form drawers from `sm:max-w-sm` to `sm:max-w-lg` | 30 min | Form UX |
| 9 | Add a content `max-width: 1440px` container to the main layout | 30 min | Layout |
| 10 | Consolidate 8 delete dialog copies into one generic `DeleteConfirmationDialog` | 4 hours | Code quality |
| 11 | Add `loading` state to submit buttons during mutation | 1 hour | UX feedback |
| 12 | Add responsive typography classes (smaller headings on mobile) | 1 hour | Mobile |
| 13 | Replace `text-slate-500` in ManagementPageLayout with `text-muted-foreground` | 15 min | Dark mode fix |
| 14 | Add tooltips to sidebar icon-only buttons in collapsed mode | 1 hour | UX |
| 15 | Persist sidebar collapsed/expanded state in localStorage | 30 min | UX |

---

## 7. High Impact Improvements

Improvements with the **highest UX value**, ranked by impact:

| Rank | Improvement | Impact | Effort |
|---|---|---|---|
| 1 | **Build the dashboard home page** — KPI cards, timeline, quick actions, recent activity | Transforms daily experience; provides system-wide awareness | 3–5 days |
| 2 | **Visual schedule builder** — calendar-style drag-and-drop weekly schedule view | Replaces most tedious workflow with intuitive interaction | 5–8 days |
| 3 | **Generic CRUD page component** — extract shared scaffolding from 7+ management pages | Eliminates ~5,000 LOC duplication; enables consistent improvements | 3–4 days |
| 4 | **Command palette** — global `Cmd+K` search across all entities and actions | Dramatically reduces navigation time for power users | 2–3 days |
| 5 | **Empty states** — illustrated components for all zero-data scenarios with CTAs | Guides new users; reduces confusion on first setup | 1–2 days |
| 6 | **Visual curriculum grid** — matrix view for curriculum builder | Reduces cognitive load for complex curriculum structures | 4–6 days |
| 7 | **Semantic color system** — add success, warning, info tokens with subtle background variants | Enables richer status communication across all components | 1 day |
| 8 | **Unified Combobox component** — replace 3 searchable-select variants with one | Improves consistency and reduces ~600 LOC duplication | 2–3 days |
| 9 | **Bulk operations** — multi-select rows with bulk delete/export | Common admin workflow currently impossible | 2–3 days |
| 10 | **Complete teachers module** — finish CRUD + add schedule visibility | Unblocks scheduling workflow that depends on teacher data | 2–3 days |

---

## 8. Implementation Roadmap

### Phase 1 — Foundation (Weeks 1–2)

**Goal:** Fix critical issues, establish design tokens, and build shared infrastructure.

| Task | Effort | Dependencies |
|---|---|---|
| Fix all hardcoded colors (slate-900, slate-500) → use design tokens | 2 hours | None |
| Add semantic color tokens (success, warning, info) to CSS | 2 hours | None |
| Define and apply typography scale as CSS custom properties | 4 hours | None |
| Add spacing scale tokens | 2 hours | None |
| Add shadow/elevation scale tokens | 1 hour | None |
| Create `EmptyState` component | 3 hours | None |
| Add accessibility fixes (aria-live, aria-sort, aria-labels, skip link) | 4 hours | None |
| Set content max-width (1440px) | 30 min | None |
| Add responsive typography utilities | 2 hours | Typography scale |
| Widen form drawers | 30 min | None |
| **Total Phase 1** | **~2.5 days** | |

---

### Phase 2 — Design System & Shared Components (Weeks 3–4)

**Goal:** Build reusable components that eliminate duplication and establish visual consistency.

| Task | Effort | Dependencies |
|---|---|---|
| Consolidate 3 searchable-select variants → unified `Combobox` | 2–3 days | Phase 1 |
| Create generic `DeleteConfirmationDialog` with impact summary | 4 hours | Phase 1 |
| Create generic `CrudPage` template component | 3–4 days | EmptyState, design tokens |
| Extract hero-card, icon-chip, status-badge from academic year page | 1 day | Phase 1 |
| Build stat card components for management page headers | 1 day | Phase 1 |
| Implement data table enhancements (row hover actions, bulk select, sticky header) | 2–3 days | Phase 1 |
| Build command palette (`Cmd+K` search) | 2–3 days | Phase 1 |
| Add unsaved-changes warning to form drawers | 4 hours | Phase 1 |
| **Total Phase 2** | **~2 weeks** | Phase 1 |

---

### Phase 3 — Page Redesign (Weeks 5–8)

**Goal:** Redesign each page using the new design system and shared components.

| Task | Effort | Dependencies |
|---|---|---|
| **Dashboard home page** — KPI cards, timeline, quick actions, activity feed | 3–5 days | Phase 2 (stat cards) |
| **Management pages (7×)** — migrate to CrudPage template, add stat bars, empty states | 3–5 days | Phase 2 (CrudPage) |
| **Teachers module** — complete CRUD, add profile card and schedule view | 2–3 days | Phase 2 |
| **Login page** — branded layout with Microsoft SSO button | 1 day | Phase 1 |
| **Section detail page** — visual weekly calendar with colored time blocks | 3–5 days | Phase 2 |
| **Sections list page** — add status badges, capacity bars, inline schedule preview | 2–3 days | Phase 2 |
| **Curriculum builder** — visual grid layout with drag-and-drop | 4–6 days | Phase 2 |
| **Total Phase 3** | **~3–4 weeks** | Phase 2 |

---

### Phase 4 — Advanced UX Enhancements (Weeks 9–12)

**Goal:** Add polish, advanced interactions, and accessibility refinements.

| Task | Effort | Dependencies |
|---|---|---|
| **Drag-and-drop scheduling** — create/move/resize time blocks | 5–8 days | Phase 3 (schedule view) |
| **Drag-and-drop curriculum** — reorder subjects between semesters | 3–4 days | Phase 3 (curriculum grid) |
| **Comprehensive accessibility audit** — WCAG 2.2 AA compliance | 3–5 days | Phase 3 |
| **Keyboard shortcuts system** — global shortcuts with discoverable cheat sheet | 2–3 days | Phase 2 (command palette) |
| **Onboarding flow** — first-time setup wizard for new installations | 3–4 days | Phase 3 (dashboard) |
| **Notification system** — in-app notifications for scheduling conflicts, approvals | 3–5 days | Phase 3 |
| **Performance optimization** — virtualized tables, optimistic updates, prefetching | 2–3 days | Phase 3 |
| **Mobile optimization** — responsive table cards, touch gestures, bottom nav | 3–5 days | Phase 3 |
| **Internationalization (i18n) preparation** — extract all hardcoded strings | 3–4 days | Phase 3 |
| **Animation and micro-interactions** — page transitions, skeleton shimmer, success animations | 2–3 days | Phase 3 |
| **Total Phase 4** | **~4–5 weeks** | Phase 3 |

---

### Timeline Summary

```
Week 1–2:   Phase 1 — Foundation (design tokens, a11y fixes, shared primitives)
Week 3–4:   Phase 2 — Design System (reusable components, deduplication)
Week 5–8:   Phase 3 — Page Redesign (dashboard, CRUD pages, curriculum, scheduling)
Week 9–12:  Phase 4 — Advanced UX (drag-and-drop, a11y audit, i18n, polish)
```

**Total estimated effort:** 10–14 weeks for a single frontend developer.

**Recommended team:** 2 frontend developers working in parallel — one on Phase 2 components while the other begins Phase 3 pages with interim components — can compress the timeline to **7–9 weeks**.

---

## Appendix: Design Reference Benchmarks

The following applications serve as north-star references for specific aspects of the redesign:

| Application | What to reference |
|---|---|
| **Linear** | Sidebar navigation, keyboard shortcuts, command palette, clean data density |
| **Notion** | Progressive loading, inline editing, flexible views (table/board/calendar) |
| **Stripe Dashboard** | KPI cards, data visualization, stat-rich page headers, typography hierarchy |
| **Vercel** | Minimalist layout, clear hierarchy, surface elevation, monospace numbers |
| **GitHub** | Table views, empty states, status badges, contextual actions |
| **Clerk** | Auth flow, onboarding, user management UI patterns |
| **Ramp** | Financial dashboards, data tables with inline stats, filtering patterns |
| **Retool** | Admin panel patterns, CRUD layouts, data table density controls |
| **Azure Portal** | Resource management, breadcrumb-based navigation, dashboard tiles |
| **Google Calendar** | Weekly schedule view, drag-to-create, time block visualization |
