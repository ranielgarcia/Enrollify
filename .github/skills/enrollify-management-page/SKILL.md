---
name: enrollify-management-page
description: >
  Step-by-step guide for implementing a fully integrated management page in the
  Enrollify frontend. Use this skill when asked to implement, wire up, or
  complete a CRUD management page (e.g., Teachers, Rooms, Buildings, Courses)
  in the React/TypeScript frontend located at
  src/system/enrollify-frontend/src. Covers model schema, API collection,
  search params, data table with advanced filtering, form drawer with real API
  mutations, delete dialog, and page index — all following the subjects page pattern.
---

# Enrollify Management Page Integration Skill

This skill encodes the complete, battle-tested process for implementing a fully
API-integrated management page in the Enrollify frontend. Follow every phase in
order. Do not skip phases.

---

## Reference Files (Read These First)

Before writing any code, always read these reference implementations:

- **Pattern to follow:** `src/page-components/subjects-management-page/`
- **Model schema:** `src/api/models/subject.ts`
- **Collection:** `src/api/collections/subject-collection.ts`
- **Hooks:** `src/hooks/use-crud-state.ts`, `src/hooks/use-debounce.ts`
- **Infrastructure:** `src/hooks/create-mutation-options.ts`, `src/hooks/create-query-options.ts`
- **Auth:** `src/infrastructure/authorization/policies/authorization-policies.ts`
- **Form components:** `src/components/form/form-field.tsx`, `src/components/form/form-select-field.tsx`, `src/components/form/form-drawer-footer.tsx`

---

## Phase 0 — Pre-flight: API Type Generation

1. Check `src/api/generated/api.ts` to verify the entity's DTO has all expected
   fields (especially `id`). The generated file is derived from the backend
   OpenAPI schema via `npm run generate:api:win` (Windows) or
   `npm run generate:api` (Linux/Mac).

2. If the backend was recently updated (new field added, endpoint added), run:
   ```
   npm run generate:api:win
   ```
   from `src/system/enrollify-frontend/` before proceeding.

3. After regeneration, find the entity's DTO in `api.ts`. Value object fields
   (e.g., `teacherIdentifier`, `email`, `phoneNumber`) are typed as named
   schemas but all resolve to plain `string` in TypeScript — use `z.string()`
   for all of them in your Zod schema.

4. Check whether all nested DTOs (e.g., `DepartmentSummaryDto`) have optional
   or required fields. If the backend marks them optional, all nested schema
   fields must use `.optional()`.

5. Determine whether a DELETE endpoint exists. If it does not exist, the delete
   dialog must show an informational "not supported" message instead of calling
   a mutation.

---

## Phase 1 — Model Schema (`src/api/models/ENTITY.ts`)

Create or rewrite the Zod schema to exactly match the API response DTO.

### Rules:
- **Always** extend `AuditInfoSchema` at the end:
  ```ts
  import { AuditInfoSchema } from "@/api/models/audit-info";
  export const EntitySchema = z.object({ ... }).extend(AuditInfoSchema.shape);
  export type Entity = z.infer<typeof EntitySchema>;
  ```
- `id` must be `z.number()` (not optional — the entity must always have an id
  after creation).
- Fields that are nullable in the API response use `.nullable().optional()`.
- Fields with default empty strings use `.default("")` (safe on model side).
- Nested summary DTOs (e.g., department, college) must mirror the backend's
  optional/required exactly. If all nested fields are optional in the DTO, use:
  ```ts
  const EntityRelatedSchema = z.object({
    id: z.number().optional(),
    name: z.string().optional(),
  });
  ```
- Do NOT include fields that do not exist in the API response (e.g., `college`,
  `subjects` if the endpoint does not return them).

### Example (Teacher):
```ts
import { z } from "zod";
import { AuditInfoSchema } from "@/api/models/audit-info";

const TeacherDepartmentSchema = z.object({
  id: z.number().optional(),
  code: z.string().optional(),
  name: z.string().optional(),
});

export const TeacherSchema = z
  .object({
    id: z.number(),
    firstName: z.string(),
    lastName: z.string(),
    middleName: z.string().default(""),
    teacherIdentifier: z.string(),
    email: z.string(),
    phoneNumber: z.string(),
    department: TeacherDepartmentSchema.optional(),
    academicTitle: z.string().nullable().optional(),
    qualification: z.string().nullable().optional(),
    specialization: z.string().nullable().optional(),
    officeLocation: z.string().nullable().optional(),
    officeHours: z.string().nullable().optional(),
    biography: z.string().nullable().optional(),
  })
  .extend(AuditInfoSchema.shape);

export type Teacher = z.infer<typeof TeacherSchema>;
```

---

## Phase 2 — API Collection (`src/api/collections/ENTITY-collection.ts`)

Create query options (for fetching) and mutation options (for create/update/delete).

### Rules:
- Use `createQueryOptions` from `@/hooks/create-query-options`.
- Use `createMutationOptions` from `@/hooks/create-mutation-options`.
- For paginated filter queries, pass `Filters`, `Sort`, `JoinOperator` as
  query params.
- The `select` function must handle empty/string responses gracefully (the
  backend may return an empty string when no results exist).
- Add `isMultipart: true` to create/update options if the endpoint accepts
  `multipart/form-data` (e.g., when a photo/file upload is supported).
- Use `toast.success()` from `sonner` in the `onSuccess` callback.
- Use `meta: { invalidateQueries: [queryKeys.base()] }` to auto-invalidate
  cache after mutations.
- Only add `queryKeys.delete` if a DELETE endpoint actually exists.

### queryKeys pattern:
```ts
const queryKeys = {
  base: () => ["entities"],
  filter: (page, pageSize, filters, sort, joinOperator) =>
    [...queryKeys.base(), "search", page, pageSize, filters, sort, joinOperator],
  create: () => [...queryKeys.base(), "create"],
  update: (id: number) => [...queryKeys.base(), "update", id],
  delete: (id: number) => [...queryKeys.base(), "delete", id], // Only if DELETE exists
};
```

### filterPaginated options pattern:
```ts
export const filterEntitiesPaginatedOptions = (
  page: number,
  pageSize: number,
  filters: ExtendedColumnFilter<Entity>[],
  sort: ExtendedColumnSort<Entity>[],
  joinOperator: string,
) =>
  createQueryOptions({
    path: "/api/entities/filter/{page}/{pageSize}",
    pathParams: { page, pageSize },
    params: {
      Filters: filters.length ? JSON.stringify(filters) : undefined,
      Sort: sort.length ? JSON.stringify(sort) : undefined,
      JoinOperator: joinOperator,
    },
    options: {
      queryKey: queryKeys.filter(page, pageSize, filters, sort, joinOperator),
      staleTime: 1000 * 60 * 2,
      select: (pagedResults): PagedResult<Entity> => {
        if (!pagedResults || (typeof pagedResults === "string" && pagedResults === "")) {
          return { items: [], page, pageSize, totalCount: 0, totalPages: 0 };
        }
        const data = typeof pagedResults === "string" ? JSON.parse(pagedResults) : pagedResults;
        return pagedEntitiesSchema.parse(data);
      },
    },
  });
```

### multipart create/update pattern:
```ts
export const createEntityOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/entities",
    isMultipart: true,   // <-- Add when endpoint accepts multipart/form-data
    mutationKey: queryKeys.create(),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => toast.success("Entity created successfully"),
    },
  });
```

---

## Phase 3 — Search Params (`searchParams.ts` in the page folder)

Create a `searchParams.ts` file in the page component folder. This uses `nuqs`
to sync pagination, filtering, and sorting state with the URL.

```ts
import { parseAsInteger, parseAsString } from "nuqs";
import { getFiltersStateParser, getSortingStateParser } from "@/lib/parsers";
import type { Entity } from "@/api/models/entity";

export const searchParams = {
  page: parseAsInteger.withDefault(1),
  perPage: parseAsInteger.withDefault(10),
  filters: getFiltersStateParser<Entity>().withDefault([]),
  sort: getSortingStateParser<Entity>().withDefault([]),
  joinOperator: parseAsString.withDefault("and"),
};
```

This file is identical for every management page — only the imported `Entity`
type changes.

---

## Phase 4 — Table Action Bar (`ENTITY-table-action-bar.tsx`)

A floating action bar that appears when rows are selected. Provides bulk
actions (typically just Export).

```tsx
import type { Table } from "@tanstack/react-table";
import { Download, X } from "lucide-react";
import * as React from "react";
import {
  ActionBar, ActionBarClose, ActionBarGroup,
  ActionBarItem, ActionBarSelection, ActionBarSeparator,
} from "@/components/ui/action-bar";
import type { Entity } from "@/api/models/entity";

interface EntityTableActionBarProps {
  table: Table<Entity>;
}

export function EntityTableActionBar({ table }: EntityTableActionBarProps) {
  const rows = table.getFilteredSelectedRowModel().rows;

  const onOpenChange = React.useCallback((open: boolean) => {
    if (!open) table.toggleAllRowsSelected(false);
  }, [table]);

  const onExport = React.useCallback(() => {
    // exportTableToCSV(table, { excludeColumns: ["select", "actions", "avatar"], onlySelected: true });
  }, [table]);

  return (
    <ActionBar open={rows.length > 0} onOpenChange={onOpenChange}>
      <ActionBarSelection>
        <span className="font-medium">{rows.length}</span>
        <span>selected</span>
        <ActionBarSeparator />
        <ActionBarClose><X /></ActionBarClose>
      </ActionBarSelection>
      <ActionBarSeparator />
      <ActionBarGroup>
        <ActionBarItem onClick={onExport}>
          <Download />
          Export
        </ActionBarItem>
      </ActionBarGroup>
    </ActionBar>
  );
}
```

---

## Phase 5 — Data Table (`ENTITY-table.tsx`)

The main data table component with advanced filtering, sorting, and pagination.

### Key rules:
- Accept `pagedEntities: PagedResult<Entity>` and `onEdit`/`onDelete` callbacks
  as props.
- Use `useDataTable` hook with `manualPagination: true`.
- Use `DataTableAdvancedToolbar` containing `DataTableSortList` and
  `DataTableFilterList`.
- Pass `shallow`, `debounceMs`, `throttleMs` from `useDataTable` to
  `DataTableFilterList`.
- Use `useTablePermissions(updatePolicy, deletePolicy)` for row action buttons.
  If there is no delete, pass the update policy for both args:
  `useTablePermissions("canUpdateEntity", "canUpdateEntity")`.
- Columns that should be filterable must have:
  `enableColumnFilter: true` and `meta: { label: "...", variant: "text" }`
  (or `variant: "range"` for numbers).
- Add an `avatar` display column (first) showing initials if the entity has
  name fields.
- Add `actions` display column (last) with Edit (and Delete if supported)
  buttons.
- Wrap columns in `useMemo` with `[canUpdate, canDelete, onEdit, onDelete]`
  as deps.

### Structure:
```tsx
export function EntityTable({ pagedEntities, onEdit, onDelete }: EntityTableProps) {
  const { canUpdate, canDelete } = useTablePermissions("canUpdateEntity", "canDeleteEntity");
  const columnHelper = createColumnHelper<Entity>();

  const columns = useMemo(() => [
    // avatar column (display)
    columnHelper.display({ id: "avatar", ... }),
    // data columns (accessor)
    columnHelper.accessor("fieldName", {
      id: "fieldName",
      header: ({ column }) => <DataTableColumnHeader column={column} label="Field Name" />,
      meta: { label: "Field Name", variant: "text" },
      enableColumnFilter: true,
      cell: (info) => <span>{info.getValue()}</span>,
    }),
    // audit columns (createdAt, createdBy, updatedAt, updatedBy)
    // actions column (display)
    columnHelper.display({
      id: "actions",
      header: "Actions",
      enableHiding: false,
      cell: (info) => {
        const item = info.row.original;
        return (
          <div className="flex gap-2">
            <Button variant="ghost" size="sm" onClick={() => onEdit(item)} disabled={!canUpdate}>
              <Edit2 className="size-4" />
            </Button>
            {/* Only include delete if DELETE endpoint exists: */}
            <Button variant="ghost" size="sm" onClick={() => onDelete(item)} disabled={!canDelete}>
              <Trash2 className="size-4" />
            </Button>
          </div>
        );
      },
    }),
  ], [canUpdate, canDelete, onEdit, onDelete]);

  const { table, shallow, debounceMs, throttleMs } = useDataTable({
    data: pagedEntities?.items ?? [],
    columns,
    pageCount: pagedEntities?.totalPages ?? -1,
    manualPagination: true,
    debounceMs: 600,
  });

  return (
    <DataTable table={table} actionBar={<EntityTableActionBar table={table} />}>
      <DataTableAdvancedToolbar table={table}>
        <DataTableSortList table={table} align="start" />
        <DataTableFilterList
          table={table}
          shallow={shallow}
          debounceMs={debounceMs}
          throttleMs={throttleMs}
          align="start"
        />
      </DataTableAdvancedToolbar>
    </DataTable>
  );
}
```

---

## Phase 6 — Form Drawer (`ENTITY-form-drawer.tsx`)

The create/update form drawer. This is the most complex component.

### Key rules:

#### Zod schema for the form:
- **CRITICAL:** Do NOT use `z.email()` (Zod v4 shorthand). Use `z.string().email()`
  instead — `z.email()` creates a `ZodEmail` type that is incompatible with
  TanStack Form's `FormValidateOrFn`.
- **CRITICAL:** Do NOT use `z.string().default("")` in the form schema. This
  creates `ZodDefault<ZodString>` which changes the inferred input type and
  causes a validator type error. Use plain `z.string()` and set defaults in
  `defaultFormValues` instead.
- Optional fields should be `z.string().optional()` in the form schema (not
  `.nullable()` — the form doesn't deal with null).

#### TanStack Form setup:
```ts
const form = useForm({
  defaultValues: defaultFormValues,
  validators: {
    onBlur: entityFormSchema,
    onSubmit: entityFormSchema,
  },
  onSubmitMeta: defaultFormMeta as FormMeta,
  onSubmit: async ({ value, meta }) => { ... },
});
```

#### multipart/FormData submission:
When the endpoint is `multipart/form-data`, build a `FormData` and cast to
`any` (necessary because TypeScript infers `unknown` for multipart endpoints):
```ts
const formData = new FormData();
formData.append("firstName", value.firstName);
if (value.optionalField) formData.append("optionalField", value.optionalField);
if (photoFile) formData.append("photo", photoFile);

if (meta.submitAction === "create") {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  await createEntityAsync(formData as any);
} else {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  await updateEntityAsync(formData as any);
}
```

#### Mutations:
```ts
const { mutateAsync: createEntityAsync } = useMutation(createEntityOptions());
const { mutateAsync: updateEntityAsync } = useMutation(updateEntityOptions(entityToUpdate?.id ?? 0));
```

#### Related data (e.g., department dropdown with college filter):
- When the form has a department selector that should be filterable by college:
  - Fetch `getAllDepartmentsOptions()` — the result includes
    `department.college: { id, name }`.
  - Derive college options using a `Map` to deduplicate:
    ```ts
    const collegeOptions = Array.from(
      new Map(
        departments
          .filter((d) => d.college)
          .map((d) => [d.college.id, { value: d.college.id.toString(), label: d.college.name }])
      ).values()
    );
    ```
  - Store the selected college as `useState<string>` (local state, NOT a form
    field — college is UI-only and must NOT be submitted to the API).
  - When college changes, reset `departmentId` to `0`:
    ```tsx
    onValueChange={(val) => {
      setSelectedCollegeId(val);
      form.setFieldValue("departmentId", 0);
    }}
    ```

#### `FormSelectField` and number conversion:
`FormSelectField` calls `field.handleChange(Number.isNaN(parsed) ? val : parsed)`,
so the stored value is automatically converted to a number. `departmentId: z.number()`
works correctly — do not fight it.

#### AuthorizeView wrapper:
Wrap the form content in `AuthorizeView` to show an unauthorized message when
the user lacks create/update permission:
```tsx
<AuthorizeView
  policy={isUpdating ? "canUpdateEntity" : "canCreateEntity"}
  unauthorized={
    <Unauthorized
      message="Your current role does not have the necessary permissions."
      buttonLabel="Back to Home"
      backCallback={() => setIsOpen(false)}
      redirectOptions={{ to: "/portal" }}
    />
  }
>
  {/* form content */}
</AuthorizeView>
```

#### Form sections:
Use `FormSection` to group related fields, with `Separator` between sections.
Use `FormField` for text/textarea inputs. Use `FormSelectField` with
`SearchableSelect` options for dropdowns.

#### Drawer structure:
```tsx
<Drawer direction="right" dismissible={false} open={isOpen} onOpenChange={onOpenChange}>
  <DrawerTrigger asChild>
    <Button ...><Plus className="size-4" /> Add Entity</Button>
  </DrawerTrigger>
  <DrawerContent>
    <AuthorizeView ...>
      <form className="flex flex-col overflow-hidden h-full" onSubmit={...}>
        <DrawerHeader className="border-b pb-4">...</DrawerHeader>
        <div className="flex-1 overflow-y-auto px-4 py-5 space-y-6">
          {/* FormSection blocks with Separators */}
        </div>
        <FormDrawerFooter form={form} isUpdate={isUpdating} onCancel={() => setIsOpen(false)} entityLabel="Entity" />
      </form>
    </AuthorizeView>
  </DrawerContent>
</Drawer>
```

---

## Phase 7 — Delete Alert Dialog (`delete-ENTITY-alert-dialog.tsx`)

### If DELETE endpoint EXISTS:
```tsx
import { deleteEntityOptions } from "@/api/collections/entity-collection";
import { DeleteAlertDialog } from "@/components/delete-alert-dialog";

export function DeleteEntityAlertDialog({ entityToDelete, isOpen, onOpenChange }) {
  return (
    <DeleteAlertDialog
      entityToDelete={entityToDelete}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      entityLabel="Entity"
      getEntityName={(e) => e.name}
      deleteMutationOptions={deleteEntityOptions(entityToDelete?.id ?? 0)}
    />
  );
}
```

### If DELETE endpoint does NOT EXIST:
Remove `deleteMutationOptions` prop and use `onConfirmDelete` + `customDialogDescription`:
```tsx
export function DeleteEntityAlertDialog({ entityToDelete, isOpen, onOpenChange }) {
  return (
    <DeleteAlertDialog
      entityToDelete={entityToDelete}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      entityLabel="Entity"
      getEntityName={(e) => `${e.firstName} ${e.lastName}`}
      onConfirmDelete={() => onOpenChange(false)}
      customDialogDescription={
        <span>
          Entity deletion is not currently supported. Please contact your
          system administrator to remove records.
        </span>
      }
    />
  );
}
```

---

## Phase 8 — Page Index (`index.tsx`)

The page entry point orchestrates all state and data fetching.

### Key rules:
- Use `useSuspenseQuery` (not `useQuery`) for all data fetching — the page is
  wrapped in a Suspense boundary by the router.
- Debounce filters and sort before passing to query options (prevents a query
  per keystroke):
  ```ts
  const debouncedFilters = useDebounce(filters, 600);
  const debouncedSort = useDebounce(sort, 600);
  ```
- Use `useQueryStates(searchParams)` from `nuqs` to read URL state.
- Use `useCrudState<Entity>()` for form open/close, entity to edit, entity to
  delete state management.
- Use `ManagementPageLayout` as the outer wrapper.
- Pass the `TeacherFormDrawer` (or equivalent) as `createNewItemButton` prop —
  it contains its own `DrawerTrigger`, so it renders the "Add" button itself.
- Only include delete dialog rendering if the DELETE endpoint exists.

### Structure:
```tsx
export default function EntityManagementPage() {
  const [{ page, perPage, filters, sort, joinOperator }] = useQueryStates(searchParams);

  const { isFormOpen, entityToEdit, entityToDelete, handleEdit, handleDelete,
          handleFormOpenChange, handleDeleteDialogOpenChange } = useCrudState<Entity>();

  const currentPage = page ? Number(page) : 1;
  const currentPageSize = perPage ? Number(perPage) : 10;
  const debouncedFilters = useDebounce(filters, 600);
  const debouncedSort = useDebounce(sort, 600);

  const { data: pagedEntities } = useSuspenseQuery(
    filterEntitiesPaginatedOptions(currentPage, currentPageSize, debouncedFilters, debouncedSort, joinOperator)
  );
  const { data: relatedData } = useSuspenseQuery(getAllRelatedDataOptions()); // e.g., departments

  return (
    <ManagementPageLayout
      title="Entity Management"
      description="Manage entities and their details"
      icon={<SomeLucideIcon />}
      createNewItemButton={
        <EntityFormDrawer
          relatedData={relatedData}
          isOpen={isFormOpen}
          setIsOpen={handleFormOpenChange}
          onOpenChange={handleFormOpenChange}
          entityToUpdate={entityToEdit}
        />
      }
    >
      <EntityTable
        pagedEntities={pagedEntities}
        onEdit={handleEdit}
        onDelete={handleDelete}   // Only if DELETE endpoint exists
      />

      {/* Only if DELETE endpoint exists: */}
      <DeleteEntityAlertDialog
        isOpen={!!entityToDelete}
        onOpenChange={handleDeleteDialogOpenChange}
        entityToDelete={entityToDelete}
      />
    </ManagementPageLayout>
  );
}
```

---

## Phase 9 — Cleanup and Verification

1. **Delete dummy data files:** Any `*-dummy-data.ts` files in
   `src/api/models/` that are no longer imported must be deleted. They cause
   TypeScript errors when the model type changes.

2. **Verify no dummy data imports remain:** Search for the dummy data file name
   across the codebase before deleting.

3. **TypeScript check:** Run `npx tsc --noEmit --project tsconfig.app.json`
   from `src/system/enrollify-frontend/`. Only pre-existing errors in
   unrelated files (e.g., `delete-alert-dialog.tsx`, `navigation-config.ts`,
   `use-table-permissions.ts`, `AuthorizationHelpers.ts`, `UserContext.ts`)
   are acceptable — zero new errors in the entity's files.

4. **Pre-existing errors to ignore** (do NOT fix these as they are unrelated):
   - `src/api/api-dtos/UserContext.ts` — missing schema in generated types
   - `src/components/delete-alert-dialog.tsx` — unused React import
   - `src/components/navigation/navigation-config.ts` — placeholder `#` URLs
   - `src/hooks/use-table-permissions.ts` — string not assignable to policy union
   - `src/infrastructure/authorization/helpers/AuthorizationHelpers.ts` — implicit `any`
   - `src/page-components/*/delete-*-alert-dialog.tsx` — `UseMutationOptions` type mismatch

---

## Common Pitfalls and Solutions

| Pitfall | Solution |
|---|---|
| `z.email()` causes TanStack Form type error | Use `z.string().email()` instead |
| `z.string().default("")` causes TanStack Form type error | Use `z.string()` in schema; set `""` in `defaultFormValues` |
| `mutateAsync(formData)` TypeScript error for multipart endpoints | Cast to `any`: `mutateAsync(formData as any)` |
| College dropdown not resetting when college changes | Call `form.setFieldValue("departmentId", 0)` in college's `onValueChange` |
| `FormSelectField` stores string instead of number | It auto-converts — use `z.number()` and it works |
| Nested DTO fields optional in API but required in schema | Make all nested schema fields `.optional()` |
| `getAllDepartmentsOptions` missing college | It returns full `Department[]` with `.college: { id, name }` nested — derive college list from it |
| Delete button visible but no endpoint | Remove delete column from table; show informational dialog instead |
| Page shows stale data after create/update | Verify `meta: { invalidateQueries: [queryKeys.base()] }` is set on mutation options |
| Empty page on first load | Ensure `useSuspenseQuery` is used (not `useQuery`) — the page needs Suspense |

---

## Authorization Policy Naming Convention

Authorization policies for new entities follow this pattern
(from `src/infrastructure/authorization/policies/authorization-policies.ts`):

```
canViewEntity      (read/list)
canCreateEntity    (create)
canUpdateEntity    (update)
canDeleteEntity    (delete — only add if endpoint exists)
```

Use camelCase with the full entity name. Check `authorization-policies.ts` to
see what policies actually exist before referencing them. If a policy does not
exist for your entity, coordinate with the backend team to add it.

---

## File Checklist

When implementing a new management page, you should create or modify:

- [ ] `src/api/models/ENTITY.ts` — Zod schema + TypeScript type
- [ ] `src/api/collections/ENTITY-collection.ts` — query + mutation options
- [ ] `src/page-components/ENTITY-management-page/searchParams.ts` — nuqs URL state
- [ ] `src/page-components/ENTITY-management-page/ENTITY-table-action-bar.tsx` — bulk action bar
- [ ] `src/page-components/ENTITY-management-page/ENTITY-table.tsx` — data table + advanced toolbar
- [ ] `src/page-components/ENTITY-management-page/ENTITY-form-drawer.tsx` — create/update form
- [ ] `src/page-components/ENTITY-management-page/delete-ENTITY-alert-dialog.tsx` — delete confirmation
- [ ] `src/page-components/ENTITY-management-page/index.tsx` — page entry point
- [ ] Delete `src/api/models/ENTITY-dummy-data.ts` if it exists
