import { useState } from "react";
import { createScheduleRowOptions } from "@/api/collections/offering-collection";
import type { ClassSchedule, DayOfWeek } from "@/api/models/class-schedule";
import { FormTimePicker } from "@/components/form/form-time-picker";
import { FormSection } from "@/components/form/form-section";
import { Button } from "@/components/ui/button";
import {
  Drawer,
  DrawerContent,
  DrawerHeader,
  DrawerTitle,
} from "@/components/ui/drawer";
import { useForm } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import { z } from "zod";

const scheduleRowSchema = z.object({
  dayOfWeek: z.enum(["MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN"]),
  startTime: z.string().regex(/^\d{2}:\d{2}$/, "Enter time as HH:MM"),
  endTime: z.string().regex(/^\d{2}:\d{2}$/, "Enter time as HH:MM"),
});

const ALL_DAYS: { key: DayOfWeek; label: string }[] = [
  { key: "MON", label: "Mon" },
  { key: "TUE", label: "Tue" },
  { key: "WED", label: "Wed" },
  { key: "THU", label: "Thu" },
  { key: "FRI", label: "Fri" },
  { key: "SAT", label: "Sat" },
  { key: "SUN", label: "Sun" },
];

interface ScheduleRowFormDrawerProps {
  offeringId: number;
  existingSchedules: ClassSchedule[];
  onSuccess?: () => void;
}

export function ScheduleRowFormDrawer({
  offeringId,
  existingSchedules,
  onSuccess,
}: ScheduleRowFormDrawerProps) {
  const [isOpen, setIsOpen] = useState(false);

  const { mutateAsync: createScheduleRow } = useMutation(
    createScheduleRowOptions(offeringId),
  );

  const assignedDays = new Set(existingSchedules.map((s) => s.dayOfWeek));

  const form = useForm({
    defaultValues: {
      dayOfWeek: "" as DayOfWeek,
      startTime: "",
      endTime: "",
    },
    validators: {
      onBlur: scheduleRowSchema,
      onSubmit: scheduleRowSchema,
    },
    onSubmit: async ({ value }) => {
      await createScheduleRow(value as any);
      setIsOpen(false);
      form.reset();
      onSuccess?.();
    },
  });

  return (
    <>
      <Button
        variant="outline"
        size="sm"
        className="gap-2"
        onClick={() => setIsOpen(true)}
      >
        <Plus className="size-4" />
        Add Schedule Row
      </Button>

      <Drawer open={isOpen} onOpenChange={setIsOpen} direction="right" dismissible={false}>
        <DrawerContent className="h-full w-full max-w-sm overflow-y-auto">
          <DrawerHeader className="border-b pb-4">
            <DrawerTitle>Add Schedule Row</DrawerTitle>
          </DrawerHeader>

          <form
            className="flex flex-col gap-6 p-4"
            onSubmit={(e) => e.preventDefault()}
          >
            <FormSection title="Day of Week">
              <form.Field name="dayOfWeek">
                {(field) => {
                  const errorMessage = field.state.meta.errors
                    .map((e: any) => (typeof e === "string" ? e : e?.message))
                    .join(", ");
                  return (
                    <div className="space-y-2">
                      <div className="flex flex-wrap gap-2">
                        {ALL_DAYS.map((d) => {
                          const isAssigned = assignedDays.has(d.key);
                          const isSelected = field.state.value === d.key;
                          return (
                            <button
                              key={d.key}
                              type="button"
                              disabled={isAssigned}
                              onClick={() => field.handleChange(d.key)}
                              className={`px-3 py-1.5 rounded-md text-sm font-medium border transition-colors ${
                                isAssigned
                                  ? "opacity-40 cursor-not-allowed bg-muted border-border"
                                  : isSelected
                                    ? "bg-primary text-primary-foreground border-primary"
                                    : "bg-background border-border hover:bg-muted"
                              }`}
                            >
                              {d.label}
                              {isAssigned && (
                                <span className="ml-1 text-[10px]">✓</span>
                              )}
                            </button>
                          );
                        })}
                      </div>
                      {errorMessage && (
                        <p className="text-xs text-destructive">{errorMessage}</p>
                      )}
                    </div>
                  );
                }}
              </form.Field>
            </FormSection>

            <FormSection title="Time">
              <form.Field name="startTime">
                {(field) => (
                  <FormTimePicker
                    field={field}
                    label="Start Time"
                    required
                    hint="e.g. 08:00"
                  />
                )}
              </form.Field>
              <form.Field name="endTime">
                {(field) => (
                  <FormTimePicker
                    field={field}
                    label="End Time"
                    required
                    hint="e.g. 09:30"
                  />
                )}
              </form.Field>
            </FormSection>
          </form>

          <div className="border-t p-4 flex flex-col gap-2">
            <form.Subscribe
              selector={(s) => [s.canSubmit, s.isSubmitting] as const}
            >
              {([canSubmit, isSubmitting]) => (
                <Button
                  disabled={!canSubmit || isSubmitting}
                  onClick={() => form.handleSubmit()}
                >
                  Add Schedule Row
                </Button>
              )}
            </form.Subscribe>
            <Button variant="outline" onClick={() => setIsOpen(false)}>
              Cancel
            </Button>
          </div>
        </DrawerContent>
      </Drawer>
    </>
  );
}
