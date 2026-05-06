---
name: enrollify-ui-redesign
description: >
  Design language guide and step-by-step instructions for redesigning Enrollify
  frontend components to be aesthetic and user-friendly. Use this skill when
  asked to redesign, beautify, or improve the UI of any component in the
  React/TypeScript frontend located at src/system/enrollify-frontend/src.
  Covers the hero-card pattern, icon chips, status badges, date ranges,
  form sections, term/item rows, empty states, and dashed dividers — all
  derived from the academic-year-management-page redesign.
---

# Enrollify UI Redesign Skill

This skill encodes the visual design language used across Enrollify's redesigned
components. Apply every applicable pattern when redesigning a component. Do not
mix the old and new patterns in the same file.

---

## Reference Files (Always Read These First)

Before redesigning any component, read the two canonical reference components:

- **View/display reference:** `src/page-components/academic-year-management-page/active-academic-year-tab.tsx`
- **Form reference:** `src/page-components/academic-year-management-page/create-academic-year-form.tsx`

Also check the available shadcn components:

```
src/components/ui/badge.tsx       src/components/ui/button.tsx
src/components/ui/card.tsx        src/components/ui/separator.tsx
src/components/ui/tooltip.tsx     src/components/ui/skeleton.tsx
```

And the utility:

```
src/lib/utils.ts        → cn()
src/lib/date-utils.ts   → formatDate()
```

---

## Core Design Principles

1. **Hierarchy through spacing** — Use `space-y-5` / `space-y-3` consistently.
   Never use fixed `mt-*` between siblings; rely on parent `space-y-*`.
2. **Flat over nested cards** — Prefer a single-level layout. Avoid
   `Card > CardContent > Card > CardContent` nesting.
3. **Icon chips, not bare icons** — Any icon used as a section marker must
   be wrapped in a small rounded container.
4. **Colour through opacity** — Use `primary/10`, `primary/20`, `muted/30`,
   `accent/40` etc. Never hardcode hex colours. Exceptions: semantic status
   colours like emerald (active), amber (warning), red (danger).
5. **Dashed borders signal emptiness or division** — Use `border-dashed` for
   empty states and section dividers; solid borders for interactive rows.
6. **Transitions on interactive rows** — Every clickable / hoverable row must
   have `transition-colors` and a `hover:bg-accent/40` (or equivalent).
7. **Tailwind v4 gradients** — Use `bg-linear-to-{direction}` not
   `bg-gradient-to-{direction}` (the project uses Tailwind v4).

---

## Pattern 1 — Hero Card (Primary Entity Display)

Use for the main entity being shown on a view/detail tab.

```tsx
<div className="relative overflow-hidden rounded-xl border bg-linear-to-br from-primary/5 via-background to-background p-6 shadow-sm">
  {/* Decorative blur blob */}
  <div className="pointer-events-none absolute -right-8 -top-8 h-40 w-40 rounded-full bg-primary/8 blur-2xl" />

  <div className="relative flex items-start justify-between gap-4">
    <div className="flex items-center gap-4">
      {/* Icon chip */}
      <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-primary/10 ring-1 ring-primary/20">
        <SomeIcon className="h-5 w-5 text-primary" />
      </div>
      <div className="space-y-1">
        <div className="flex items-center gap-2 flex-wrap">
          <h2 className="text-xl font-bold tracking-tight">{title}</h2>
          {/* Status badge — see Pattern 4 */}
        </div>
        {/* Subtitle or date range — see Pattern 3 */}
      </div>
    </div>
    {/* Action button — edit, details, etc. */}
    <Button
      variant="outline"
      size="sm"
      className="shrink-0 gap-1.5 shadow-sm"
      onClick={onAction}
    >
      <ActionIcon className="size-3.5" />
      Label
    </Button>
  </div>
</div>
```

**Rules:**

- Always use `relative overflow-hidden` so the blur blob is clipped.
- The blur blob is purely decorative — `pointer-events-none` required.
- Use `ring-1 ring-primary/20` on the icon chip to give it depth.
- The action button sits top-right with `shrink-0` so it never wraps.

---

## Pattern 2 — List / Item Rows (Repeating Entities)

Use for lists of related sub-entities (terms, subjects, semesters, schedules).

```tsx
<div className="space-y-3">
  {/* Section label */}
  <p className="text-xs font-semibold uppercase tracking-widest text-muted-foreground px-0.5">
    Section Title
  </p>

  <div className="grid gap-2">
    {items.map((item, idx) => (
      <div
        key={item.id ?? idx}
        className="group flex items-center justify-between rounded-lg border bg-card px-4 py-3 transition-colors hover:bg-accent/40"
      >
        <div className="flex items-center gap-3">
          {/* Ordinal / number chip */}
          <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-md bg-primary/10 text-xs font-bold text-primary">
            {item.orderNumber}
          </span>
          <div className="space-y-0.5">
            <p className="text-sm font-medium leading-none">{item.name}</p>
            <p className="text-xs text-muted-foreground">{item.subtitle}</p>
          </div>
        </div>
        {/* Optional right-side actions or metadata */}
      </div>
    ))}
  </div>
</div>
```

**Rules:**

- Number/ordinal chips use `h-7 w-7 rounded-md` (smaller than hero icon chip).
- `group` class on the row enables `group-hover:*` on children if needed.
- Always pre-sort items before rendering; never sort inside `.map()`.
- If a name can be absent, fall back gracefully: `item.name ?? \`Item ${idx + 1}\``.

---

## Pattern 3 — Date Range Display

Use wherever a start→end date pair needs to be shown.

```tsx
// Reusable helper — define at the top of the file, not exported unless shared
function DateRange({
  start,
  end,
  size = "md",
}: {
  start?: string | null;
  end?: string | null;
  size?: "sm" | "md";
}) {
  return (
    <div
      className={cn(
        "flex items-center gap-2 text-muted-foreground",
        size === "sm" ? "text-xs" : "text-sm",
      )}
    >
      <span className="font-medium text-foreground">{formatDate(start)}</span>
      <ArrowRight className={cn(size === "sm" ? "size-3" : "size-3.5")} />
      <span className="font-medium text-foreground">{formatDate(end)}</span>
    </div>
  );
}
```

**Usage:**

```tsx
<DateRange start={entity.startDate} end={entity.endDate} />
<DateRange start={term.startDate} end={term.endDate} size="sm" />
```

**Rules:**

- Import `ArrowRight` from `lucide-react` and `cn` from `@/lib/utils`.
- `size="sm"` is for use inside compact rows; `size="md"` (default) is for
  hero cards and section headers.
- Never repeat `formatDate(x)` inline when a date range is needed — always use
  this helper.

---

## Pattern 4 — Status Badge

Use for entity status indicators (Active, Inactive, Archived, Pending, etc.).

```tsx
{
  /* Active */
}
<Badge className="text-[11px] px-2 py-0.5 font-semibold bg-emerald-500/15 text-emerald-600 border border-emerald-500/25 hover:bg-emerald-500/15">
  ● Active
</Badge>;

{
  /* Inactive / Archived */
}
<Badge className="text-[11px] px-2 py-0.5 font-semibold bg-muted text-muted-foreground border hover:bg-muted">
  ● Inactive
</Badge>;

{
  /* Warning / Pending */
}
<Badge className="text-[11px] px-2 py-0.5 font-semibold bg-amber-500/15 text-amber-600 border border-amber-500/25 hover:bg-amber-500/15">
  ● Pending
</Badge>;

{
  /* Danger / Rejected */
}
<Badge className="text-[11px] px-2 py-0.5 font-semibold bg-red-500/15 text-red-600 border border-red-500/25 hover:bg-red-500/15">
  ● Rejected
</Badge>;
```

**Rules:**

- Never use `variant="default"` for status — it has the wrong visual weight.
- Always include the `●` dot prefix inside the label.
- Always include `hover:bg-*` to override the default badge hover.
- Keep `text-[11px]` so badges don't compete with the heading.

---

## Pattern 5 — Empty State

Use when a view has no data to display.

```tsx
<div className="flex flex-col items-center justify-center py-24 gap-5 text-center">
  <div className="flex h-20 w-20 items-center justify-center rounded-2xl bg-muted/60 border border-dashed">
    <SomeIcon className="h-9 w-9 text-muted-foreground/60" />
  </div>
  <div className="space-y-1.5">
    <h3 className="text-base font-semibold tracking-tight">
      No {EntityName} Found
    </h3>
    <p className="text-sm text-muted-foreground max-w-xs">
      There are no {entities} yet. Use the{" "}
      <strong className="text-foreground font-medium">Create Form</strong> tab
      or button to add one.
    </p>
  </div>
</div>
```

**Rules:**

- The icon container is `rounded-2xl` with `border border-dashed` — the dashed
  border signals "placeholder / nothing here yet".
- Icon itself is `text-muted-foreground/60` (60% opacity) to feel ghostly.
- `max-w-xs` on the description prevents overly wide text lines.
- Use `<strong className="text-foreground font-medium">` to emphasise action
  words, not `<b>` or raw bold.

---

## Pattern 6 — Form Section Header

Use at the top of each logical group of fields inside a form.

```tsx
<section className="space-y-4">
  <div className="flex items-center gap-2">
    <div className="flex h-7 w-7 items-center justify-center rounded-md bg-primary/10">
      <SomeIcon className="h-3.5 w-3.5 text-primary" />
    </div>
    <div>
      <p className="text-sm font-semibold leading-none">Section Title</p>
      <p className="text-xs text-muted-foreground mt-0.5">
        One-line description of what this section collects.
      </p>
    </div>
  </div>

  {/* Fields go here */}
</section>
```

**Rules:**

- Use `<section>` (not `<div>` or `<fieldset>`) for semantic grouping.
- Icon chip here is `h-7 w-7 rounded-md` — smaller than the hero chip.
- No `ring-*` on form section chips (ring is reserved for hero cards).
- Description is `text-xs text-muted-foreground`, not `text-sm`.

---

## Pattern 7 — Form Item Row (Repeating Form Groups)

Use when a form has a dynamic list of sub-items (terms, schedules, prerequisites).

```tsx
{
  items.map((_, idx) => (
    <div key={idx} className="rounded-lg border bg-muted/30 p-4 space-y-3">
      {/* Row header */}
      <div className="flex items-center gap-2">
        <span className="flex h-6 w-6 items-center justify-center rounded-md bg-primary/10 text-xs font-bold text-primary">
          {idx + 1}
        </span>
        <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide">
          Item {idx + 1}
        </p>
      </div>

      {/* Fields */}
      <div className="grid grid-cols-2 gap-3">{/* ... form fields ... */}</div>
    </div>
  ));
}
```

**Rules:**

- `bg-muted/30` gives each row a subtle tint to distinguish it from the page.
- Number chip uses `h-6 w-6` (smaller than view-side `h-7 w-7`) to feel
  appropriate in a form context.
- `uppercase tracking-wide` on the row label gives it a "field group" feel.
- Do NOT use a left border strip (`border-l-4`) — use the full-enclosure card
  instead.

---

## Pattern 8 — Dashed Section Divider

Use between logically separate sections of a form or view. Replace solid
`<Separator>` with this lighter treatment.

```tsx
<div className="relative">
  <div className="absolute inset-0 flex items-center">
    <div className="w-full border-t border-dashed" />
  </div>
</div>
```

**Rules:**

- The `relative` + `absolute inset-0` pattern centres the line vertically in a
  zero-height container, preventing it from adding extra vertical space.
- Only use the solid `<Separator>` component inside `CardContent` where the
  card's padding is already providing structure. Everywhere else, use dashed.

---

## Applying the Patterns: Step-by-Step Process

### Step 1 — Read the existing component fully

Use `read_file` on the target file. Never edit without reading first.

### Step 2 — Identify which patterns apply

| Component type           | Applicable patterns                                                             |
| ------------------------ | ------------------------------------------------------------------------------- |
| Entity detail / view tab | 1 (Hero Card), 2 (Item Rows), 3 (Date Range), 4 (Status Badge), 5 (Empty State) |
| Create / Edit form       | 6 (Form Section Header), 7 (Form Item Row), 8 (Dashed Divider)                  |
| Mixed (form + preview)   | All patterns                                                                    |

### Step 3 — Remove old patterns

Identify and remove these old patterns as you go:

- `Card > CardContent > ...` wrapping a form — remove the card wrapper, keep
  only the semantic layout.
- `border-l-4 border-l-primary/40 pl-4` left-strip on form groups — replace
  with Pattern 7.
- `variant="default"` on status badges — replace with Pattern 4.
- `<Separator />` between form sections — replace with Pattern 8.
- Repeated `<div className="space-y-1"><p>Label</p><p>Value</p></div>` date
  pairs — replace with Pattern 3 (`DateRange` helper).
- `text-foreground` on section headings that also say `flex items-center gap-2`
  with a bare icon — replace with Pattern 6.

### Step 4 — Check for Tailwind v4 linting errors

After editing, call `get_errors` on the file. The most common linting error:

```
The class `bg-gradient-to-br` can be written as `bg-linear-to-br`
```

Fix by replacing `bg-gradient-to-{dir}` with `bg-linear-to-{dir}`.

### Step 5 — Verify no unused imports remain

After removing old patterns, remove unused imports:

- `Card`, `CardContent`, `CardHeader`, `CardTitle` if no cards are left.
- `Separator` if all separators were replaced with dashed dividers.
- `Badge` (from the old `variant="secondary"` term badge) if replaced by chips.
- `FormSection` if form sections were replaced with Pattern 6.
- `Clock` icon if replaced by a different icon.

---

## Available Icon Suggestions by Domain

Pick icons from `lucide-react` that match the entity's domain:

| Domain          | Suggested icon                  |
| --------------- | ------------------------------- |
| Academic year   | `CalendarDays`, `CalendarRange` |
| Semester / term | `CalendarClock`                 |
| Course          | `BookOpen`                      |
| Subject         | `BookMarked`                    |
| Teacher         | `GraduationCap`, `UserRound`    |
| Room            | `DoorOpen`, `Building2`         |
| Building        | `Building2`                     |
| Department      | `Layers`                        |
| College         | `University`                    |
| Student         | `User`                          |
| Enrollment      | `ClipboardList`                 |
| Schedule        | `Clock`, `CalendarCheck`        |
| Grade           | `BarChart2`                     |

---

## Do NOT

- Do NOT add `hover:` states to static display text.
- Do NOT use `text-foreground` as the default text colour — it is the default,
  adding it is noise.
- Do NOT use `rounded-full` for icon chips in forms or list rows — only use it
  in empty-state illustrations.
- Do NOT use `gap-4` inside compact rows — use `gap-2` or `gap-3`.
- Do NOT use `CardHeader` / `CardTitle` for section headings — use Pattern 6.
- Do NOT add `font-mono` to term/item row labels — it looks like a code element,
  not a UI label.
