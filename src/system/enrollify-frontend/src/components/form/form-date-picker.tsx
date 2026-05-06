import * as React from "react";
import type { AnyFieldApi } from "@tanstack/react-form";
import { CalendarIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import { Label } from "@/components/ui/label";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { cn } from "@/lib/utils";

interface FormDatePickerProps {
  field: AnyFieldApi;
  label: string;
  placeholder?: string;
  required?: boolean;
  hint?: string;
  className?: string;
  disabled?: boolean;
  numberOfYearsPastAndFuture?: number;
}

export function FormDatePicker({
  field,
  label,
  placeholder = "Select date",
  required,
  hint,
  className,
  disabled,
  numberOfYearsPastAndFuture = 5,
}: FormDatePickerProps) {
  const [open, setOpen] = React.useState(false);

  const currentYear = new Date().getFullYear();
  const fromYear = currentYear - numberOfYearsPastAndFuture;
  const toYear = currentYear + numberOfYearsPastAndFuture;

  const date: Date | undefined = field.state.value
    ? new Date(field.state.value)
    : undefined;

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
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            id={field.name}
            disabled={disabled}
            aria-describedby={describedBy}
            aria-invalid={!!hasError}
            className={cn(
              "w-full justify-start font-normal",
              !date && "text-muted-foreground",
              hasError && "border-destructive",
            )}
            onBlur={field.handleBlur}
          >
            <CalendarIcon className="mr-2 h-4 w-4" />
            {date ? date.toLocaleDateString() : placeholder}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto overflow-hidden p-0" align="start">
          <Calendar
            mode="single"
            selected={date}
            defaultMonth={date}
            captionLayout="dropdown"
            startMonth={new Date(fromYear, 0)}
            endMonth={new Date(toYear, 11)}
            onSelect={(selected) => {
              field.handleChange(selected ? selected.toISOString() : undefined);
              setOpen(false);
            }}
          />
        </PopoverContent>
      </Popover>
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
