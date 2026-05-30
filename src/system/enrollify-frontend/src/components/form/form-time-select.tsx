import type { AnyFieldApi } from "@tanstack/react-form";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

interface FormTimeSelectProps {
  field: AnyFieldApi;
  label: string;
  required?: boolean;
  disabled?: boolean;
}

function generateTimeOptions(): { value: string; label: string }[] {
  const options: { value: string; label: string }[] = [];
  for (let h = 7; h <= 22; h++) {
    for (const m of [0, 30]) {
      if (h === 22 && m === 30) break;
      const hStr = String(h).padStart(2, "0");
      const mStr = String(m).padStart(2, "0");
      const value = `${hStr}:${mStr}:00`;
      const hour12 = h === 0 ? 12 : h > 12 ? h - 12 : h;
      const ampm = h < 12 ? "AM" : "PM";
      const label = `${String(hour12).padStart(2, "0")}:${mStr} ${ampm}`;
      options.push({ value, label });
    }
  }
  return options;
}

const TIME_OPTIONS = generateTimeOptions();

export function FormTimeSelect({
  field,
  label,
  required,
  disabled,
}: FormTimeSelectProps) {
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
      <Select
        value={field.state.value ?? ""}
        onValueChange={(val) => field.handleChange(val)}
        disabled={disabled}
      >
        <SelectTrigger
          id={field.name}
          className={hasError ? "border-destructive" : ""}
        >
          <SelectValue placeholder="Select time..." />
        </SelectTrigger>
        <SelectContent className="max-h-[240px]">
          {TIME_OPTIONS.map((opt) => (
            <SelectItem key={opt.value} value={opt.value}>
              {opt.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      {hasError && (
        <p role="alert" className="text-xs text-destructive">
          {errorMessage}
        </p>
      )}
    </div>
  );
}
