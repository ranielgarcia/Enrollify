import { useQuery } from "@tanstack/react-query";
import { getOfferingDetailsForClassSectionOptions } from "@/api/collections/class-section-collection";
import type { ClassSectionMinimal } from "@/api/models/class-scheduling/class-section";
import type { Offering } from "@/api/models/class-scheduling/offering";
import { Loader2, AlertTriangle, ArrowUpRight } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";

const DAYS: Array<{ key: string; label: string }> = [
  { key: "MON", label: "Mon" },
  { key: "TUE", label: "Tue" },
  { key: "WED", label: "Wed" },
  { key: "THU", label: "Thu" },
  { key: "FRI", label: "Fri" },
  { key: "SAT", label: "Sat" },
];

// Time slots from 07:00 to 20:00 in 30-min intervals
const TIME_SLOTS: string[] = [];
for (let h = 7; h < 20; h++) {
  TIME_SLOTS.push(`${String(h).padStart(2, "0")}:00`);
  TIME_SLOTS.push(`${String(h).padStart(2, "0")}:30`);
}

function timeToMinutes(time: string): number {
  const [h, m] = time.split(":").map(Number);
  return (h ?? 0) * 60 + (m ?? 0);
}

const SLOT_HEIGHT_PX = 20; // px per 30-min slot
const GRID_START_MINUTES = 7 * 60; // 07:00

interface OfferingBlock {
  offering: Offering;
  day: string;
  startMinutes: number;
  endMinutes: number;
  hasConflict: boolean;
}

function buildOfferingBlocks(offerings: Offering[]): OfferingBlock[] {
  const blocks: OfferingBlock[] = [];
  for (const offering of offerings) {
    const schedules = offering.schedules ?? [];
    const hasConflict =
      (offering.conflicts?.filter((c) => c.type?.severity === "Error")
        .length ?? 0) > 0;

    for (const schedule of schedules) {
      const day = schedule.dayOfWeekAbbreviation;
      const startMinutes = timeToMinutes(
        schedule.startTime.substring(0, 5),
      );
      const endMinutes = timeToMinutes(schedule.endTime.substring(0, 5));
      blocks.push({
        offering,
        day,
        startMinutes,
        endMinutes,
        hasConflict,
      });
    }
  }
  return blocks;
}

interface MiniScheduleGridProps {
  offerings: Offering[];
  sectionName: string;
  onViewDetails?: () => void;
}

function MiniScheduleGrid({
  offerings,
  sectionName,
  onViewDetails,
}: MiniScheduleGridProps) {
  const blocks = buildOfferingBlocks(offerings);
  const daysWithData = new Set(blocks.map((b) => b.day));
  const visibleDays = DAYS.filter(
    (d) =>
      daysWithData.has(d.key) ||
      ["MON", "TUE", "WED", "THU", "FRI"].includes(d.key),
  );
  const totalMinutes = TIME_SLOTS.length * 30;
  const gridHeight = (totalMinutes / 30) * SLOT_HEIGHT_PX;

  return (
    <div className="w-full">
      {/* Header */}
      <div className="flex items-center justify-between mb-2 pb-2 border-b">
        <div>
          <div className="font-semibold text-sm">{sectionName}</div>
          <div className="text-xs text-muted-foreground">Weekly Schedule</div>
        </div>
        {onViewDetails && (
          <Button
            variant="ghost"
            size="sm"
            onClick={onViewDetails}
            className="h-7 gap-1 text-xs"
          >
            <ArrowUpRight className="size-3" />
            Full View
          </Button>
        )}
      </div>

      {/* Day headers */}
      <div className="flex gap-px mb-1">
        <div className="w-10 shrink-0" />
        {visibleDays.map((d) => (
          <div
            key={d.key}
            className="flex-1 text-center text-xs font-medium text-muted-foreground py-1"
          >
            {d.label}
          </div>
        ))}
      </div>

      {/* Grid body */}
      <div className="flex gap-px relative">
        {/* Time labels */}
        <div
          className="w-10 shrink-0 relative"
          style={{ height: gridHeight }}
        >
          {TIME_SLOTS.filter((_, i) => i % 2 === 0).map((time, i) => (
            <div
              key={time}
              className="absolute right-1 text-[10px] text-muted-foreground leading-none"
              style={{
                top: i * SLOT_HEIGHT_PX * 2 - 4,
              }}
            >
              {time}
            </div>
          ))}
        </div>

        {/* Day columns */}
        {visibleDays.map((d) => {
          const dayBlocks = blocks.filter((b) => b.day === d.key);
          return (
            <div
              key={d.key}
              className="flex-1 relative bg-muted/20 rounded-sm"
              style={{ height: gridHeight }}
            >
              {/* Slot lines */}
              {TIME_SLOTS.map((time, i) => (
                <div
                  key={time}
                  className={cn(
                    "absolute inset-x-0 border-t",
                    i % 2 === 0
                      ? "border-border/60"
                      : "border-border/20",
                  )}
                  style={{ top: i * SLOT_HEIGHT_PX }}
                />
              ))}

              {/* Lunch break highlight */}
              <div
                className="absolute inset-x-0 bg-muted/40"
                style={{
                  top: ((12 * 60 - GRID_START_MINUTES) / 30) * SLOT_HEIGHT_PX,
                  height: (60 / 30) * SLOT_HEIGHT_PX,
                }}
              />

              {/* Offering blocks */}
              {dayBlocks.map((block, idx) => {
                const top =
                  ((block.startMinutes - GRID_START_MINUTES) / 30) *
                  SLOT_HEIGHT_PX;
                const height =
                  ((block.endMinutes - block.startMinutes) / 30) *
                  SLOT_HEIGHT_PX;
                const subjectCode =
                  block.offering.snapshotSubjectCode ?? "—";
                const teacherName = block.offering.teacher
                  ? block.offering.teacher.lastName
                  : "—";
                const roomNum =
                  block.offering.room?.roomNumber ?? "—";

                return (
                  <div
                    key={`${block.offering.id}-${block.day}-${idx}`}
                    className={cn(
                      "absolute inset-x-0.5 rounded-sm px-1 py-0.5 overflow-hidden",
                      "flex flex-col justify-start text-[9px] leading-tight font-medium",
                      block.hasConflict
                        ? "bg-rose-100 text-rose-900 border border-rose-400 dark:bg-rose-950/50 dark:text-rose-300 dark:border-rose-700"
                        : "bg-blue-100 text-blue-900 border border-blue-300 dark:bg-blue-950/50 dark:text-blue-300 dark:border-blue-700",
                    )}
                    style={{ top, height: Math.max(height, SLOT_HEIGHT_PX) }}
                    title={`${subjectCode} — ${teacherName} (Room ${roomNum})`}
                  >
                    <div className="truncate font-semibold">{subjectCode}</div>
                    {height >= SLOT_HEIGHT_PX * 2 && (
                      <div className="truncate opacity-80">{teacherName}</div>
                    )}
                    {height >= SLOT_HEIGHT_PX * 3 && (
                      <div className="truncate opacity-70">Rm {roomNum}</div>
                    )}
                    {block.hasConflict && (
                      <AlertTriangle className="size-2 text-rose-500 absolute top-0.5 right-0.5" />
                    )}
                  </div>
                );
              })}
            </div>
          );
        })}
      </div>

      {/* Conflict legend */}
      {blocks.some((b) => b.hasConflict) && (
        <div className="mt-2 flex items-center gap-3 text-xs text-muted-foreground pt-2 border-t">
          <div className="flex items-center gap-1">
            <div className="size-2.5 rounded-sm bg-blue-200 border border-blue-400" />
            Offering
          </div>
          <div className="flex items-center gap-1">
            <div className="size-2.5 rounded-sm bg-rose-200 border border-rose-400" />
            Conflict
          </div>
        </div>
      )}
    </div>
  );
}

interface SectionCardMiniScheduleProps {
  section: ClassSectionMinimal;
  isHovered: boolean;
  onViewDetails?: () => void;
}

export function SectionCardMiniSchedule({
  section,
  isHovered,
  onViewDetails,
}: SectionCardMiniScheduleProps) {
  const { data: offerings, isLoading } = useQuery({
    ...getOfferingDetailsForClassSectionOptions(section.id),
    enabled: isHovered && !!section.id,
  });

  const validationSummary = section.validationSummary;

  // Phase 1: Static summary (always available)
  const staticSummary = validationSummary
    ? [
        validationSummary.totalOfferings > 0
          ? `${validationSummary.totalOfferings} offering${validationSummary.totalOfferings !== 1 ? "s" : ""}`
          : "No offerings",
        validationSummary.offeringsWithConflicts > 0
          ? `${validationSummary.offeringsWithConflicts} conflict${validationSummary.offeringsWithConflicts !== 1 ? "s" : ""}`
          : null,
        validationSummary.offeringsWithErrors > 0
          ? `${validationSummary.offeringsWithErrors} error${validationSummary.offeringsWithErrors !== 1 ? "s" : ""}`
          : null,
      ]
        .filter(Boolean)
        .join(" · ")
    : "No schedule data";

  if (isLoading) {
    return (
      <div className="space-y-2 w-64">
        <div className="text-xs text-muted-foreground">{staticSummary}</div>
        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          <Loader2 className="size-3 animate-spin" />
          Loading weekly schedule…
        </div>
      </div>
    );
  }

  if (!offerings || offerings.length === 0) {
    return (
      <div className="w-64 space-y-1">
        <div className="text-sm font-semibold">{section.name}</div>
        <div className="text-xs text-muted-foreground">
          No offerings scheduled yet.
        </div>
        {staticSummary && (
          <div className="text-xs text-muted-foreground">{staticSummary}</div>
        )}
      </div>
    );
  }

  return (
    <div style={{ width: 340 }}>
      <MiniScheduleGrid
        offerings={offerings}
        sectionName={section.name}
        onViewDetails={onViewDetails}
      />
    </div>
  );
}
