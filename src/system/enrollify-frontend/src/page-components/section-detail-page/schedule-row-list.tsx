import { deleteScheduleRowOptions } from "@/api/collections/offering-collection";
import type { ClassSchedule } from "@/api/models/class-schedule";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { useMutation } from "@tanstack/react-query";
import { Trash2, Clock } from "lucide-react";
import { ScheduleRowFormDrawer } from "./schedule-row-form-drawer";

interface ScheduleRowListProps {
  offeringId: number;
  sectionId: number;
  schedules: ClassSchedule[];
  hoursPerDay?: number;
  daysPerWeek?: number;
}

const DAY_LABELS: Record<string, string> = {
  MON: "Mon",
  TUE: "Tue",
  WED: "Wed",
  THU: "Thu",
  FRI: "Fri",
  SAT: "Sat",
  SUN: "Sun",
};

function formatTime(time: string): string {
  return time.slice(0, 5);
}

interface ScheduleRowItemProps {
  offeringId: number;
  sectionId: number;
  schedule: ClassSchedule;
}

function ScheduleRowItem({
  offeringId,
  sectionId,
  schedule,
}: ScheduleRowItemProps) {
  const { mutateAsync: deleteRow, isPending } = useMutation(
    deleteScheduleRowOptions(offeringId, schedule.id, sectionId),
  );

  return (
    <div className="flex items-center gap-2 rounded-md border px-3 py-2 bg-muted/30">
      <Clock className="size-3.5 text-muted-foreground shrink-0" />
      <Badge variant="outline" className="text-xs font-mono">
        {DAY_LABELS[schedule.dayOfWeekAbbreviation] ??
          schedule.dayOfWeekAbbreviation}
      </Badge>
      <span className="text-sm font-mono">
        {formatTime(schedule.startTime)} – {formatTime(schedule.endTime)}
      </span>
      <Button
        variant="ghost"
        size="sm"
        className="ml-auto h-6 w-6 p-0 hover:bg-destructive/10 hover:text-destructive"
        onClick={() => deleteRow({})}
        disabled={isPending}
        title="Remove schedule row"
      >
        <Trash2 className="size-3" />
      </Button>
    </div>
  );
}

export function ScheduleRowList({
  offeringId,
  sectionId,
  schedules,
  hoursPerDay,
  daysPerWeek,
}: ScheduleRowListProps) {
  return (
    <div className="space-y-2">
      {schedules.length === 0 ? (
        <p className="text-xs text-muted-foreground italic py-2">
          No schedule rows yet. Add one below.
        </p>
      ) : (
        <div className="space-y-1.5">
          {schedules.map((s) => (
            <ScheduleRowItem
              key={s.id}
              offeringId={offeringId}
              sectionId={sectionId}
              schedule={s}
            />
          ))}
        </div>
      )}

      <ScheduleRowFormDrawer
        offeringId={offeringId}
        sectionId={sectionId}
        existingSchedules={schedules}
        hoursPerDay={hoursPerDay}
        daysPerWeek={daysPerWeek}
      />
    </div>
  );
}
