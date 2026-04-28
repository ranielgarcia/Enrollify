import type { AnyFieldApi } from "@tanstack/react-form";
import { MaskInput, type MaskPattern } from "@/components/ui/mask-input";
import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";

const philippinesPhonePattern: MaskPattern = {
  pattern: "####-###-####",
  transform: (value) => value.replace(/\D/g, ""),
  validate: (value) => /^09\d{9}$/.test(value.replace(/\D/g, "")),
};

interface FormPhoneFieldProps {
  field: AnyFieldApi;
  label: string;
  placeholder?: string;
  className?: string;
  disabled?: boolean;
  required?: boolean;
  hint?: string;
  autoFocus?: boolean;
}

export function FormPhoneField({
  field,
  label,
  placeholder,
  className,
  disabled,
  required,
  hint,
  autoFocus,
}: FormPhoneFieldProps) {
  const handleValueChange = (_maskedValue: string, unmaskedValue: string) => {
    field.handleChange(unmaskedValue);
  };

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
      <MaskInput
        id={field.name}
        mask={philippinesPhonePattern}
        placeholder={placeholder ?? "Enter phone number"}
        maskPlaceholder="09__-___-____"
        value={String(field.state.value ?? "")}
        onValueChange={handleValueChange}
        onBlur={field.handleBlur}
        disabled={disabled}
        autoFocus={autoFocus}
        inputMode="numeric"
        aria-describedby={describedBy}
        aria-invalid={!!hasError}
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
