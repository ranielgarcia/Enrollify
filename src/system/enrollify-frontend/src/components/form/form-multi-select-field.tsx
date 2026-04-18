import type { AnyFieldApi } from "@tanstack/react-form";
import {
  MultiSearchableSelect,
  type MultiSearchableSelectOption,
} from "@/components/form/multi-searchable-select";
import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";

interface FormMultiSelectFieldProps {
  field: AnyFieldApi;
  label: string;
  options: MultiSearchableSelectOption[];
  placeholder?: string;
  searchPlaceholder?: string;
  emptyMessage?: string;
  required?: boolean;
  hint?: string;
  className?: string;
  disabled?: boolean;
  maxDisplay?: number;
}

export function FormMultiSelectField({
  field,
  label,
  options,
  placeholder,
  searchPlaceholder,
  emptyMessage,
  required,
  hint,
  className,
  disabled,
  maxDisplay,
}: FormMultiSelectFieldProps) {
  const errorMessage = field.state.meta.errors
    .map((e: { message?: string } | string) =>
      typeof e === "string" ? e : e?.message,
    )
    .join(", ");
  const hasError = !field.state.meta.isValid && errorMessage;

  const describedBy =
    [
      hint && !hasError ? `${field.name}-hint` : null,
      hasError ? `${field.name}-error` : null,
    ]
      .filter(Boolean)
      .join(" ") || undefined;

  return (
    <div className={cn("grid w-full items-center gap-1.5", className)}>
      <Label htmlFor={field.name}>
        {label}
        {required && <span className="text-destructive ml-0.5">*</span>}
      </Label>
      <MultiSearchableSelect
        options={options}
        value={field.state.value}
        onValueChange={(val) => field.handleChange(val)}
        name={field.name}
        placeholder={placeholder}
        searchPlaceholder={searchPlaceholder}
        emptyMessage={emptyMessage}
        disabled={disabled}
        maxDisplay={maxDisplay}
        aria-describedby={describedBy}
      />
      <div className="flex items-center justify-between">
        {hint && !hasError && (
          <p
            id={`${field.name}-hint`}
            className="text-xs text-muted-foreground"
          >
            {hint}
          </p>
        )}
        {hasError && (
          <p
            id={`${field.name}-error`}
            role="alert"
            className="text-xs text-destructive"
          >
            {errorMessage}
          </p>
        )}
      </div>
    </div>
  );
}
