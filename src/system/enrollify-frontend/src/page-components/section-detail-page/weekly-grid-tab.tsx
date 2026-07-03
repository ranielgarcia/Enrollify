import type { Offering } from "@/api/models/class-scheduling/offering";
import { WeeklyScheduleGrid } from "./weekly-schedule-grid";
import { CalendarClock } from "lucide-react";

interface WeeklyGridTabProps {
  offerings: Offering[];
}

export function WeeklyGridTab({ offerings }: WeeklyGridTabProps) {
  const hasSchedules = offerings.some(
    (o) => o.schedules && o.schedules.length > 0,
  );

  if (!hasSchedules) {
    return (
      <div className="flex flex-col items-center justify-center py-16 gap-4 text-center">
        <div className="rounded-full bg-muted p-4">
          <CalendarClock className="size-8 text-muted-foreground" />
        </div>
        <div>
          <p className="text-base font-semibold">No schedules yet</p>
          <p className="text-sm text-muted-foreground mt-1">
            Assign offerings and add schedule rows to see the weekly grid.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <p className="text-sm text-muted-foreground">
        Showing all scheduled offerings for this section. Highlighted cells
        indicate active time slots.
      </p>
      <WeeklyScheduleGrid offerings={offerings} />
    </div>
  );
}
