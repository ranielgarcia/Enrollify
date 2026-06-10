# Senior-Level Code & UX Review: `academic-year-management-page`

**Reviewed:** `src/page-components/academic-year-management-page/` (4 files, ~640 lines)  
**Context:** College-level enrollment administration system (Enrollify)  
**Audience:** Registrars, Admins, SystemAdmins managing AY 2024–2025 and its semesters

---

## Executive Summary

The component is visually polished and architecturally coherent as a bespoke tab-based layout for a lifecycle entity. The hero-card design is appropriate and the styled badges are a step above a typical CRUD form. However, beneath that veneer are six high-severity issues that make this unsafe for production in a real college enrollment context: **network errors silently masquerade as empty states**, **`termName` data is irrecoverably wiped on every edit**, **a refreshed edit session yields a blank form with no warning**, **zero date cross-validation means nonsensical academic calendars are silently accepted**, **the entire previous-years list is permanently read-only with no delete or edit actions**, and **an existing active year is never checked before creating a new one**.

For an enrollment system where a misconfigured academic year propagates down to curriculum, scheduling, and ultimately student enrollment records, these are not cosmetic issues — they are correctness failures.

---

## Confidence Assessment

| Finding | Certainty | Basis |
|---|---|---|
| Missing error UI | ✅ Verified | `index.tsx:24-28` — `isError` never destructured; silent fallthrough confirmed |
| `termName` data loss on update | ✅ Verified | Schema at `create-academic-year-form.tsx:19-23`; update payload at lines 91-95 |
| React key bug | ✅ Verified | `previous-academic-years-tab.tsx:67-68` — `<>` fragment with key on inner div |
| Date validation gaps | ✅ Verified | Full `termFormSchema` and `academicYearFormSchema` examined; zero `.refine()` calls |
| Date format inconsistency | ✅ Verified | `form-date-picker.tsx:82` vs `date-utils.ts:16` |
| `deleteAcademicYearOptions` unwired | ✅ Verified | Defined in `academic-year-collection.ts:84-94`; zero imports in page directory |
| Session storage stale state | ✅ Verified | `index.tsx:18-34`; `entityToEdit` is React-only state — not persisted |

---

## Issue 1 — HIGH: Network Errors Silently Become Empty States

### What Is Not Ideal for College-Level Enrollment

When the `GET /api/academic-years/active` call fails (network timeout, 500, CORS, auth expiry), `data` becomes `undefined`. The page evaluates `activeYear ?? null` and passes `null` to `ActiveAcademicYearTab`, which renders the friendly "No Active Academic Year" empty state. The registrar sees an empty calendar icon and a message saying to use the "Create Form" tab — not an error. **They will attempt to create a new academic year over an existing one because they have no idea the system is down.**

Same silent failure for previous years — a 500 on that endpoint shows "No Previous Academic Years" with zero indication anything went wrong.

### Why It Matters

In other pages (teachers, courses, buildings), the identical failure throws to the `PageErrorBoundary` because they use `useSuspenseQuery`. Academic year uniquely uses `useQuery` with manual loading state, which bypasses the portal's Suspense/error boundary at `portal/route.tsx:112-120`. The inconsistency is both a UX and reliability issue.

### Suggested Approach

Switch to `useSuspenseQuery` (consistent with all other management pages) and let the portal boundary handle errors. If the tab-based isLoading behavior must be preserved, at minimum destructure `isError` and render a visible error state.

### Code Fix

```tsx
// index.tsx — Option A: align with project standard
import { useSuspenseQuery } from "@tanstack/react-query";

const { data: activeYear } = useSuspenseQuery(getActiveAcademicYearOptions());
const { data: previousYears } = useSuspenseQuery(getPreviousAcademicYearsOptions());
// Remove isLoading — the Suspense boundary handles it
// Remove isLoading={isLoading} from ManagementPageLayout
```

```tsx
// index.tsx — Option B: keep useQuery but surface errors
const { data: activeYear, isLoading, isError: isActiveYearError } = useQuery(
  getActiveAcademicYearOptions()
);
const { data: previousYears, isLoading: isPreviousLoading } = useQuery(
  getPreviousAcademicYearsOptions()
);

// Pass both loading states:
<ManagementPageLayout isLoading={isLoading || isPreviousLoading}>

// In ActiveAcademicYearTab, add an error prop and render accordingly:
<ActiveAcademicYearTab
  activeYear={activeYear ?? null}
  isError={isActiveYearError}
  onEdit={handleEditActiveYear}
/>
```

> **Evidence:** `index.tsx:24-28`[^1]; `portal/route.tsx Suspense boundary`[^2]

---

## Issue 2 — HIGH: `termName` Is Silently Wiped on Every Edit

### What Is Not Ideal

The `AcademicTerm` model has a `termName` field (e.g., `"First Semester"`, `"Second Semester"`, `"Summer Term"`) that is shown in the active and previous year card views. However, the `termFormSchema` in the create/edit form **does not include `termName`**:

```ts
// create-academic-year-form.tsx:19-23
const termFormSchema = z.object({
  termNumber: z.number().min(1, "Term number must be at least 1"),
  startDate: z.string().min(1, "Start date is required"),
  endDate: z.string().min(1, "End date is required"),
  // termName is absent
});
```

When building the edit default values from server data (lines 53–61), `termName` is mapped out:
```ts
.map((t) => ({
  termNumber: t.termNumber ?? 1,
  startDate: t.startDate ?? "",
  endDate: t.endDate ?? "",
  // t.termName silently dropped here
}))
```

The update payload at lines 91–95 sends only `{ startDate, endDate, terms: [{ termNumber, startDate, endDate }] }`. If the backend replaces term data (standard REST PUT behavior), `"First Semester"` becomes `null`.

### Why It Matters

In a college enrollment context, term names are critical for human legibility. Students, faculty, and admin staff refer to "First Semester AY 2024–2025", not "Term 1 AY 2024–2025". If a registrar updates the academic year dates to adjust for a calendar shift, they will unknowingly erase all term names.

### Suggested Approach

Add `termName` to the form schema, render a text input for it in each term card, and include it in the submit payload.

### Code Fix

```ts
// create-academic-year-form.tsx — schema
const termFormSchema = z.object({
  termNumber: z.number().min(1, "Term number must be at least 1"),
  termName: z.string().min(1, "Term name is required"),
  startDate: z.string().min(1, "Start date is required"),
  endDate: z.string().min(1, "End date is required"),
});
```

```ts
// Default terms — edit mode: preserve termName
.map((t) => ({
  termNumber: t.termNumber ?? 1,
  termName: t.termName ?? `Term ${t.termNumber ?? 1}`,
  startDate: t.startDate ?? "",
  endDate: t.endDate ?? "",
}))

// Default terms — create mode: seed with institution-standard names
Array.from({ length: numberOfSemesters }, (_, i) => ({
  termNumber: i + 1,
  termName: i === 0 ? "First Semester" : i === 1 ? "Second Semester" : `Term ${i + 1}`,
  startDate: "",
  endDate: "",
}))
```

```tsx
// In the term card JSX, add a FormField for term name
<form.Field name={`terms[${termIndex}].termName`}>
  {(field) => (
    <FormField
      field={field}
      label="Term Name"
      type="text"
      required
      placeholder="e.g. First Semester"
    />
  )}
</form.Field>
```

> **Evidence:** `create-academic-year-form.tsx:19-23`[^3], `:53-61`[^4], `:91-95`[^5]; `academic-year.ts:7`[^6]; `active-academic-year-tab.tsx:126`[^7]

---

## Issue 3 — HIGH: Zero Date Validation — Nonsensical Calendars Are Silently Accepted

### What Is Not Ideal

The Zod validation on both the academic year and its terms checks only that dates are non-empty strings. There are **no chronological, cross-field, or overlap rules at all**:

- An academic year with `startDate: "2025-06-01"` and `endDate: "2024-01-01"` (end before start) will pass validation and be submitted.
- Term 1 `endDate: "2025-12-31"` and Term 2 `startDate: "2025-01-01"` (Term 2 fully inside Term 1) will pass.
- Term dates outside the academic year range will pass.
- No overlap detection between any two terms.

### Why It Matters

In a college enrollment system, the academic year dates drive course scheduling, curriculum assignment, enrollment windows, and grade period calculations. A miskeyed "2024" instead of "2025" goes straight to the server with a friendly success toast. In a live system with students already enrolled in sections tied to those terms, this corruption is difficult to recover.

### Suggested Approach

Add Zod `.superRefine()` to enforce all chronological rules before submission, and wire the `FormDatePicker`'s `disabled` prop to restrict calendar selection based on sibling field values.

### Code Fix

```ts
// create-academic-year-form.tsx — hardened schema
const academicYearFormSchema = z
  .object({
    startDate: z.string().min(1, "Academic year start date is required"),
    endDate: z.string().min(1, "Academic year end date is required"),
    terms: z.array(termFormSchema).min(1, "At least one term is required"),
  })
  .superRefine((data, ctx) => {
    const ayStart = new Date(data.startDate);
    const ayEnd = new Date(data.endDate);

    // Rule 1: AY end must be after AY start
    if (ayEnd <= ayStart) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Academic year end date must be after start date",
        path: ["endDate"],
      });
    }

    const sortedTerms = [...data.terms].sort(
      (a, b) => a.termNumber - b.termNumber
    );

    sortedTerms.forEach((term, i) => {
      const tStart = new Date(term.startDate);
      const tEnd = new Date(term.endDate);

      // Rule 2: Term end after term start
      if (tEnd <= tStart) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: `Term ${term.termNumber} end date must be after start date`,
          path: ["terms", i, "endDate"],
        });
      }

      // Rule 3: Term dates within academic year range
      if (tStart < ayStart || tEnd > ayEnd) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: `Term ${term.termNumber} dates must fall within the academic year`,
          path: ["terms", i, "startDate"],
        });
      }

      // Rule 4: No overlap with previous term
      if (i > 0) {
        const prevEnd = new Date(sortedTerms[i - 1].endDate);
        if (tStart < prevEnd) {
          ctx.addIssue({
            code: z.ZodIssueCode.custom,
            message: `Term ${term.termNumber} must start after Term ${sortedTerms[i - 1].termNumber} ends`,
            path: ["terms", i, "startDate"],
          });
        }
      }
    });
  });
```

> **Evidence:** `create-academic-year-form.tsx:19-31`[^3] — zero `.refine()` or `.superRefine()` calls in the entire file

---

## Issue 4 — HIGH: Previous Academic Years Are Permanently Read-Only

### What Is Not Ideal

The Previous Academic Years tab renders a beautiful, well-styled list of inactive year cards — but every card is completely inert. No edit, no delete, no reactivation. The `PreviousAcademicYearsTabProps` interface has no callbacks:

```ts
// previous-academic-years-tab.tsx:8-10
interface PreviousAcademicYearsTabProps {
  previousYears: AcademicYear[];  // no onEdit, no onDelete, no onReactivate
}
```

Yet `deleteAcademicYearOptions` is fully defined in the collection (`academic-year-collection.ts:84-94`), `canDeleteAcademicYearAndTerms` policy is registered, and the shared `<DeleteAlertDialog>` component exists and is used by five other pages. The entire delete infrastructure is built and ready — just never wired.

### Why It Matters

For college enrollment administration:
- **A registrar who creates a year with wrong dates needs to delete it.** If it's been made inactive (e.g., a new year was created), there's no way to remove the erroneous record through the UI.
- **There are years a decade old** in any real institution. With no pagination and no delete, the previous years list becomes an unmanageable scroll with no remediation options.
- **Reactivation is a real scenario**: A registrar may need to revert to a previous year if the current one was created in error.

Also critical: the active year has an Edit button but **no Delete button**. The delete API and policy are wired nowhere in the entire page.

### Suggested Approach

1. Add `onEdit` and `onDelete` callbacks to `PreviousAcademicYearsTabProps`.
2. Wire `DeleteAcademicYearAlertDialog` (create it as a thin wrapper around `DeleteAlertDialog`).
3. Add a Delete icon button to both the active year card and each previous year card.
4. For the active year, show a warning in the delete confirmation that deleting the active year will leave no active year until a new one is created.

### Code Fix

```tsx
// Create: delete-academic-year-alert-dialog.tsx
import { deleteAcademicYearOptions } from "@/api/collections/academic-year-collection";
import { DeleteAlertDialog } from "@/components/delete-alert-dialog";
import type { AcademicYear } from "@/api/models/academic-year";

interface Props {
  yearToDelete: AcademicYear | undefined;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeleteAcademicYearAlertDialog({ yearToDelete, isOpen, onOpenChange }: Props) {
  return (
    <DeleteAlertDialog
      entityToDelete={yearToDelete}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      entityLabel="Academic Year"
      getEntityName={(y) => y.academicYearTitle ?? `AY ${y.startYear}–${y.endYear}`}
      deleteMutationOptions={deleteAcademicYearOptions(yearToDelete?.id ?? 0)}
      customDialogDescription={
        yearToDelete?.isActive
          ? "Deleting the active academic year will leave no active year in the system. Ensure you create a new one immediately after."
          : undefined
      }
    />
  );
}
```

```tsx
// previous-academic-years-tab.tsx — add callbacks
interface PreviousAcademicYearsTabProps {
  previousYears: AcademicYear[];
  onEdit: (year: AcademicYear) => void;
  onDelete: (year: AcademicYear) => void;
}

// In each year card header (right side, mirroring active tab):
<div className="flex items-center gap-2">
  <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => onEdit(year)}>
    <Edit2 className="size-3.5" />
    <span className="sr-only">Edit {title}</span>
  </Button>
  <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive hover:text-destructive" onClick={() => onDelete(year)}>
    <Trash2 className="size-3.5" />
    <span className="sr-only">Delete {title}</span>
  </Button>
</div>
```

> **Evidence:** `previous-academic-years-tab.tsx:8-10`[^8]; `academic-year-collection.ts:84-94`[^9]; `academic-year-and-terms-policies.ts:45-55`[^10]

---

## Issue 5 — HIGH: Creating a New Year With No Active Year Conflict Check

### What Is Not Ideal

When a registrar navigates directly to the "Create Form" tab (without using the Edit button), there is no indication of whether an active year already exists. The form submits `POST /api/academic-years` unconditionally. The `activeYear` data is fetched in `index.tsx` but **is never passed as a prop to `CreateAcademicYearForm`**.

```tsx
// index.tsx:92-97 — activeYear is fetched but not passed to the form
<CreateAcademicYearForm
  key={entityToEdit?.id ?? "new"}
  yearToEdit={entityToEdit}
  onSuccess={handleFormSuccess}
  onCancel={handleFormCancel}
  // activeYear={activeYear}  ← never passed
/>
```

### Why It Matters

The result depends entirely on backend behavior: the server may overwrite the existing active year, return a 409 conflict, or create a second "active" year depending on implementation. None of these outcomes are surfaced to the user before submission. In a live semester with enrolled students, accidentally creating an overlapping academic year is a serious data integrity issue.

### Suggested Approach

Pass `activeYear` to the form and render a warning banner when it exists and the form is in "create" (not "edit") mode.

### Code Fix

```tsx
// create-academic-year-form.tsx — add prop and warning banner
interface CreateAcademicYearFormProps {
  yearToEdit?: AcademicYear | null;
  currentActiveYear?: AcademicYear | null;  // new
  onSuccess?: () => void;
  onCancel?: () => void;
}

// In JSX, before the form fields:
{!isUpdating && currentActiveYear && (
  <div className="flex items-start gap-3 rounded-lg border border-amber-500/30 bg-amber-500/10 p-4 text-sm text-amber-700">
    <AlertTriangle className="mt-0.5 size-4 shrink-0" />
    <div>
      <p className="font-medium">Active Academic Year Exists</p>
      <p className="text-amber-600">
        {currentActiveYear.academicYearTitle ?? `AY ${currentActiveYear.startYear}–${currentActiveYear.endYear}`} is currently active.
        Creating a new year will replace it. Use the{" "}
        <button className="underline" onClick={onCancel}>Edit button</button> on the Active tab to modify the current year instead.
      </p>
    </div>
  </div>
)}
```

> **Evidence:** `index.tsx:92-97`[^1]; `create-academic-year-form.tsx:96-101`[^3]

---

## Issue 6 — HIGH: Tab Persistence Breaks Edit State on Refresh

### What Is Not Ideal

```ts
// index.tsx:18-35
const [activeTab, setActiveTab] = useState(
  () => sessionStorage.getItem("academic-year-tab") ?? "active",
);
const handleTabChange = (value: string) => {
  setActiveTab(value);
  sessionStorage.setItem("academic-year-tab", value);
};
```

`sessionStorage` persists the active tab string. When the user clicks "Edit" on the Active Year card, two things happen: `entityToEdit` is set in React state (memory) and the active tab is set to `"create"` in sessionStorage. If the user refreshes, the tab restores to `"create"` but `entityToEdit` is `undefined` — React state does not survive a refresh. The form renders as empty "Create Form" mode, with no warning that the edit context was lost.

### Why It Matters

A registrar mid-edit who hits F5 (which happens on browser performance issues, accidental key combos, network problems triggering a page reload) will see an empty create form. They may not notice, fill in new values, and create a duplicate academic year rather than updating the existing one.

### Suggested Approach

Either (a) never persist the `"create"` tab value to sessionStorage (only persist `"active"` and `"previous"`), or (b) store the entity ID alongside the tab value and re-fetch on mount. Option (a) is simpler and correct:

### Code Fix

```ts
// index.tsx — only persist safe tabs
const handleTabChange = (value: string) => {
  setActiveTab(value);
  // Don't persist "create" — edit context doesn't survive refresh
  if (value !== "create") {
    sessionStorage.setItem("academic-year-tab", value);
  } else {
    sessionStorage.removeItem("academic-year-tab");
  }
};
```

> **Evidence:** `index.tsx:18-35`[^1]; `use-crud-state.ts`[^11] — `entityToEdit` is React state only

---

## Issue 7 — MEDIUM: "Create Form" Tab Label Is Developer Jargon

### What Is Not Ideal

```tsx
// index.tsx:72-75
<TabsTrigger value="create" className="gap-2">
  <CalendarPlus className="size-4" />
  {entityToEdit ? "Edit Academic Year" : "Create Form"}
</TabsTrigger>
```

The label "Create Form" is an implementation description ("there is a form for creating"), not a task description ("you can set up a new academic year here"). College admin staff are not engineers. "Create Form" is meaningless to a registrar; "New Academic Year" or "Set Up Year" communicates intent.

Compounding the problem: the empty state in `active-academic-year-tab.tsx:51-54` hardcodes the tab name as a string:

```tsx
Use the <strong className="text-foreground font-medium">Create Form</strong> tab to initiate one.
```

If the tab label is ever updated, this description silently goes stale.

### Why It Matters

Tab labels are the primary navigation affordance. When a registrar is onboarding to a new system, ambiguous labels create hesitation and support tickets.

### Suggested Approach

Rename the tab and replace the hardcoded reference with a shared constant.

### Code Fix

```ts
// Create a shared constant (e.g., inside index.tsx or a constants file)
const TAB_LABELS = {
  active: "Active Academic Year",
  create: "New Academic Year",
  previous: "Previous Academic Years",
} as const;

// index.tsx — use it in both the tab and the empty-state description
{entityToEdit ? "Edit Academic Year" : TAB_LABELS.create}

// active-academic-year-tab.tsx — accept tab label as prop
interface ActiveAcademicYearTabProps {
  activeYear: AcademicYear | null;
  onEdit: () => void;
  onNavigateToCreate: () => void;  // new — also enables the CTA button
  createTabLabel: string;           // new — prevents hardcoded string
}

// Empty state with actionable CTA
<p className="text-sm text-muted-foreground max-w-xs">
  No active academic year. Use the{" "}
  <button className="font-medium text-foreground underline" onClick={onNavigateToCreate}>
    {createTabLabel}
  </button>{" "}
  tab to set one up.
</p>
<Button size="sm" className="mt-2" onClick={onNavigateToCreate}>
  <CalendarPlus className="size-4 mr-2" />
  Set Up Academic Year
</Button>
```

> **Evidence:** `index.tsx:72-75`[^1]; `active-academic-year-tab.tsx:51-54`[^12]

---

## Issue 8 — MEDIUM: Term Names Not Editable — Form Is Contextually Blind

### What Is Not Ideal

Term names (e.g., "First Semester", "Second Semester", "Midyear") are meaningful institutional identifiers in Philippine CHED-aligned academic systems (which Enrollify appears to target). The active year display correctly shows `term.termName` (`active-academic-year-tab.tsx:126`), and the model has `termName: z.string().optional()` — but the form has no input for it.

Admins cannot set a term name during creation and cannot read back the existing term name during editing (it is stripped from `defaultTerms` at `create-academic-year-form.tsx:53-61`). The number of terms and their structural identifiers are locked to a global `academicSystem` integer with no per-year override. A university with both semestral and trimestral programs under the same admin cannot configure them differently per year.

### Why It Matters

In the Philippine context (and most Asian tertiary institutions), academic administrators formally refer to "First Semester", "Second Semester", and "Midyear Term" — not "Term 1", "Term 2", "Term 3". Having the system auto-label them as "Term 1" is technically functional but professionally unprofessional for a formal enrollment document system.

### Suggested Approach

Add `termName` to the form (see Issue 2 above for the schema fix). Additionally, consider seeding smart defaults based on the total term count:

```ts
function getDefaultTermName(termIndex: number, totalTerms: number): string {
  if (totalTerms === 2) {
    return termIndex === 0 ? "First Semester" : "Second Semester";
  }
  if (totalTerms === 3) {
    return termIndex === 0
      ? "First Semester"
      : termIndex === 1
      ? "Second Semester"
      : "Midyear Term";
  }
  return `Term ${termIndex + 1}`;
}
```

> **Evidence:** `create-academic-year-form.tsx:53-61`[^3]; `active-academic-year-tab.tsx:126`[^12]; `academic-year.ts:7`[^6]

---

## Issue 9 — MEDIUM: Date Format Inconsistency Between Picker and Card View

### What Is Not Ideal

The date picker button displays dates using `date.toLocaleDateString()` (locale-dependent, `form-date-picker.tsx:82`). In the US (en-US), this renders `"6/15/2025"`. In the Philippines (en-PH or fil-PH), it may render `"June 15, 2025"` or `"15/6/2025"` depending on the browser's detected locale. After saving, the active year card displays dates via `formatDate()` from `date-utils.ts:16` which uses `format(parseISO(str), "MMMM d, yyyy")` — always `"June 15, 2025"` regardless of locale.

The same date reads `"6/15/2025"` in the picker and `"June 15, 2025"` on the card — two different representations for the same value in the same session.

Additionally, date-only strings from the server (`"2025-08-15"` without a time component) are parsed by `new Date("2025-08-15")` as UTC midnight per the ECMAScript spec. For users in UTC-negative timezones (UTC−1 through UTC−12), this shifts the displayed date back one day in the picker button (e.g., `"August 14, 2025"` instead of `"August 15, 2025"`).

### Why It Matters

Inconsistent date formatting erodes registrar trust in the system. When a registrar sets "August 15, 2025" as the start of AY 2025–2026, they expect to see "August 15, 2025" throughout the UI — not `"8/15/2025"` in one place and `"August 15, 2025"` in another.

### Suggested Approach

Use `date-fns/format` with an explicit format string in `FormDatePicker` to match the card view format. Fix the UTC parsing issue by appending time to date-only strings.

### Code Fix

```tsx
// form-date-picker.tsx — replace toLocaleDateString() with explicit format
import { format, parseISO } from "date-fns";

// Replace line 82:
// {date ? date.toLocaleDateString() : placeholder}
{date ? format(date, "MMMM d, yyyy") : placeholder}
```

```ts
// In the form default values / edit pre-fill (create-academic-year-form.tsx),
// normalize date-only strings to avoid UTC midnight off-by-one:
function normalizeDateString(dateStr: string | null | undefined): string {
  if (!dateStr) return "";
  // If it's a date-only string (YYYY-MM-DD), append local midnight
  return /^\d{4}-\d{2}-\d{2}$/.test(dateStr) ? `${dateStr}T00:00:00` : dateStr;
}
// Use when building defaultFormValues:
startDate: normalizeDateString(yearToEdit?.startDate),
endDate: normalizeDateString(yearToEdit?.endDate),
```

> **Evidence:** `form-date-picker.tsx:82`[^13]; `date-utils.ts:16`[^14]

---

## Issue 10 — MEDIUM: No Loading State for Previous Years — Phantom Empty State

### What Is Not Ideal

```tsx
// index.tsx:28-30
const { data: previousYears } = useQuery(getPreviousAcademicYearsOptions());
console.log(previousYears);  // ← also: debug log in production
```

Only `isLoading` from the **active year** query is extracted and passed to `ManagementPageLayout`. Previous years have no loading indicator. On first page load, if a user clicks "Previous Academic Years" before the query resolves, they see the "No Previous Academic Years" empty state (`previous-academic-years-tab.tsx:38-53`). When the data arrives, the list appears — a flash of incorrect empty state before real data.

### Why It Matters

A college registrar who sees "No Previous Academic Years" may reasonably conclude the system has no history and begin creating a redundant year, or file a support ticket about missing data.

### Code Fix

```tsx
// previous-academic-years-tab.tsx — accept loading state
interface PreviousAcademicYearsTabProps {
  previousYears: AcademicYear[];
  isLoading?: boolean;
  // ...
}

// Render skeleton or spinner while loading
if (isLoading) {
  return (
    <div className="space-y-4">
      {[1, 2].map((i) => (
        <div key={i} className="rounded-xl border bg-muted/20 p-6 animate-pulse h-28" />
      ))}
    </div>
  );
}
```

```tsx
// index.tsx — extract and pass loading state
const { data: previousYears, isLoading: isPreviousLoading } = useQuery(
  getPreviousAcademicYearsOptions()
);

// Remove: console.log(previousYears);  ← debug log

<PreviousAcademicYearsTab
  previousYears={previousYears ?? []}
  isLoading={isPreviousLoading}
/>
```

> **Evidence:** `index.tsx:28-30`[^1]; `previous-academic-years-tab.tsx:38-53`[^8]

---

## Issue 11 — MEDIUM: React Key Bug — List Reconciliation Silently Broken

### What Is Not Ideal

```tsx
// previous-academic-years-tab.tsx:67-68
return (
  <>                              {/* ← key must be HERE */}
    <div key={year.id} ...>       {/* ← key on inner div is ignored by React */}
```

The `key` prop must be on the **outermost element returned by `.map()`**, which is the `<>` shorthand fragment. Shorthand fragments cannot accept props — the `key` on the inner `<div>` is silently ignored by React for reconciliation. This causes incorrect DOM reuse when the list order changes (e.g., a year is deleted and others shift positions).

### Why It Matters

Incorrect list keys cause visible UI glitches (wrong data rendered in wrong card position after mutations) and React warnings in development. The `<Separator />` sibling makes the Fragment necessary, but the Fragment form that accepts a `key` must be the longhand `<React.Fragment key={...}>`.

### Code Fix

```tsx
// previous-academic-years-tab.tsx
import React from "react";

// Change:
return (
  <>
    <div key={year.id} ...>

// To:
return (
  <React.Fragment key={year.id}>
    <div className="space-y-3">
```

> **Evidence:** `previous-academic-years-tab.tsx:67-68`[^8]

---

## Issue 12 — LOW: `DateRange` Component Duplicated Across Two Files

### What Is Not Ideal

The `DateRange` display component (renders a `start → end` date range with formatting) is defined **character-for-character identically** in both `active-academic-year-tab.tsx:13-34` and `previous-academic-years-tab.tsx:12-33`. Any future change (e.g., fixing the locale, adding `aria-label`) requires updating two places.

### Code Fix

```tsx
// Create: src/page-components/academic-year-management-page/date-range.tsx
import { ArrowRight } from "lucide-react";
import { formatDate } from "@/lib/date-utils";
import { cn } from "@/lib/utils";

export function DateRange({
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
      aria-label={`${formatDate(start)} to ${formatDate(end)}`}
    >
      <span className="font-medium text-foreground">{formatDate(start)}</span>
      <ArrowRight className={cn(size === "sm" ? "size-3" : "size-3.5")} aria-hidden="true" />
      <span className="font-medium text-foreground">{formatDate(end)}</span>
    </div>
  );
}
```

> **Evidence:** `active-academic-year-tab.tsx:13-34`[^12]; `previous-academic-years-tab.tsx:12-33`[^8]

---

## Issue 13 — LOW: Accessibility Gaps

### Three Distinct Problems

**A. "●" Bullet in Badge Announced by Screen Readers**

```tsx
// active-academic-year-tab.tsx:84
<Badge ...>● Active</Badge>
// → Screen reader announces: "black circle Active"
```

Fix:
```tsx
<Badge ...>
  <span aria-hidden="true">●</span>
  {" Active"}
</Badge>
```

**B. Required Fields Missing `aria-required`**

`FormDatePicker` at `form-date-picker.tsx:63-65` renders only a visual `*` for `required`. The button trigger is never marked `aria-required="true"`:

```tsx
// form-date-picker.tsx — add to Button props
aria-required={required}
```

**C. Term Number Chips Have No Context for Screen Readers**

```tsx
// active-academic-year-tab.tsx:121-123
<span className="flex h-7 w-7 ...text-primary">{term.termNumber}</span>
// → Screen reader announces: "1" with no context

// Fix:
<span className="flex h-7 w-7 ...text-primary" aria-label={`Term ${term.termNumber}`}>
  <span aria-hidden="true">{term.termNumber}</span>
</span>
```

> **Evidence:** `active-academic-year-tab.tsx:84`[^12]; `form-date-picker.tsx:63-65`[^13]; `active-academic-year-tab.tsx:121-123`[^12]

---

## Issue 14 — LOW: `ManagementPageLayout` Uses Hardcoded Non-Semantic Colors

### What Is Not Ideal

```tsx
// management-page-layout.tsx:32,34
<h2 className="text-2xl font-bold text-slate-900">{title}</h2>
<span className="text-sm font-medium text-slate-500">{description}</span>
```

Hardcoded `text-slate-900` and `text-slate-500` will not adapt to a dark mode theme. All other places in the codebase use semantic tokens (`text-foreground`, `text-muted-foreground`).

### Code Fix

```tsx
<h2 className="text-2xl font-bold text-foreground">{title}</h2>
<span className="text-sm font-medium text-muted-foreground">{description}</span>
```

> **Evidence:** `management-page-layout.tsx:32-34`[^15]

---

## Issue 15 — LOW: `form.reset()` Called Before Navigation — Potential Empty Form Flash

### What Is Not Ideal

```ts
// create-academic-year-form.tsx:104-105
form.reset();    // empties fields synchronously — form re-renders blank
onSuccess?.();   // then navigates away
```

`form.reset()` triggers an immediate React re-render with empty field values. `onSuccess?.()` fires synchronously after in the same call stack, but React 18's automatic batching is not guaranteed to batch these in all scenarios. The form may flash blank before the tab navigates away.

### Code Fix

```ts
// Reverse the order — navigate first, reset on unmount
onSuccess?.();   // navigate away
form.reset();    // now resetting an unmounted/hidden form — harmless
```

> **Evidence:** `create-academic-year-form.tsx:104-105`[^3]

---

## Summary Table — All Issues

| # | Issue | Severity | File |
|---|---|---|---|
| 1 | Network errors silently render as empty states | 🔴 HIGH | `index.tsx:24-28` |
| 2 | `termName` wiped silently on every update | 🔴 HIGH | `create-academic-year-form.tsx:19-23,53-61,91-95` |
| 3 | Zero date chronological/overlap validation | 🔴 HIGH | `create-academic-year-form.tsx:19-31` |
| 4 | Previous years are permanently read-only; delete API unwired | 🔴 HIGH | `previous-academic-years-tab.tsx`, `academic-year-collection.ts:84-94` |
| 5 | No conflict check before creating new year over existing active | 🔴 HIGH | `index.tsx:92-97`, `create-academic-year-form.tsx:96-101` |
| 6 | Tab persistence breaks edit state on browser refresh | 🔴 HIGH | `index.tsx:18-35` |
| 7 | "Create Form" tab label is developer jargon | 🟠 MEDIUM | `index.tsx:74` |
| 8 | Term names not editable in form | 🟠 MEDIUM | `create-academic-year-form.tsx:19-23` |
| 9 | Date format inconsistency + UTC off-by-one bug | 🟠 MEDIUM | `form-date-picker.tsx:82`, `create-academic-year-form.tsx` |
| 10 | Previous years has no loading indicator — phantom empty state | 🟠 MEDIUM | `index.tsx:28`, `previous-academic-years-tab.tsx:38-53` |
| 11 | React key on wrong element — list reconciliation broken | 🟠 MEDIUM | `previous-academic-years-tab.tsx:67-68` |
| 12 | `DateRange` component duplicated in two files | 🟡 LOW | `active-academic-year-tab.tsx:13-34`, `previous-academic-years-tab.tsx:12-33` |
| 13 | Accessibility gaps (badge bullet, aria-required, term chip labels) | 🟡 LOW | `form-date-picker.tsx`, both tab files |
| 14 | Hardcoded `text-slate-900/500` won't adapt to dark mode | 🟡 LOW | `management-page-layout.tsx:32-34` |
| 15 | `form.reset()` before navigation — potential blank form flash | 🟡 LOW | `create-academic-year-form.tsx:104-105` |
| — | `console.log(previousYears)` left in production code | 🟡 LOW | `index.tsx:30` |

---

## Footnotes

[^1]: `src/page-components/academic-year-management-page/index.tsx:24-35`
[^2]: `src/routes/portal/route.tsx` — Suspense boundary at portal route level
[^3]: `src/page-components/academic-year-management-page/create-academic-year-form.tsx:19-105`
[^4]: `src/page-components/academic-year-management-page/create-academic-year-form.tsx:53-61`
[^5]: `src/page-components/academic-year-management-page/create-academic-year-form.tsx:91-95`
[^6]: `src/api/models/academic-year.ts:7` — `termName: z.string().optional()`
[^7]: `src/page-components/academic-year-management-page/active-academic-year-tab.tsx:126` — display fallback
[^8]: `src/page-components/academic-year-management-page/previous-academic-years-tab.tsx:8-10,38-53,67-68`
[^9]: `src/api/collections/academic-year-collection.ts:84-94` — `deleteAcademicYearOptions` fully defined
[^10]: `src/infrastructure/authorization/policies/academic-year-and-terms-policies.ts:45-55` — policy registered
[^11]: `src/hooks/use-crud-state.ts` — `entityToEdit` is React state, not persisted
[^12]: `src/page-components/academic-year-management-page/active-academic-year-tab.tsx:51-54,84,121-126`
[^13]: `src/components/form/form-date-picker.tsx:63-65,82`
[^14]: `src/lib/date-utils.ts:16` — `format(parseISO(dateStr), "MMMM d, yyyy")`
[^15]: `src/components/page-layouts/management-page-layout.tsx:32-34`
