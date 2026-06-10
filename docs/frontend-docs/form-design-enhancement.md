# Form Design Enhancement Plan

## Current State Assessment

### What We Have

| Aspect | Current Implementation |
|---|---|
| **Container** | Right-side `Drawer` (all management forms), `Card` (curriculum form) |
| **Form library** | TanStack React Form with Zod validation |
| **Data fetching** | TanStack React Query (`useMutation`) |
| **Reusable input** | `FormField` — supports text, number, textarea, email, tel |
| **Select components** | `SearchableSelect`, `MultiSearchableSelect`, `SearchableSelectWithCustomTrigger` |
| **Error display** | Inline `<em role="alert">` below each field |
| **Loading state** | `Loader2` spinner icon inside submit button |
| **Authorization** | `AuthorizeView` wrapping form content |
| **Styling** | Tailwind CSS with OKLCH color tokens, shadcn/ui primitives |

### Forms Inventory (11 total)

| Form | Location |
|---|---|
| Course | `courses-management-page/course-form-drawer.tsx` |
| College | `colleges-management-page/college-form-drawer.tsx` |
| Subject | `subjects-management-page/subject-form-drawer.tsx` |
| Equivalence Group | `subjects-management-page/equivalence-groups/equivalence-group-form-dialog.tsx` |
| Teacher | `teachers-management-page/teacher-form-drawer.tsx` |
| Room | `rooms-management-page/room-components/room-form-drawer.tsx` |
| Room Type | `rooms-management-page/room-types-components/room-type-form-drawer.tsx` |
| Department | `departments-management-page/department-form-drawer.tsx` |
| Building | `buildings-management-page/building-form-drawer.tsx` |
| Curriculum | `curriculum-builder-page/curriculum-form.tsx` |

---

## Problems & Pain Points

### 1. Visual Design

- **No visual hierarchy** — all fields are stacked with identical spacing (`pb-4`), making forms feel flat and monotonous.
- **No field grouping** — related fields (e.g., code + name, or college + department) are not visually grouped together.
- **No section headings** — longer forms (teacher) have no logical sections to guide the user.
- **Drawer description is generic** — e.g., "Set department details." on the course form.
- **No visual feedback on success** — form just closes; no toast/animation confirming the action.

### 2. User Experience

- **No dirty-state warning** — closing a drawer with unsaved changes silently discards data.
- **No field-level help text** — users don't know what format a code should be or character limits.
- **No character counters** — textarea fields have no indication of remaining length.
- **No focus management** — first field is not auto-focused when the drawer opens.
- **Validation fires on every keystroke** — `onChange` validation can feel aggressive on initial entry; no "touched" gating.
- **No "Save & Add Another"** — users creating multiple records must re-open the drawer each time.
- **Submit button label is ambiguous** — "Submit" doesn't tell the user what will happen.

### 3. Accessibility

- **Missing `aria-describedby`** — error messages exist but are not linked to inputs via `aria-describedby`.
- **No required field indicators** — labels don't show which fields are mandatory.
- **Keyboard navigation** — no visible focus rings on drawer actions; no Escape-to-close shortcut.

### 4. Code Consistency

- **Duplicated error rendering** — `SearchableSelect` fields inline their own error block instead of using `FormField`.
- **Duplicated footer logic** — every form rebuilds the `Subscribe → Button → DrawerClose` block.
- **Inconsistent spacing classes** — some forms use `pb-4`, others use `space-y-4`, some mix both.
- **FormMeta boilerplate repeated** — identical `FormMeta` type and `defaultMeta` in every file.

---

## Enhancement Plan

### Phase 1 — Foundation (Shared Components)

> Goal: Build the reusable pieces that all forms will consume.

#### 1.1 Create `FormSection` component

Groups related fields under an optional heading with consistent spacing.

```tsx
// components/form-section.tsx
interface FormSectionProps {
  title?: string;
  description?: string;
  children: React.ReactNode;
}

export function FormSection({ title, description, children }: FormSectionProps) {
  return (
    <fieldset className="space-y-4">
      {title && (
        <legend className="text-sm font-semibold text-foreground">
          {title}
        </legend>
      )}
      {description && (
        <p className="text-xs text-muted-foreground -mt-2">{description}</p>
      )}
      {children}
    </fieldset>
  );
}
```

**Apply to**: Group "Code + Name" together, separate "Description" into its own section in longer forms, group "College" selector with related metadata.

#### 1.2 Enhance `FormField` component

Add support for:

| Feature | Prop | Purpose |
|---|---|---|
| Help text | `hint?: string` | Small muted text below the input explaining expected format |
| Required indicator | `required?: boolean` | Renders a red `*` next to the label |
| Character counter | `maxLength?: number` | Shows `12/100` below textarea fields |
| `aria-describedby` | auto-wired | Links hint and error text to the input for screen readers |
| Success state | auto-detected | Show green check icon when field is valid and touched |

```tsx
// Enhanced FormField sketch
<div className={cn("grid w-full items-center gap-1.5", className)}>
  <Label htmlFor={field.name}>
    {label}
    {required && <span className="text-destructive ml-0.5">*</span>}
  </Label>

  {/* Input / Textarea */}

  <div className="flex items-center justify-between">
    {hint && !hasError && (
      <p id={`${field.name}-hint`} className="text-xs text-muted-foreground">
        {hint}
      </p>
    )}
    {hasError && (
      <p id={`${field.name}-error`} role="alert" className="text-xs text-destructive">
        {errorMessage}
      </p>
    )}
    {maxLength && type === "textarea" && (
      <span className="text-xs text-muted-foreground ml-auto">
        {currentLength}/{maxLength}
      </span>
    )}
  </div>
</div>
```

#### 1.3 Create `FormSelectField` component

Wraps `SearchableSelect` with the same label/error/hint pattern as `FormField`, so select fields no longer duplicate error rendering.

```tsx
// components/form-select-field.tsx
interface FormSelectFieldProps {
  field: AnyFieldApi;
  label: string;
  options: SearchableSelectOption[];
  placeholder?: string;
  searchPlaceholder?: string;
  emptyMessage?: string;
  required?: boolean;
  hint?: string;
}
```

#### 1.4 Extract `FormDrawerFooter` component

Encapsulates the `Subscribe` → submit/cancel button pattern so every drawer form uses the same footer.

```tsx
// components/form-drawer-footer.tsx
interface FormDrawerFooterProps {
  form: ReactFormApi<any, any>;
  isUpdate: boolean;
  onCancel: () => void;
  entityLabel: string; // "Course", "Teacher", etc.
  showSaveAndAddAnother?: boolean;
}

export function FormDrawerFooter({
  form,
  isUpdate,
  onCancel,
  entityLabel,
  showSaveAndAddAnother = false,
}: FormDrawerFooterProps) {
  return (
    <DrawerFooter>
      <form.Subscribe
        selector={(state) => [state.canSubmit, state.isSubmitting]}
        children={([canSubmit, isSubmitting]) => (
          <div className="flex flex-col gap-2">
            <Button
              disabled={!canSubmit || isSubmitting}
              type="submit"
              onClick={() =>
                form.handleSubmit({
                  submitAction: isUpdate ? "update" : "create",
                  formAction: "close",
                })
              }
            >
              {isSubmitting ? (
                <Loader2 className="size-4 animate-spin" />
              ) : isUpdate ? (
                `Update ${entityLabel}`
              ) : (
                `Create ${entityLabel}`
              )}
            </Button>
            {showSaveAndAddAnother && !isUpdate && (
              <Button
                variant="secondary"
                disabled={!canSubmit || isSubmitting}
                type="submit"
                onClick={() =>
                  form.handleSubmit({
                    submitAction: "create",
                    formAction: "stayopen",
                  })
                }
              >
                Save & Add Another
              </Button>
            )}
          </div>
        )}
      />
      <Button variant="outline" onClick={onCancel}>
        Cancel
      </Button>
    </DrawerFooter>
  );
}
```

#### 1.5 Extract shared `FormMeta` type

Move the repeated `FormMeta` type and `defaultMeta` into a shared module.

```tsx
// lib/form-meta.ts
export type FormMeta = {
  submitAction: "create" | "update" | null;
  formAction: "close" | "stayopen" | null;
};

export const defaultFormMeta: FormMeta = {
  submitAction: null,
  formAction: null,
};
```

---

### Phase 2 — UX Improvements

> Goal: Make forms feel polished, responsive, and forgiving.

#### 2.1 Auto-focus first field on drawer open

Add `autoFocus` to the first `<Input>` rendered inside the drawer. Implement via a ref or by passing `autoFocus` prop through `FormField`.

#### 2.2 Switch validation timing to `onBlur` + `onSubmit`

Replace `onChange` validation with `onBlur` for per-field validation so errors only appear after the user leaves a field, plus `onSubmit` as a safety net. This prevents aggressive red errors while the user is still typing.

```tsx
const form = useForm({
  defaultValues: ...,
  validators: {
    onBlur: formSchema,   // validate when user leaves field
    onSubmit: formSchema,  // validate on submit as fallback
  },
  ...
});
```

Update `FormField` to call `field.handleBlur` on blur:

```tsx
<Input
  ...
  onBlur={field.handleBlur}
/>
```

#### 2.3 Dirty-state confirmation on drawer close

Before closing a drawer with unsaved changes, show a confirmation dialog.

```tsx
const handleOpenChange = (open: boolean) => {
  if (!open && form.state.isDirty) {
    // Show alert dialog: "You have unsaved changes. Discard?"
    setShowDiscardDialog(true);
    return;
  }
  onOpenChange(open);
};
```

Use the existing `AlertDialog` component for this — no new dependencies needed.

#### 2.4 Success toast on save

After a successful mutation, show a toast message using the existing Sonner integration.

```tsx
import { toast } from "sonner";

onSubmit: async ({ value, meta }) => {
  // ... mutation
  toast.success(`${entityLabel} ${meta.submitAction === "create" ? "created" : "updated"} successfully`);
};
```

#### 2.5 Scroll to first error on submit

When submission fails validation, auto-scroll to the first invalid field.

```tsx
// After failed validation
const firstErrorField = document.querySelector('[role="alert"]');
firstErrorField?.scrollIntoView({ behavior: "smooth", block: "center" });
```

---

### Phase 3 — Visual Polish

> Goal: Elevate the visual design from functional to professional.

#### 3.1 Improved field spacing and layout

Replace `pb-4` blocks with consistent `space-y-5` on the parent and remove per-field padding. Use `gap-1.5` inside each field (tighter label-to-input spacing).

**Before:**
```tsx
<div className="p-4 pb-0">
  <div className="pb-4">
    <FormField ... />
  </div>
  <div className="pb-4">
    <FormField ... />
  </div>
</div>
```

**After:**
```tsx
<div className="px-4 space-y-5">
  <FormField ... />
  <FormField ... />
</div>
```

#### 3.2 Two-column layout for short fields

For fields that are naturally short (e.g., Code + Duration Years), render them side by side on wider drawers.

```tsx
<div className="grid grid-cols-2 gap-4">
  <FormField field={codeField} label="Code" required />
  <FormField field={durationField} label="Duration (Years)" required />
</div>
```

#### 3.3 Separator between sections

Use the existing `Separator` component between form sections for visual clarity.

```tsx
<FormSection title="Basic Information">
  <FormField ... />
  <FormField ... />
</FormSection>
<Separator />
<FormSection title="Classification">
  <FormSelectField ... />
</FormSection>
```

#### 3.4 Styled drawer header

Add a subtle bottom border and tighter spacing to the drawer header.

```tsx
<DrawerHeader className="border-b pb-4">
  <DrawerTitle className="text-lg font-semibold">
    {isUpdate ? "Update" : "New"} Course
  </DrawerTitle>
  <DrawerDescription className="text-sm text-muted-foreground">
    Fill in the details below to {isUpdate ? "update this" : "create a new"} course.
  </DrawerDescription>
</DrawerHeader>
```

#### 3.5 Sticky footer

Make the footer stick to the bottom of the drawer so buttons are always visible, even on long forms.

```tsx
<DrawerFooter className="border-t bg-background sticky bottom-0 pt-4">
  ...
</DrawerFooter>
```

#### 3.6 Input styling enhancements

Add subtle transitions and focus ring improvements to inputs:

```css
/* In globals/index.css or via Tailwind config */
input:focus-visible,
textarea:focus-visible {
  ring-width: 2px;
  ring-color: var(--ring);
  transition: box-shadow 150ms ease;
}
```

---

### Phase 4 — Advanced Patterns (as needed)

> Goal: Handle complex form scenarios that appear in specific forms.

#### 4.1 Multi-step form layout (for Teacher form)

The teacher form has many fields (personal info, contact, subjects, profile picture). Break it into collapsible sections or a stepped layout:

```
Step 1: Personal Information  (Name, Employee Number)
Step 2: Contact Details       (Email, Phone)
Step 3: Teaching Assignment   (Subjects multi-select)
Step 4: Profile               (Photo upload)
```

Can use the existing `Tabs` or `Collapsible` component rather than adding a new stepper library.

#### 4.2 Inline field validation icons

Show a subtle green checkmark or red X inside the input's right side when the field is touched and valid/invalid.

```tsx
<div className="relative">
  <Input ... className="pr-8" />
  {field.state.meta.isTouched && (
    field.state.meta.isValid
      ? <CheckCircle className="absolute right-2 top-1/2 -translate-y-1/2 size-4 text-green-500" />
      : <XCircle className="absolute right-2 top-1/2 -translate-y-1/2 size-4 text-destructive" />
  )}
</div>
```

#### 4.3 Optimistic loading skeleton

While mutation data is loading (e.g., college list for the select), show a `Skeleton` in place of the select. Use the existing `Skeleton` component:

```tsx
{isLoadingColleges ? (
  <Skeleton className="h-10 w-full rounded-md" />
) : (
  <FormSelectField ... />
)}
```

---

## Implementation Priority

| Priority | Task | Impact | Effort |
|---|---|---|---|
| 🔴 High | 1.2 Enhance `FormField` (hint, required, aria) | All forms | Small |
| 🔴 High | 1.3 Create `FormSelectField` | All forms with selects | Small |
| 🔴 High | 3.1 Fix spacing consistency | All forms | Small |
| 🔴 High | 2.2 Switch to `onBlur` validation | All forms | Small |
| 🟡 Medium | 1.1 Create `FormSection` | Forms with 4+ fields | Small |
| 🟡 Medium | 1.4 Extract `FormDrawerFooter` | All drawer forms | Small |
| 🟡 Medium | 1.5 Extract shared `FormMeta` | All forms | Trivial |
| 🟡 Medium | 2.4 Success toast | All forms | Trivial |
| 🟡 Medium | 3.4 Styled drawer header | All drawers | Trivial |
| 🟡 Medium | 3.5 Sticky footer | All drawers | Trivial |
| 🟡 Medium | 2.1 Auto-focus first field | All forms | Trivial |
| 🟢 Low | 2.3 Dirty-state confirmation | All forms | Small |
| 🟢 Low | 3.2 Two-column layout | Forms with short fields | Small |
| 🟢 Low | 3.3 Separator between sections | Longer forms | Trivial |
| 🟢 Low | 2.5 Scroll to first error | Long forms | Small |
| 🟢 Low | 4.1 Multi-step layout | Teacher form | Medium |
| 🟢 Low | 4.2 Inline validation icons | All forms | Small |
| 🟢 Low | 4.3 Loading skeleton for selects | Forms with async data | Small |

---

## Applying to the Course Form (Reference Example)

Below is how the course form drawer would look after applying all enhancements. Use this as the reference pattern when updating all other forms.

```tsx
// course-form-drawer.tsx (enhanced)

import { FormSection } from "@/components/form-section";
import { FormField } from "@/components/form-field";
import { FormSelectField } from "@/components/form-select-field";
import { FormDrawerFooter } from "@/components/form-drawer-footer";
import { type FormMeta, defaultFormMeta } from "@/lib/form-meta";
import { toast } from "sonner";

export function CourseFormDrawer({ ... }) {
  const form = useForm({
    defaultValues: { ... },
    validators: {
      onBlur: courseFormSchema,
      onSubmit: courseFormSchema,
    },
    onSubmitMeta: defaultFormMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = courseFormSchema.parse(value);
      if (meta.submitAction === "create") {
        await createNewCourseAsync(formValues);
        toast.success("Course created successfully");
      } else if (meta.submitAction === "update") {
        await updateCourseAsync(formValues);
        toast.success("Course updated successfully");
      }
      if (meta.formAction === "close") setIsOpen(false);
      form.reset();
    },
  });

  return (
    <Drawer direction="right" dismissible={false} open={isOpen} onOpenChange={handleOpenChange}>
      <DrawerTrigger asChild>
        <Button size="sm">
          <Plus className="size-4" />
          Add Course
        </Button>
      </DrawerTrigger>
      <DrawerContent>
        <AuthorizeView policy={isUpdate ? "canUpdateCourse" : "canCreateCourse"} unauthorized={...}>
          <form onSubmit={(e) => { e.preventDefault(); e.stopPropagation(); }}>
            <DrawerHeader className="border-b pb-4">
              <DrawerTitle>{isUpdate ? "Update" : "New"} Course</DrawerTitle>
              <DrawerDescription>
                Fill in the details below to {isUpdate ? "update this" : "create a new"} course.
              </DrawerDescription>
            </DrawerHeader>

            <div className="px-4 py-5 space-y-6 overflow-y-auto">
              <FormSection title="Identification">
                <div className="grid grid-cols-2 gap-4">
                  <form.Field name="code" children={(field) => (
                    <FormField field={field} label="Code" required hint="e.g. BSCS, BSIT" autoFocus />
                  )} />
                  <form.Field name="durationYears" children={(field) => (
                    <FormField field={field} label="Duration (Years)" type="number" required />
                  )} />
                </div>
                <form.Field name="name" children={(field) => (
                  <FormField field={field} label="Name" required hint="Full course name" />
                )} />
              </FormSection>

              <Separator />

              <FormSection title="Details">
                <form.Field name="description" children={(field) => (
                  <FormField field={field} label="Description" type="textarea" required maxLength={500} />
                )} />
              </FormSection>

              <Separator />

              <FormSection title="Classification">
                <form.Field name="collegeId" children={(field) => (
                  <FormSelectField
                    field={field}
                    label="College"
                    required
                    options={collegesOptions}
                    placeholder="Select a college"
                    searchPlaceholder="Search colleges..."
                    emptyMessage="No college found"
                  />
                )} />
              </FormSection>
            </div>

            <FormDrawerFooter
              form={form}
              isUpdate={isUpdate}
              onCancel={() => setIsOpen(false)}
              entityLabel="Course"
              showSaveAndAddAnother
            />
          </form>
        </AuthorizeView>
      </DrawerContent>
    </Drawer>
  );
}
```

---

## Checklist for Applying to Any Form

When enhancing a form, follow this checklist:

- [ ] Import shared `FormMeta` / `defaultFormMeta` from `@/lib/form-meta`
- [ ] Switch validators from `onChange` to `onBlur` + `onSubmit`
- [ ] Replace per-field `<div className="pb-4">` wrappers with `space-y-5` on parent
- [ ] Add `required` prop to mandatory `FormField` instances
- [ ] Add `hint` prop where input format is non-obvious
- [ ] Replace inline `SearchableSelect` + error blocks with `FormSelectField`
- [ ] Wrap related fields in `FormSection` with a descriptive `title`
- [ ] Add `Separator` between sections
- [ ] Use `FormDrawerFooter` instead of inline subscribe/button block
- [ ] Add `toast.success(...)` in `onSubmit` after successful mutation
- [ ] Add `autoFocus` to the first field
- [ ] Add `border-b` to `DrawerHeader` and `border-t sticky bottom-0` to `DrawerFooter`
- [ ] Update `DrawerDescription` to be specific and action-oriented
- [ ] Test keyboard navigation: Tab through fields, Escape to cancel
