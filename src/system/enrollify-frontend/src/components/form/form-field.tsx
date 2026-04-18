import type { AnyFieldApi } from "@tanstack/react-form";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { cn } from "@/lib/utils";

interface FormFieldProps {
  field: AnyFieldApi;
  label: string;
  type?: "text" | "number" | "textarea" | "email" | "tel";
  placeholder?: string;
  className?: string;
  disabled?: boolean;
  required?: boolean;
  hint?: string;
  maxLength?: number;
  autoFocus?: boolean;
}

export function FormField({
  field,
  label,
  type = "text",
  placeholder,
  className,
  disabled,
  required,
  hint,
  maxLength,
  autoFocus,
}: FormFieldProps) {
  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>,
  ) => {
    const value = type === "number" ? Number(e.target.value) : e.target.value;
    field.handleChange(value);
  };

  const errorMessage = field.state.meta.errors
    .map((e: { message?: string } | string) =>
      typeof e === "string" ? e : e?.message,
    )
    .join(", ");
  const hasError = !field.state.meta.isValid && errorMessage;
  const currentLength =
    type === "textarea" && maxLength
      ? String(field.state.value ?? "").length
      : 0;

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
      {type === "textarea" ? (
        <Textarea
          id={field.name}
          placeholder={placeholder ?? label}
          value={field.state.value ?? ""}
          onChange={handleChange}
          onBlur={field.handleBlur}
          disabled={disabled}
          maxLength={maxLength}
          autoFocus={autoFocus}
          aria-describedby={describedBy}
          aria-invalid={!!hasError}
        />
      ) : (
        <Input
          type={type}
          id={field.name}
          placeholder={placeholder ?? label}
          value={field.state.value ?? ""}
          onChange={handleChange}
          onBlur={field.handleBlur}
          disabled={disabled}
          autoFocus={autoFocus}
          aria-describedby={describedBy}
          aria-invalid={!!hasError}
        />
      )}
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
        {maxLength && type === "textarea" && (
          <span className="text-xs text-muted-foreground ml-auto">
            {currentLength}/{maxLength}
          </span>
        )}
      </div>
    </div>
  );
}
