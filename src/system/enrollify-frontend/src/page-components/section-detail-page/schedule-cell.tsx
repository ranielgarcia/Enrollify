import type { OfferingWithSchedules } from "@/api/models/offering";
import type { DayOfWeek } from "@/api/models/class-schedule";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";

interface ScheduleCellProps {
  day: DayOfWeek;
  timeSlot: string;
  offerings: OfferingWithSchedules[];
}

function timeToMinutes(time: string): number {
  const [h, m] = time.split(":").map(Number);
  return (h ?? 0) * 60 + (m ?? 0);
}

export function ScheduleCell({ day, timeSlot, offerings }: ScheduleCellProps) {
  const slotMinutes = timeToMinutes(timeSlot);

  console.log(offerings);

  const matching = offerings.filter((o) =>
    o.schedules?.some((s) => {
      if (s.dayOfWeekAbbreviation !== day) return false;
      const start = timeToMinutes(s.startTime);
      const end = timeToMinutes(s.endTime);
      return slotMinutes >= start && slotMinutes < end;
    }),
  );

  if (matching.length === 0) {
    return <td className="border border-border/40 h-8 min-w-[80px]" />;
  }

  const isStartSlot = (o: OfferingWithSchedules) =>
    o.schedules?.some(
      (s) =>
        s.dayOfWeekAbbreviation === day &&
        timeToMinutes(s.startTime) === slotMinutes,
    );

  const hasConflict = matching.some(
    (o) => o.conflicts && o.conflicts.length > 0,
  );

  return (
    <td
      className={cn(
        "border border-border/40 h-8 min-w-[80px] p-0.5",
        hasConflict ? "bg-destructive/10" : "bg-primary/10",
      )}
    >
      {matching.filter(isStartSlot).map((o) => (
        <Tooltip key={o.id}>
          <TooltipTrigger asChild>
            <div
              className={cn(
                "rounded text-[10px] leading-tight px-1 py-0.5 cursor-default truncate",
                hasConflict
                  ? "bg-destructive/20 border border-destructive/40 text-destructive"
                  : "bg-primary/20 border border-primary/30 text-primary-foreground dark:text-primary",
              )}
            >
              <span className="font-semibold">{o.snapshotSubjectCode}</span>
              <span className="text-muted-foreground ml-1">
                {o.teacher?.lastName}
              </span>
            </div>
          </TooltipTrigger>
          <TooltipContent side="top" className="text-xs max-w-[200px]">
            <p className="font-semibold">{o.snapshotSubjectTitle}</p>
            <p>
              {o.teacher?.firstName} {o.teacher?.lastName}
            </p>
            <p>{o.room?.roomNumber}</p>
            {hasConflict && (
              <p className="text-destructive mt-1">⚠ Conflict detected</p>
            )}
          </TooltipContent>
        </Tooltip>
      ))}
    </td>
  );
}
