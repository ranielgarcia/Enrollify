import type { AnyFieldApi } from "@tanstack/react-form";
import { MaskInput } from "@/components/ui/mask-input";
import { Label } from "@/components/ui/label";

interface FormTimePickerProps {
  field: AnyFieldApi;
  label: string;
  required?: boolean;
  hint?: string;
  disabled?: boolean;
}

export function FormTimePicker({
  field,
  label,
  required,
  hint,
  disabled,
}: FormTimePickerProps) {
  const errorMessage = field.state.meta.errors
    .map((e: { message?: string } | string) =>
      typeof e === "string" ? e : e?.message,
    )
    .join(", ");
  const hasError = !field.state.meta.isValid && errorMessage;

  return (
    <div className="grid w-full items-center gap-1.5">
      <Label htmlFor={field.name}>
        {label}
        {required && <span className="text-destructive ml-0.5">*</span>}
      </Label>
      <MaskInput
        id={field.name}
        mask="time"
        value={field.state.value ?? ""}
        onValueChange={(maskedValue) => field.handleChange(maskedValue)}
        onBlur={field.handleBlur}
        placeholder="HH:MM"
        disabled={disabled}
        invalid={!!hasError}
        aria-invalid={!!hasError}
      />
      <div className="flex items-center justify-between">
        {hint && !hasError && (
          <p className="text-xs text-muted-foreground">{hint}</p>
        )}
        {hasError && (
          <p role="alert" className="text-xs text-destructive">
            {errorMessage}
          </p>
        )}
      </div>
    </div>
  );
}
