import type { CSSProperties } from "react";
import { useQuery } from "@tanstack/react-query";
import { AlertTriangle, ArrowUpRight, Loader2 } from "lucide-react";

import { getOfferingDetailsForClassSectionOptions } from "@/api/collections/class-section-collection";
import type { ClassSectionMinimal } from "@/api/models/class-scheduling/class-section";
import type { Offering } from "@/api/models/class-scheduling/offering";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

const DAYS: ReadonlyArray<{ key: string; label: string }> = [
  { key: "MON", label: "Mon" },
  { key: "TUE", label: "Tue" },
  { key: "WED", label: "Wed" },
  { key: "THU", label: "Thu" },
  { key: "FRI", label: "Fri" },
  { key: "SAT", label: "Sat" },
];

const SLOT_HEIGHT_PX = 24; // pixels per 30-minute slot
const DEFAULT_START_HOUR = 7;
const DEFAULT_END_HOUR = 19;
const ABS_MIN_HOUR = 6;
const ABS_MAX_HOUR = 21;
const GRID_WIDTH_PX = 360;
const TIME_AXIS_PX = 44;

function timeToMinutes(time: string): number {
  const [h, m] = time.split(":").map(Number);
  return (h ?? 0) * 60 + (m ?? 0);
}

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
    const hasConflict =
      (offering.conflicts?.filter((c) => c.type?.severity === "Error").length ??
        0) > 0;
    for (const schedule of offering.schedules ?? []) {
      blocks.push({
        offering,
        day: schedule.dayOfWeekAbbreviation,
        startMinutes: timeToMinutes(schedule.startTime.substring(0, 5)),
        endMinutes: timeToMinutes(schedule.endTime.substring(0, 5)),
        hasConflict,
      });
    }
  }
  return blocks;
}

/**
 * Decide the visible time range. Defaults to 07:00–19:00 but expands outward
 * (capped to ABS_MIN/MAX) if any offering falls outside the default range.
 */
function computeVisibleRange(blocks: OfferingBlock[]): {
  startHour: number;
  endHour: number;
} {
  if (blocks.length === 0) {
    return { startHour: DEFAULT_START_HOUR, endHour: DEFAULT_END_HOUR };
  }
  let minStart = DEFAULT_START_HOUR * 60;
  let maxEnd = DEFAULT_END_HOUR * 60;
  for (const b of blocks) {
    if (b.startMinutes < minStart) minStart = b.startMinutes;
    if (b.endMinutes > maxEnd) maxEnd = b.endMinutes;
  }
  const startHour = Math.max(ABS_MIN_HOUR, Math.floor(minStart / 60));
  const endHour = Math.min(ABS_MAX_HOUR, Math.ceil(maxEnd / 60));
  return { startHour, endHour };
}

function formatHour(h: number): string {
  return `${String(h).padStart(2, "0")}:00`;
}

function formatHoursPerWeek(blocks: OfferingBlock[]): number {
  let totalMin = 0;
  for (const b of blocks) totalMin += b.endMinutes - b.startMinutes;
  return Math.round((totalMin / 60) * 10) / 10;
}

interface MiniScheduleGridProps {
  blocks: OfferingBlock[];
  startHour: number;
  endHour: number;
}

function MiniScheduleGrid({
  blocks,
  startHour,
  endHour,
}: MiniScheduleGridProps) {
  const gridStartMinutes = startHour * 60;
  const slotsPerHour = 2;
  const totalSlots = (endHour - startHour) * slotsPerHour;
  const gridHeight = totalSlots * SLOT_HEIGHT_PX;
  const dayColumnWidth = (GRID_WIDTH_PX - TIME_AXIS_PX) / DAYS.length;

  const hourLabels: number[] = [];
  for (let h = startHour; h <= endHour; h++) hourLabels.push(h);

  return (
    <div
      style={
        {
          "--grid-w": `${GRID_WIDTH_PX}px`,
          "--grid-h": `${gridHeight}px`,
          "--axis-w": `${TIME_AXIS_PX}px`,
          "--col-w": `${dayColumnWidth}px`,
          "--slot-h": `${SLOT_HEIGHT_PX}px`,
        } as CSSProperties
      }
      className="w-(--grid-w)"
    >
      {/* Day headers */}
      <div className="mb-1 flex">
        <div className="w-(--axis-w) shrink-0" />
        <div className="flex flex-1">
          {DAYS.map((d) => (
            <div
              key={d.key}
              className="flex-1 py-1 text-center text-[11px] font-semibold text-muted-foreground"
            >
              {d.label}
            </div>
          ))}
        </div>
      </div>

      {/* Body */}
      <div className="relative flex">
        {/* Time axis */}
        <div className="relative w-(--axis-w) shrink-0 h-(--grid-h)">
          {hourLabels.map((h) => {
            const topPx = (h - startHour) * slotsPerHour * SLOT_HEIGHT_PX;
            return (
              <div
                key={h}
                style={{ top: `${topPx}px` }}
                className="absolute right-1 -translate-y-1/2 text-[10px] leading-none text-muted-foreground"
              >
                {formatHour(h)}
              </div>
            );
          })}
        </div>

        {/* Day columns */}
        {DAYS.map((d) => {
          const dayBlocks = blocks.filter((b) => b.day === d.key);
          return (
            <div
              key={d.key}
              className="relative h-(--grid-h) w-(--col-w) shrink-0 border-l border-border/40 first:border-l-0"
            >
              {/* Hour gridlines (strong) and half-hour gridlines (faint) */}
              {Array.from({ length: totalSlots + 1 }, (_, i) => i).map((i) => {
                const isHour = i % 2 === 0;
                const topPx = i * SLOT_HEIGHT_PX;
                return (
                  <div
                    key={i}
                    style={{ top: `${topPx}px` }}
                    className={cn(
                      "absolute inset-x-0 border-t",
                      isHour ? "border-border/60" : "border-border/20",
                    )}
                  />
                );
              })}

              {/* Offering blocks */}
              {dayBlocks.map((block, idx) => {
                const topPx =
                  ((block.startMinutes - gridStartMinutes) / 30) *
                  SLOT_HEIGHT_PX;
                const heightPx = Math.max(
                  ((block.endMinutes - block.startMinutes) / 30) *
                    SLOT_HEIGHT_PX,
                  SLOT_HEIGHT_PX,
                );
                const subjectCode = block.offering.snapshotSubjectCode ?? "—";
                const teacherName = block.offering.teacher?.lastName ?? "—";
                const roomNum = block.offering.room?.roomNumber ?? "—";

                return (
                  <div
                    key={`${block.offering.id}-${block.day}-${idx}`}
                    style={{ top: `${topPx}px`, height: `${heightPx}px` }}
                    title={`${subjectCode} · ${teacherName} · Rm ${roomNum}`}
                    className={cn(
                      "absolute inset-x-0.5 overflow-hidden rounded-sm border px-1 py-0.5 text-[11px] font-medium leading-tight",
                      block.hasConflict
                        ? "border-rose-400 bg-rose-100 text-rose-900 dark:border-rose-700 dark:bg-rose-950/60 dark:text-rose-200"
                        : "border-blue-300 bg-blue-100 text-blue-900 dark:border-blue-700 dark:bg-blue-950/60 dark:text-blue-200",
                    )}
                  >
                    {block.hasConflict && (
                      <div
                        aria-hidden
                        style={{
                          backgroundImage:
                            "repeating-linear-gradient(45deg, rgba(244,63,94,0.25) 0 4px, transparent 4px 8px)",
                        }}
                        className="pointer-events-none absolute inset-0"
                      />
                    )}
                    <div className="relative truncate font-semibold">
                      {subjectCode}
                    </div>
                    {heightPx >= SLOT_HEIGHT_PX * 1.5 && (
                      <div className="relative truncate text-[10px] opacity-80">
                        {roomNum} · {teacherName}
                      </div>
                    )}
                    {block.hasConflict && (
                      <AlertTriangle className="absolute right-0.5 top-0.5 size-3 text-rose-500" />
                    )}
                  </div>
                );
              })}
            </div>
          );
        })}
      </div>
    </div>
  );
}

interface SectionCardMiniScheduleProps {
  section: ClassSectionMinimal;
  isHovered: boolean;
  onViewDetails?: () => void;
}

/**
 * Lazy-loading weekly schedule preview shown inside a HoverCard. Renders a
 * compact static summary instantly (from `validationSummary`) and fetches the
 * offering details only after the parent flags `isHovered`.
 */
export function SectionCardMiniSchedule({
  section,
  isHovered,
  onViewDetails,
}: SectionCardMiniScheduleProps) {
  const { data: offerings, isLoading } = useQuery({
    ...getOfferingDetailsForClassSectionOptions(section.id),
    enabled: isHovered && !!section.id,
  });

  const vs = section.validationSummary;
  const totalOfferings = vs?.OFFERINGS_COUNT ?? 0;
  const withIssues = vs?.OFFERING_WITH_ISSUE_COUNT ?? 0;

  const blocks = offerings ? buildOfferingBlocks(offerings) : [];
  const conflictBlocks = blocks.filter((b) => b.hasConflict);
  const hoursPerWeek = formatHoursPerWeek(blocks);
  const { startHour, endHour } = computeVisibleRange(blocks);

  // Header summary chips — always visible.
  const summaryChips: { label: string; tone: "neutral" | "danger" | "warn" }[] =
    [];
  if (totalOfferings > 0)
    summaryChips.push({
      label: `${totalOfferings} offering${totalOfferings === 1 ? "" : "s"}`,
      tone: "neutral",
    });
  if (offerings && conflictBlocks.length > 0)
    summaryChips.push({
      label: `${conflictBlocks.length} conflict${conflictBlocks.length === 1 ? "" : "s"}`,
      tone: "danger",
    });
  if (offerings && hoursPerWeek > 0)
    summaryChips.push({
      label: `${hoursPerWeek} hr/wk`,
      tone: "neutral",
    });
  if (!offerings && withIssues > 0)
    summaryChips.push({
      label: `${withIssues} need${withIssues === 1 ? "s" : ""} attention`,
      tone: "warn",
    });

  const chipToneClass: Record<"neutral" | "danger" | "warn", string> = {
    neutral: "border-border bg-muted text-foreground",
    danger:
      "border-rose-200 bg-rose-50 text-rose-700 dark:border-rose-800 dark:bg-rose-950/30 dark:text-rose-400",
    warn: "border-amber-200 bg-amber-50 text-amber-700 dark:border-amber-800 dark:bg-amber-950/30 dark:text-amber-400",
  };

  return (
    <div className="space-y-2">
      {/* Header */}
      <div className="flex items-start justify-between gap-2">
        <div className="min-w-0">
          <div className="truncate text-sm font-semibold">
            {section.fullName ?? section.name}
          </div>
          <div className="text-[11px] text-muted-foreground">
            Weekly schedule
          </div>
        </div>
        {onViewDetails && (
          <Button
            variant="ghost"
            size="sm"
            onClick={onViewDetails}
            className="-mr-1 h-7 gap-1 px-2 text-[11px]"
          >
            <ArrowUpRight className="size-3" />
            Open
          </Button>
        )}
      </div>

      {/* Summary chips */}
      {summaryChips.length > 0 && (
        <div className="flex flex-wrap gap-1">
          {summaryChips.map((c) => (
            <Badge
              key={c.label}
              variant="outline"
              className={cn(
                "h-5 px-1.5 text-[10px] font-medium",
                chipToneClass[c.tone],
              )}
            >
              {c.label}
            </Badge>
          ))}
        </div>
      )}

      {/* Body */}
      {isLoading && (
        <div className="flex items-center gap-2 py-3 text-xs text-muted-foreground">
          <Loader2 className="size-3 animate-spin" />
          Loading weekly schedule…
        </div>
      )}

      {!isLoading && (!offerings || offerings.length === 0) && (
        <div className="rounded-md border border-dashed bg-muted/30 px-3 py-4 text-center text-xs text-muted-foreground">
          No offerings scheduled yet.
        </div>
      )}

      {!isLoading && offerings && offerings.length > 0 && (
        <MiniScheduleGrid
          blocks={blocks}
          startHour={startHour}
          endHour={endHour}
        />
      )}

      {/* Conflict legend */}
      {conflictBlocks.length > 0 && (
        <div className="flex items-center gap-3 border-t pt-2 text-[11px] text-muted-foreground">
          <div className="flex items-center gap-1">
            <span className="inline-block size-2.5 rounded-sm border border-blue-400 bg-blue-200" />
            Offering
          </div>
          <div className="flex items-center gap-1">
            <span className="inline-block size-2.5 rounded-sm border border-rose-400 bg-rose-200" />
            Conflict
          </div>
        </div>
      )}
    </div>
  );
}
