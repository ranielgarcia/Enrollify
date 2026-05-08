import { deleteScheduleRowOptions } from "@/api/collections/offering-collection";
import type { ClassSchedule } from "@/api/models/class-schedule";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { useMutation } from "@tanstack/react-query";
import { Trash2, Clock } from "lucide-react";
import { ScheduleRowFormDrawer } from "./schedule-row-form-drawer";

interface ScheduleRowListProps {
  offeringId: number;
  schedules: ClassSchedule[];
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

export function ScheduleRowList({
  offeringId,
  schedules,
}: ScheduleRowListProps) {
  const { mutateAsync: deleteRow } = useMutation(
    deleteScheduleRowOptions(offeringId, 0),
  );

  const handleDelete = async (scheduleId: number) => {
    await deleteRow({ scheduleId } as any);
  };

  return (
    <div className="space-y-2">
      {schedules.length === 0 ? (
        <p className="text-xs text-muted-foreground italic py-2">
          No schedule rows yet. Add one below.
        </p>
      ) : (
        <div className="space-y-1.5">
          {schedules.map((s) => (
            <div
              key={s.id}
              className="flex items-center gap-2 rounded-md border px-3 py-2 bg-muted/30"
            >
              <Clock className="size-3.5 text-muted-foreground shrink-0" />
              <Badge variant="outline" className="text-xs font-mono">
                {DAY_LABELS[s.dayOfWeek] ?? s.dayOfWeek}
              </Badge>
              <span className="text-sm font-mono">
                {s.startTime} – {s.endTime}
              </span>
              <Button
                variant="ghost"
                size="sm"
                className="ml-auto h-6 w-6 p-0 hover:bg-destructive/10 hover:text-destructive"
                onClick={() => handleDelete(s.id)}
                title="Remove schedule row"
              >
                <Trash2 className="size-3" />
              </Button>
            </div>
          ))}
        </div>
      )}

      <ScheduleRowFormDrawer
        offeringId={offeringId}
        existingSchedules={schedules}
      />
    </div>
  );
}
