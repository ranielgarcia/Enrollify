import type { OfferingWithSchedules } from "@/api/models/offering";
import { ScrollArea, ScrollBar } from "@/components/ui/scroll-area";
import { ScheduleCell } from "./schedule-cell";
import type { DayOfWeek } from "@/api/models/class-schedule";

interface WeeklyScheduleGridProps {
  offerings: OfferingWithSchedules[];
}

const DAYS: { key: DayOfWeek; label: string }[] = [
  { key: "MON", label: "Monday" },
  { key: "TUE", label: "Tuesday" },
  { key: "WED", label: "Wednesday" },
  { key: "THU", label: "Thursday" },
  { key: "FRI", label: "Friday" },
  { key: "SAT", label: "Saturday" },
];

function generateTimeSlots(): string[] {
  const slots: string[] = [];
  for (let h = 7; h <= 20; h++) {
    slots.push(`${String(h).padStart(2, "0")}:00`);
    if (h < 20) slots.push(`${String(h).padStart(2, "0")}:30`);
  }
  return slots;
}

const TIME_SLOTS = generateTimeSlots();

function formatTimeLabel(slot: string): string {
  const [h, m] = slot.split(":").map(Number);
  const hour = h ?? 0;
  const ampm = hour < 12 ? "AM" : "PM";
  const displayHour = hour === 0 ? 12 : hour > 12 ? hour - 12 : hour;
  return `${displayHour}:${String(m ?? 0).padStart(2, "0")} ${ampm}`;
}

export function WeeklyScheduleGrid({ offerings }: WeeklyScheduleGridProps) {
  const activeDays = DAYS.filter((d) =>
    offerings.some((o) =>
      o.schedules.some((s) => s.dayOfWeekAbbreviation === d.key),
    ),
  );
  const displayDays = activeDays.length > 0 ? activeDays : DAYS.slice(0, 5);

  return (
    <ScrollArea className="rounded-md border">
      <div className="overflow-auto">
        <table className="border-collapse text-xs w-full min-w-max">
          <thead>
            <tr>
              <th className="sticky left-0 z-10 bg-muted border border-border/40 px-3 py-2 text-left font-semibold min-w-[80px]">
                Time
              </th>
              {displayDays.map((d) => (
                <th
                  key={d.key}
                  className="bg-muted border border-border/40 px-3 py-2 text-center font-semibold min-w-[100px]"
                >
                  {d.label}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {TIME_SLOTS.map((slot) => (
              <tr key={slot}>
                <td className="sticky left-0 z-10 bg-background border border-border/40 px-3 py-0.5 text-muted-foreground font-mono whitespace-nowrap">
                  {formatTimeLabel(slot)}
                </td>
                {displayDays.map((d) => (
                  <ScheduleCell
                    key={`${d.key}-${slot}`}
                    day={d.key}
                    timeSlot={slot}
                    offerings={offerings}
                  />
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <ScrollBar orientation="horizontal" />
    </ScrollArea>
  );
}
