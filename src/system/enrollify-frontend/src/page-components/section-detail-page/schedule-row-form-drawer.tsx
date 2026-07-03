import { useState } from "react";
import { createScheduleRowsOptions } from "@/api/collections/offering-collection";
import type {
  ClassSchedule,
  DayOfWeek,
} from "@/api/models/class-scheduling/class-schedule";
import type { components } from "@/api/generated/api";
import { FormTimeSelect } from "@/components/form/form-time-select";
import { toast } from "sonner";

type AddMultipleSchedulesRequest =
  components["schemas"]["EnrollifyWebAPIFeaturesSubjectOfferingsAddMultipleSchedulesToOfferingRequest"];
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

const timeSchema = z.string().regex(/^\d{2}:\d{2}:\d{2}$/, "Select a time");

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

function computeDurationHours(startTime: string, endTime: string): number {
  const [sh, sm, ss] = startTime.split(":").map(Number);
  const [eh, em, es] = endTime.split(":").map(Number);
  return eh + em / 60 + es / 3600 - (sh + sm / 60 + ss / 3600);
}

/**
 * Returns the shared start/end time if ALL existing schedules have the exact
 * same time range, otherwise returns null.
 */
function getSharedExistingTime(
  schedules: ClassSchedule[],
): { startTime: string; endTime: string } | null {
  if (schedules.length === 0) return null;
  const first = schedules[0];
  const allSame = schedules.every(
    (s) => s.startTime === first.startTime && s.endTime === first.endTime,
  );
  return allSame
    ? { startTime: first.startTime, endTime: first.endTime }
    : null;
}

interface ScheduleRowFormDrawerProps {
  offeringId: number;
  sectionId: number;
  existingSchedules: ClassSchedule[];
  hoursPerDay?: number;
  daysPerWeek?: number;
  onSuccess?: () => void;
}

export function ScheduleRowFormDrawer({
  offeringId,
  sectionId,
  existingSchedules,
  hoursPerDay,
  daysPerWeek,
  onSuccess,
}: ScheduleRowFormDrawerProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const { mutateAsync: createScheduleRows } = useMutation({
    ...createScheduleRowsOptions(offeringId, sectionId),
    onError: () => {
      toast.error("Failed to add schedule row(s). Check the offering limits.");
    },
  });

  const assignedDays = new Set(
    existingSchedules.map((s) => s.dayOfWeekAbbreviation),
  );
  const remainingSlots =
    daysPerWeek != null ? daysPerWeek - existingSchedules.length : null;
  const isMaxDaysReached = remainingSlots != null && remainingSlots <= 0;

  // Detect if all existing schedules share the same time range.
  const sharedExistingTime = getSharedExistingTime(existingSchedules);
  const isTimeLockedByExisting = sharedExistingTime != null;

  const form = useForm<BulkScheduleFormValues>({
    defaultValues: {
      selectedDays: [],
      sameTime: true,
      sharedStartTime: sharedExistingTime?.startTime ?? "",
      sharedEndTime: sharedExistingTime?.endTime ?? "",
      perDayTimes: defaultPerDayTimes(),
    },
    onSubmit: async ({ value }) => {
      setFormError(null);

      if (hoursPerDay != null) {
        for (const day of value.selectedDays) {
          const startTime = value.sameTime
            ? value.sharedStartTime
            : value.perDayTimes[day].startTime;
          const endTime = value.sameTime
            ? value.sharedEndTime
            : value.perDayTimes[day].endTime;
          if (!startTime || !endTime) {
            setFormError("All selected days must have start and end times.");
            return;
          }
          const duration = computeDurationHours(startTime, endTime);
          if (Math.abs(duration - hoursPerDay) > 0.01) {
            setFormError(
              `Each schedule must be exactly ${hoursPerDay} hour(s). Duration for ${ALL_DAYS.find((d) => d.key === day)?.label ?? day} is ${duration.toFixed(2)} hour(s).`,
            );
            return;
          }
        }
      }

      const schedules = value.selectedDays.map((day) => {
        const startTime = value.sameTime
          ? value.sharedStartTime
          : value.perDayTimes[day].startTime;
        const endTime = value.sameTime
          ? value.sharedEndTime
          : value.perDayTimes[day].endTime;
        return { dayOfWeek: day, startTime, endTime };
      });

      // Submit all schedules in a single batch request to avoid race conditions
      await createScheduleRows({
        schedules,
      } as AddMultipleSchedulesRequest);

      setIsOpen(false);
      setFormError(null);
      form.reset();
      onSuccess?.();
    },
  });

  const handleOpen = () => {
    setFormError(null);
    setIsOpen(true);
  };

  const handleClose = () => {
    setIsOpen(false);
    setFormError(null);
    form.reset();
  };

  /**
   * Builds the duration validator for a time field.
   * Uses onChangeListenTo so TanStack Form re-runs each field's own validator
   * whenever the sibling changes — no manual validateField calls, no recursion.
   */
  function makeDurationValidator(
    getSiblingValue: () => string,
    role: "start" | "end",
  ) {
    return ({ value }: { value: string }) => {
      // Validate the field's own time format first.
      const parsed = timeSchema.safeParse(value);
      if (!parsed.success) return "Select a time";

      if (!hoursPerDay) return;

      const sibling = getSiblingValue();
      if (!sibling || !timeSchema.safeParse(sibling).success) return;

      const startTime = role === "start" ? value : sibling;
      const endTime = role === "end" ? value : sibling;
      const duration = computeDurationHours(startTime, endTime);

      if (duration <= 0) {
        return role === "start"
          ? "Start time must be before end time"
          : "End time must be after start time";
      }

      if (Math.abs(duration - hoursPerDay) > 0.01) {
        return `Duration must be exactly ${hoursPerDay} hour(s)`;
      }
    };
  }

  return (
    <>
      <Button
        variant="outline"
        size="sm"
        className="gap-2"
        disabled={isMaxDaysReached}
        title={
          isMaxDaysReached
            ? `Maximum of ${daysPerWeek} schedule row(s) reached`
            : "Add Schedule Row"
        }
        onClick={handleOpen}
      >
        <Plus className="size-4" />
        {isMaxDaysReached ? "Max Rows Reached" : "Add Schedule Row"}
      </Button>

      <Drawer
        open={isOpen}
        onOpenChange={setIsOpen}
        direction="right"
        dismissible={false}
      >
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
                      typeof e === "string"
                        ? e
                        : (e as { message: string })?.message,
                    )
                    .join(", ");
                  return (
                    <div className="space-y-2">
                      <div className="flex flex-wrap gap-2">
                        {ALL_DAYS.map((d) => {
                          const isAssigned = assignedDays.has(d.key);
                          const isSelected = field.state.value.includes(d.key);
                          // Cannot select more than remainingSlots new days.
                          const atMaxSlots =
                            remainingSlots != null &&
                            !isSelected &&
                            field.state.value.length >= remainingSlots;
                          return (
                            <button
                              key={d.key}
                              type="button"
                              disabled={isAssigned || atMaxSlots}
                              title={
                                atMaxSlots
                                  ? `Maximum ${remainingSlots} new day(s) allowed (${daysPerWeek} day/week limit)`
                                  : isAssigned
                                    ? "Already scheduled"
                                    : d.label
                              }
                              onClick={() => {
                                const current = field.state.value;
                                if (!isSelected && atMaxSlots) return;
                                field.handleChange(
                                  isSelected
                                    ? current.filter((x) => x !== d.key)
                                    : [...current, d.key],
                                );
                              }}
                              className={`px-3 py-1.5 rounded-md text-sm font-medium border transition-colors ${
                                isAssigned || atMaxSlots
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
                      {daysPerWeek != null && (
                        <p className="text-xs text-muted-foreground">
                          {existingSchedules.length} of {daysPerWeek} day(s)
                          scheduled
                          {field.state.value.length > 0 && (
                            <> · {field.state.value.length} selected</>
                          )}
                        </p>
                      )}
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
                    onCheckedChange={(checked) => field.handleChange(checked)}
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
                  <FormSection
                    title="Time (All Days)"
                    description={
                      isTimeLockedByExisting
                        ? "Time is fixed to match existing schedules."
                        : undefined
                    }
                  >
                    <form.Field
                      name="sharedStartTime"
                      validators={{
                        onChangeListenTo: ["sharedEndTime"],
                        onChange: makeDurationValidator(
                          () => form.getFieldValue("sharedEndTime"),
                          "start",
                        ),
                        onSubmit: timeSchema,
                      }}
                    >
                      {(field) => (
                        <FormTimeSelect
                          field={field}
                          label="Start Time"
                          required
                          disabled={isTimeLockedByExisting}
                        />
                      )}
                    </form.Field>
                    <form.Field
                      name="sharedEndTime"
                      validators={{
                        onChangeListenTo: ["sharedStartTime"],
                        onChange: makeDurationValidator(
                          () => form.getFieldValue("sharedStartTime"),
                          "end",
                        ),
                        onSubmit: timeSchema,
                      }}
                    >
                      {(field) => (
                        <FormTimeSelect
                          field={field}
                          label="End Time"
                          required
                          disabled={isTimeLockedByExisting}
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
                              onChangeListenTo: [`perDayTimes.${day}.endTime`],
                              onChange: makeDurationValidator(
                                () =>
                                  form.getFieldValue(
                                    `perDayTimes.${day}.endTime`,
                                  ),
                                "start",
                              ),
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
                              onChangeListenTo: [
                                `perDayTimes.${day}.startTime`,
                              ],
                              onChange: makeDurationValidator(
                                () =>
                                  form.getFieldValue(
                                    `perDayTimes.${day}.startTime`,
                                  ),
                                "end",
                              ),
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
                <>
                  {formError && (
                    <p className="text-xs text-destructive">{formError}</p>
                  )}
                  <Button
                    disabled={dayCount === 0 || isSubmitting}
                    onClick={() => form.handleSubmit()}
                  >
                    {isSubmitting
                      ? "Adding..."
                      : `Add ${dayCount > 0 ? `${dayCount} ` : ""}Schedule Row${dayCount !== 1 ? "s" : ""}`}
                  </Button>
                </>
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
