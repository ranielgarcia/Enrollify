import { cn } from "@/lib/utils";

interface FormSectionProps {
  title?: string;
  description?: string;
  children: React.ReactNode;
  className?: string;
}

export function FormSection({
  title,
  description,
  children,
  className,
}: FormSectionProps) {
  return (
    <fieldset className={cn("space-y-4", className)}>
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
