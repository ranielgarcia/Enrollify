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
}

export function FormField({
  field,
  label,
  type = "text",
  placeholder,
  className,
  disabled,
}: FormFieldProps) {
  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>,
  ) => {
    const value = type === "number" ? Number(e.target.value) : e.target.value;
    field.handleChange(value);
  };

  return (
    <div className={cn("grid w-full items-center gap-3", className)}>
      <Label htmlFor={field.name}>{label}</Label>
      {type === "textarea" ? (
        <Textarea
          id={field.name}
          placeholder={placeholder ?? label}
          value={field.state.value ?? ""}
          onChange={handleChange}
          disabled={disabled}
        />
      ) : (
        <Input
          type={type}
          id={field.name}
          placeholder={placeholder ?? label}
          value={field.state.value ?? ""}
          onChange={handleChange}
          disabled={disabled}
        />
      )}
      {!field.state.meta.isValid && (
        <em role="alert" className="text-destructive text-sm">
          {field.state.meta.errors
            .map((e: { message?: string } | string) =>
              typeof e === "string" ? e : e?.message,
            )
            .join(", ")}
        </em>
      )}
    </div>
  );
}
