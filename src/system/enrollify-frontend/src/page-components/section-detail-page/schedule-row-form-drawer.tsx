import { useState } from "react";
import { createScheduleRowOptions } from "@/api/collections/offering-collection";
import type { ClassSchedule, DayOfWeek } from "@/api/models/class-schedule";
import type { components } from "@/api/generated/api";
import { FormTimeSelect } from "@/components/form/form-time-select";

type AddScheduleRequest =
  components["schemas"]["EnrollifyWebAPIFeaturesSubjectOfferingsAddScheduleToOfferingRequest"];
import { FormSection } from "@/components/form/form-section";
import { Button } from "@/components/ui/button";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
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

const timeSchema = z
  .string()
  .regex(/^\d{2}:\d{2}:\d{2}$/, "Select a time");

const ALL_DAYS: { key: DayOfWeek; label: string }[] = [
  { key: "MON", label: "Mon" },
  { key: "TUE", label: "Tue" },
  { key: "WED", label: "Wed" },
  { key: "THU", label: "Thu" },
  { key: "FRI", label: "Fri" },
  { key: "SAT", label: "Sat" },
  { key: "SUN", label: "Sun" },
];

interface PerDayTime {
  startTime: string;
  endTime: string;
}

interface BulkScheduleFormValues {
  selectedDays: DayOfWeek[];
  sameTime: boolean;
  sharedStartTime: string;
  sharedEndTime: string;
  perDayTimes: Record<DayOfWeek, PerDayTime>;
}

const defaultPerDayTimes = (): Record<DayOfWeek, PerDayTime> =>
  Object.fromEntries(
    ALL_DAYS.map((d) => [d.key, { startTime: "", endTime: "" }]),
  ) as Record<DayOfWeek, PerDayTime>;

interface ScheduleRowFormDrawerProps {
  offeringId: number;
  sectionId: number;
  existingSchedules: ClassSchedule[];
  onSuccess?: () => void;
}

export function ScheduleRowFormDrawer({
  offeringId,
  sectionId,
  existingSchedules,
  onSuccess,
}: ScheduleRowFormDrawerProps) {
  const [isOpen, setIsOpen] = useState(false);

  const { mutateAsync: createScheduleRow } = useMutation(
    createScheduleRowOptions(offeringId, sectionId),
  );

  const assignedDays = new Set(
    existingSchedules.map((s) => s.dayOfWeekAbbreviation),
  );

  const form = useForm<BulkScheduleFormValues>({
    defaultValues: {
      selectedDays: [],
      sameTime: true,
      sharedStartTime: "",
      sharedEndTime: "",
      perDayTimes: defaultPerDayTimes(),
    },
    onSubmit: async ({ value }) => {
      const rows = value.selectedDays.map((day) => {
        const startTime = value.sameTime
          ? value.sharedStartTime
          : value.perDayTimes[day].startTime;
        const endTime = value.sameTime
          ? value.sharedEndTime
          : value.perDayTimes[day].endTime;
        return { dayOfWeek: day, startTime, endTime };
      });

      await Promise.all(
        rows.map((row) => createScheduleRow(row as AddScheduleRequest)),
      );

      setIsOpen(false);
      form.reset();
      onSuccess?.();
    },
  });

  const handleClose = () => {
    setIsOpen(false);
    form.reset();
  };

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
        <DrawerContent className="data-[vaul-drawer-direction=right]:w-[520px] data-[vaul-drawer-direction=right]:sm:max-w-none h-full w-full overflow-y-auto">
          <DrawerHeader className="border-b pb-4">
            <DrawerTitle>Add Schedule Rows</DrawerTitle>
          </DrawerHeader>

          <form
            className="flex flex-col gap-6 p-4"
            onSubmit={(e) => e.preventDefault()}
          >
            {/* Day selection */}
            <FormSection title="Days of Week">
              <form.Field name="selectedDays">
                {(field) => {
                  const errorMessage = field.state.meta.errors
                    .map((e) =>
                      typeof e === "string" ? e : (e as { message: string })?.message,
                    )
                    .join(", ");
                  return (
                    <div className="space-y-2">
                      <div className="flex flex-wrap gap-2">
                        {ALL_DAYS.map((d) => {
                          const isAssigned = assignedDays.has(d.key);
                          const isSelected = field.state.value.includes(d.key);
                          return (
                            <button
                              key={d.key}
                              type="button"
                              disabled={isAssigned}
                              onClick={() => {
                                const current = field.state.value;
                                field.handleChange(
                                  isSelected
                                    ? current.filter((x) => x !== d.key)
                                    : [...current, d.key],
                                );
                              }}
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
                        <p className="text-xs text-destructive">
                          {errorMessage}
                        </p>
                      )}
                    </div>
                  );
                }}
              </form.Field>
            </FormSection>

            {/* Same time toggle */}
            <form.Field name="sameTime">
              {(field) => (
                <div className="flex items-center gap-3">
                  <Switch
                    id="sameTime"
                    checked={field.state.value}
                    onCheckedChange={(checked) =>
                      field.handleChange(checked)
                    }
                  />
                  <Label htmlFor="sameTime" className="text-sm cursor-pointer">
                    Same time for all selected days
                  </Label>
                </div>
              )}
            </form.Field>

            {/* Shared time inputs */}
            <form.Subscribe selector={(s) => s.values.sameTime}>
              {(sameTime) =>
                sameTime ? (
                  <FormSection title="Time (All Days)">
                    <form.Field
                      name="sharedStartTime"
                      validators={{
                        onBlur: timeSchema,
                        onSubmit: timeSchema,
                      }}
                    >
                      {(field) => (
                        <FormTimeSelect
                          field={field}
                          label="Start Time"
                          required
                        />
                      )}
                    </form.Field>
                    <form.Field
                      name="sharedEndTime"
                      validators={{
                        onBlur: timeSchema,
                        onSubmit: timeSchema,
                      }}
                    >
                      {(field) => (
                        <FormTimeSelect
                          field={field}
                          label="End Time"
                          required
                        />
                      )}
                    </form.Field>
                  </FormSection>
                ) : null
              }
            </form.Subscribe>

            {/* Per-day time inputs */}
            <form.Subscribe
              selector={(s) => ({
                sameTime: s.values.sameTime,
                selectedDays: s.values.selectedDays,
              })}
            >
              {({ sameTime, selectedDays }) =>
                !sameTime && selectedDays.length > 0 ? (
                  <div className="space-y-4">
                    {selectedDays.map((day) => {
                      const dayLabel =
                        ALL_DAYS.find((d) => d.key === day)?.label ?? day;
                      return (
                        <FormSection key={day} title={`${dayLabel} Time`}>
                          <form.Field
                            name={`perDayTimes.${day}.startTime`}
                            validators={{
                              onBlur: timeSchema,
                              onSubmit: timeSchema,
                            }}
                          >
                            {(field) => (
                              <FormTimeSelect
                                field={field}
                                label="Start Time"
                                required
                              />
                            )}
                          </form.Field>
                          <form.Field
                            name={`perDayTimes.${day}.endTime`}
                            validators={{
                              onBlur: timeSchema,
                              onSubmit: timeSchema,
                            }}
                          >
                            {(field) => (
                              <FormTimeSelect
                                field={field}
                                label="End Time"
                                required
                              />
                            )}
                          </form.Field>
                        </FormSection>
                      );
                    })}
                  </div>
                ) : null
              }
            </form.Subscribe>
          </form>

          <div className="border-t p-4 flex flex-col gap-2">
            <form.Subscribe
              selector={(s) =>
                [s.isSubmitting, s.values.selectedDays.length] as const
              }
            >
              {([isSubmitting, dayCount]) => (
                <Button
                  disabled={dayCount === 0 || isSubmitting}
                  onClick={() => form.handleSubmit()}
                >
                  {isSubmitting
                    ? "Adding..."
                    : `Add ${dayCount > 0 ? `${dayCount} ` : ""}Schedule Row${dayCount !== 1 ? "s" : ""}`}
                </Button>
              )}
            </form.Subscribe>
            <Button variant="outline" onClick={handleClose}>
              Cancel
            </Button>
          </div>
        </DrawerContent>
      </Drawer>
    </>
  );
}
