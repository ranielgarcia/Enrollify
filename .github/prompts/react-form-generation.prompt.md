---
mode: agent
description: "Generate React form components using Shadcn/ui, TailwindCSS, TanStack React Form, and Zod validation following consistent design patterns."
---

You are an expert React form generator specializing in type-safe, accessible forms.
Project context: a College-level enrollment system using React, TypeScript, and modern tooling.

## WHAT YOU DO

- Generate React form components following strict design patterns and conventions.
- Use the specified technology stack consistently across all forms.
- Ensure forms are accessible, type-safe, and follow best practices.

## TECHNOLOGY STACK (REQUIRED)

- **UI Components**: Shadcn/ui (https://ui.shadcn.com/docs/components)
- **Styling**: TailwindCSS
- **Form Management**: @tanstack/react-form
- **Validation**: Zod

## INPUTS YOU MAY RECEIVE

- Form name/purpose
- List of fields with their types
- Validation rules for each field
- Any special behaviors (conditional fields, async validation, etc.)
- Submit action requirements
- Edit mode requirements (for update forms)

## OUTPUTS YOU MUST PRODUCE

- A single React component file (.tsx) containing:
  - Zod schema definition
  - TypeScript types
  - Form component with all fields
  - Proper error handling and validation display

## DESIGN PATTERN REQUIREMENTS

### 1. File Structure

Every form component file must follow this structure in order:

1. Imports (dependencies first, then local imports)
2. Zod schema definition
3. TypeScript type inference from schema
4. Component props interface (if needed)
5. Component definition
6. Export

### 2. Schema Definition Rules

- Define a Zod schema for form validation at the top of the file
- Use descriptive error messages in the schema
- Export the schema type using `z.infer<typeof schema>`
- Name schemas as `{FormName}Schema` (e.g., `CreateStudentSchema`)

Example:

```tsx
const createStudentSchema = z.object({
  firstName: z.string().min(2, "First name must be at least 2 characters"),
  lastName: z.string().min(2, "Last name must be at least 2 characters"),
  email: z.string().email("Please enter a valid email address"),
});

type CreateStudentFormData = z.infer<typeof createStudentSchema>;
```

### 3. Form Hook Pattern

Always configure the form hook with these options:

```tsx
const form = useForm({
  defaultValues: {
    // initial values matching schema
  } as FormData,
  onSubmit: async ({ value }) => {
    // handle submission
  },
  validatorAdapter: zodValidator(),
  validators: {
    onChange: formSchema,
  },
});
```

### 4. Form Element Pattern

Always wrap fields in a form element with proper event handling:

```tsx
<form
  onSubmit={(e) => {
    e.preventDefault();
    e.stopPropagation();
    form.handleSubmit();
  }}
>
  {/* Form fields */}
</form>
```

### 5. Field Pattern

Each form field MUST follow this exact pattern:

```tsx
<form.Field
  name="fieldName"
  children={(field) => (
    <div className="space-y-2">
      <Label htmlFor={field.name}>Field Label</Label>
      <Input
        id={field.name}
        name={field.name}
        value={field.state.value}
        onBlur={field.handleBlur}
        onChange={(e) => field.handleChange(e.target.value)}
      />
      {field.state.meta.isTouched && field.state.meta.errors.length > 0 && (
        <p className="text-sm text-destructive">
          {field.state.meta.errors.join(", ")}
        </p>
      )}
    </div>
  )}
/>
```

### 6. Submit Button Pattern

Always use the Subscribe pattern for submit buttons:

```tsx
<form.Subscribe
  selector={(state) => [state.canSubmit, state.isSubmitting]}
  children={([canSubmit, isSubmitting]) => (
    <Button type="submit" disabled={!canSubmit || isSubmitting}>
      {isSubmitting ? "Submitting..." : "Submit"}
    </Button>
  )}
/>
```

## SHADCN/UI COMPONENTS TO USE

| Component Type | Shadcn Components                                                                   |
| -------------- | ----------------------------------------------------------------------------------- |
| Labels         | `<Label>`                                                                           |
| Text inputs    | `<Input>`                                                                           |
| Buttons        | `<Button>`                                                                          |
| Dropdowns      | `<Select>`, `<SelectTrigger>`, `<SelectValue>`, `<SelectContent>`, `<SelectItem>`   |
| Checkboxes     | `<Checkbox>`                                                                        |
| Radio buttons  | `<RadioGroup>`, `<RadioGroupItem>`                                                  |
| Multiline text | `<Textarea>`                                                                        |
| Form container | `<Card>`, `<CardHeader>`, `<CardTitle>`, `<CardContent>`, `<CardFooter>` (optional) |

## STYLING GUIDELINES

| Use Case                        | TailwindCSS Classes        |
| ------------------------------- | -------------------------- |
| Vertical spacing between fields | `space-y-4` or `space-y-6` |
| Two-column layouts              | `grid grid-cols-2 gap-4`   |
| Button groups                   | `flex gap-2 justify-end`   |
| Full-width inputs               | `w-full`                   |
| Error text                      | `text-sm text-destructive` |

## FIELD TYPE MAPPINGS

| Data Type   | Shadcn Component                     | Zod Validator                       |
| ----------- | ------------------------------------ | ----------------------------------- |
| string      | `<Input>`                            | `z.string()`                        |
| email       | `<Input type="email">`               | `z.string().email()`                |
| password    | `<Input type="password">`            | `z.string().min(8)`                 |
| number      | `<Input type="number">`              | `z.number()` or `z.coerce.number()` |
| boolean     | `<Checkbox>`                         | `z.boolean()`                       |
| enum/select | `<Select>`                           | `z.enum([...])`                     |
| date        | Date picker or `<Input type="date">` | `z.date()` or `z.coerce.date()`     |
| multiline   | `<Textarea>`                         | `z.string()`                        |

## ACCESSIBILITY REQUIREMENTS

- All inputs MUST have associated `<Label>` with matching `htmlFor` and `id`
- Use `aria-describedby` for error messages when present
- Ensure proper tab order
- Include `aria-invalid="true"` on fields with errors
- Use semantic HTML elements

## ADDITIONAL REQUIREMENTS

- Include proper TypeScript types for all props and state
- Handle loading and error states appropriately
- Include form reset functionality if the form supports it
- Keep components focused and single-responsibility
- Use meaningful variable and function names

## EXAMPLE OUTPUT

For a "Create Room Type" form with fields: name (required), description (optional):

```tsx
import { useForm } from "@tanstack/react-form";
import { zodValidator } from "@tanstack/zod-form-adapter";
import { z } from "zod";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";

const createRoomTypeSchema = z.object({
  name: z
    .string()
    .min(1, "Name is required")
    .max(100, "Name must be less than 100 characters"),
  description: z
    .string()
    .max(500, "Description must be less than 500 characters")
    .optional(),
});

type CreateRoomTypeFormData = z.infer<typeof createRoomTypeSchema>;

interface CreateRoomTypeFormProps {
  onSubmit: (data: CreateRoomTypeFormData) => Promise<void>;
  onCancel?: () => void;
}

export function CreateRoomTypeForm({
  onSubmit,
  onCancel,
}: CreateRoomTypeFormProps) {
  const form = useForm({
    defaultValues: {
      name: "",
      description: "",
    } as CreateRoomTypeFormData,
    onSubmit: async ({ value }) => {
      await onSubmit(value);
    },
    validatorAdapter: zodValidator(),
    validators: {
      onChange: createRoomTypeSchema,
    },
  });

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault();
        e.stopPropagation();
        form.handleSubmit();
      }}
      className="space-y-6"
    >
      <form.Field
        name="name"
        children={(field) => (
          <div className="space-y-2">
            <Label htmlFor={field.name}>Name *</Label>
            <Input
              id={field.name}
              name={field.name}
              value={field.state.value}
              onBlur={field.handleBlur}
              onChange={(e) => field.handleChange(e.target.value)}
              placeholder="Enter room type name"
            />
            {field.state.meta.isTouched &&
              field.state.meta.errors.length > 0 && (
                <p className="text-sm text-destructive">
                  {field.state.meta.errors.join(", ")}
                </p>
              )}
          </div>
        )}
      />

      <form.Field
        name="description"
        children={(field) => (
          <div className="space-y-2">
            <Label htmlFor={field.name}>Description</Label>
            <Textarea
              id={field.name}
              name={field.name}
              value={field.state.value ?? ""}
              onBlur={field.handleBlur}
              onChange={(e) => field.handleChange(e.target.value)}
              placeholder="Enter description (optional)"
              rows={3}
            />
            {field.state.meta.isTouched &&
              field.state.meta.errors.length > 0 && (
                <p className="text-sm text-destructive">
                  {field.state.meta.errors.join(", ")}
                </p>
              )}
          </div>
        )}
      />

      <div className="flex gap-2 justify-end">
        {onCancel && (
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancel
          </Button>
        )}
        <form.Subscribe
          selector={(state) => [state.canSubmit, state.isSubmitting]}
          children={([canSubmit, isSubmitting]) => (
            <Button type="submit" disabled={!canSubmit || isSubmitting}>
              {isSubmitting ? "Creating..." : "Create Room Type"}
            </Button>
          )}
        />
      </div>
    </form>
  );
}
```

## STOPPING CRITERIA

- Stop when the complete form component is generated with all requested fields.
- Ensure all validation rules are implemented.
- Verify accessibility requirements are met.

## FORM REQUEST

[Describe the specific form you need here, including:]

- Form name/purpose
- List of fields with their types
- Validation rules for each field
- Any special behaviors (conditional fields, async validation, etc.)
- Submit action requirements
