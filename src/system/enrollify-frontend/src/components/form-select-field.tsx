import type { AnyFieldApi } from "@tanstack/react-form";
import {
  SearchableSelect,
  type SearchableSelectOption,
} from "@/components/searchable-select";
import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";

interface FormSelectFieldProps {
  field: AnyFieldApi;
  label: string;
  options: SearchableSelectOption[];
  placeholder?: string;
  searchPlaceholder?: string;
  emptyMessage?: string;
  required?: boolean;
  hint?: string;
  className?: string;
  disabled?: boolean;
  onValueChange?: (value: string) => void;
}

export function FormSelectField({
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
  onValueChange,
}: FormSelectFieldProps) {
  const errorMessage = field.state.meta.errors
    .map((e: { message?: string } | string) =>
      typeof e === "string" ? e : e?.message,
    )
    .join(", ");
  const hasError = !field.state.meta.isValid && errorMessage;

  const describedBy = [
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
      <SearchableSelect
        options={options}
        value={field.state.value?.toString() ?? ""}
        onValueChange={(val) => {
          const parsed = Number(val);
          field.handleChange(Number.isNaN(parsed) ? val : parsed);
          onValueChange?.(val);
        }}
        name={field.name}
        placeholder={placeholder}
        searchPlaceholder={searchPlaceholder}
        emptyMessage={emptyMessage}
        disabled={disabled}
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
