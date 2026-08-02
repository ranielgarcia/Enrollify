import { useMemo } from "react";

import { ScrollArea, ScrollBar } from "@/components/ui/scroll-area";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";
import { AlertTriangle, DoorOpen } from "lucide-react";

import type {
  RoomScheduleOffering,
  RoomScheduleRoom,
} from "@/api/models/class-scheduling/room-schedule";
import { getBlockColor } from "./color-by-course";

const PX_PER_MINUTE = 2.4;
const LANE_HEIGHT = 40;
const LANE_GAP = 4;
const ROW_PADDING = 8;
const ROOM_COL_WIDTH = 200;
const DEFAULT_WINDOW_START = 7 * 60; // 07:00
const DEFAULT_WINDOW_END = 19 * 60; // 19:00

interface RoomScheduleGridProps {
  rooms: RoomScheduleRoom[];
  unassigned: RoomScheduleOffering[];
  onSelectOffering: (offering: RoomScheduleOffering) => void;
}

function timeToMinutes(time: string): number {
  const [h, m] = time.split(":").map(Number);
  return (h ?? 0) * 60 + (m ?? 0);
}

function formatHour(minutes: number): string {
  const h = Math.floor(minutes / 60);
  const ampm = h < 12 ? "AM" : "PM";
  const display = h === 0 ? 12 : h > 12 ? h - 12 : h;
  return `${display} ${ampm}`;
}

function formatTimeRange(start: string, end: string): string {
  const fmt = (t: string) => {
    const [h, m] = t.split(":").map(Number);
    const hour = h ?? 0;
    const ampm = hour < 12 ? "AM" : "PM";
    const display = hour === 0 ? 12 : hour > 12 ? hour - 12 : hour;
    return `${display}:${String(m ?? 0).padStart(2, "0")} ${ampm}`;
  };
  return `${fmt(start)} – ${fmt(end)}`;
}

interface PositionedOffering {
  offering: RoomScheduleOffering;
  startMin: number;
  endMin: number;
  lane: number;
}

/** Packs offerings into lanes so overlapping blocks are both visible. */
function packLanes(offerings: RoomScheduleOffering[]): {
  positioned: PositionedOffering[];
  laneCount: number;
} {
  const sorted = [...offerings].sort(
    (a, b) => timeToMinutes(a.startTime) - timeToMinutes(b.startTime),
  );
  const laneEnds: number[] = [];
  const positioned: PositionedOffering[] = [];

  for (const offering of sorted) {
    const startMin = timeToMinutes(offering.startTime);
    const endMin = timeToMinutes(offering.endTime);
    let lane = laneEnds.findIndex((end) => end <= startMin);
    if (lane === -1) {
      lane = laneEnds.length;
      laneEnds.push(endMin);
    } else {
      laneEnds[lane] = endMin;
    }
    positioned.push({ offering, startMin, endMin, lane });
  }

  return { positioned, laneCount: Math.max(laneEnds.length, 1) };
}

interface RowModel {
  key: string;
  isUnassigned: boolean;
  room?: RoomScheduleRoom;
  positioned: PositionedOffering[];
  laneCount: number;
  hasConflict: boolean;
}

export function RoomScheduleGrid({
  rooms,
  unassigned,
  onSelectOffering,
}: RoomScheduleGridProps) {
  const { rows, windowStart, windowEnd } = useMemo(() => {
    const allOfferings = [...rooms.flatMap((r) => r.offerings), ...unassigned];

    let minStart = DEFAULT_WINDOW_START;
    let maxEnd = DEFAULT_WINDOW_END;
    for (const o of allOfferings) {
      minStart = Math.min(minStart, timeToMinutes(o.startTime));
      maxEnd = Math.max(maxEnd, timeToMinutes(o.endTime));
    }
    // Snap to whole hours for clean tick marks.
    const start = Math.floor(minStart / 60) * 60;
    const end = Math.ceil(maxEnd / 60) * 60;

    const rowModels: RowModel[] = rooms.map((room) => {
      const { positioned, laneCount } = packLanes(room.offerings);
      return {
        key: `room-${room.id}`,
        isUnassigned: false,
        room,
        positioned,
        laneCount,
        hasConflict: room.hasConflicts,
      };
    });

    if (unassigned.length > 0) {
      const { positioned, laneCount } = packLanes(unassigned);
      rowModels.push({
        key: "unassigned",
        isUnassigned: true,
        positioned,
        laneCount,
        hasConflict: positioned.some((p) => p.offering.hasConflict),
      });
    }

    return { rows: rowModels, windowStart: start, windowEnd: end };
  }, [rooms, unassigned]);

  const totalMinutes = windowEnd - windowStart;
  const trackWidth = totalMinutes * PX_PER_MINUTE;
  const hourTicks = useMemo(() => {
    const ticks: number[] = [];
    for (let m = windowStart; m <= windowEnd; m += 60) ticks.push(m);
    return ticks;
  }, [windowStart, windowEnd]);

  const rowHeight = (laneCount: number) =>
    laneCount * LANE_HEIGHT + (laneCount - 1) * LANE_GAP + ROW_PADDING * 2;

  return (
    <ScrollArea className="rounded-md border">
      <div className="min-w-max">
        {/* Header: time axis */}
        <div className="flex sticky top-0 z-20 bg-muted/95 backdrop-blur border-b">
          <div
            className="sticky left-0 z-30 shrink-0 border-r bg-muted/95 px-3 py-2 text-xs font-semibold"
            style={{ width: ROOM_COL_WIDTH }}
          >
            Room
          </div>
          <div className="relative" style={{ width: trackWidth, height: 32 }}>
            {hourTicks.map((tick) => (
              <div
                key={tick}
                className="absolute top-0 flex h-full flex-col justify-center border-l border-border/50 pl-1 text-[10px] text-muted-foreground"
                style={{ left: (tick - windowStart) * PX_PER_MINUTE }}
              >
                {formatHour(tick)}
              </div>
            ))}
          </div>
        </div>

        {/* Rows */}
        {rows.map((row) => (
          <div
            key={row.key}
            className={cn(
              "flex border-b last:border-b-0",
              row.isUnassigned && "bg-muted/30",
            )}
          >
            {/* Sticky room label */}
            <div
              className={cn(
                "sticky left-0 z-10 flex shrink-0 flex-col justify-center gap-0.5 border-r bg-background px-3 py-2",
                row.isUnassigned && "bg-muted/60",
              )}
              style={{ width: ROOM_COL_WIDTH }}
            >
              {row.isUnassigned ? (
                <div className="flex items-center gap-1.5 text-sm font-semibold text-muted-foreground">
                  <AlertTriangle className="size-3.5" />
                  Unassigned
                </div>
              ) : (
                <>
                  <div className="flex items-center gap-1.5 text-sm font-semibold">
                    <DoorOpen className="size-3.5 text-muted-foreground" />
                    {row.room?.roomNumber}
                    {row.hasConflict && (
                      <AlertTriangle className="size-3.5 text-destructive" />
                    )}
                  </div>
                  <div className="truncate text-[11px] text-muted-foreground">
                    {row.room?.buildingName ?? "—"}
                    {row.room?.roomTypeName
                      ? ` · ${row.room.roomTypeName}`
                      : ""}
                    {` · ${row.room?.capacity ?? 0} seats`}
                  </div>
                </>
              )}
            </div>

            {/* Time track */}
            <div
              className="relative"
              style={{ width: trackWidth, height: rowHeight(row.laneCount) }}
            >
              {/* hour gridlines */}
              {hourTicks.map((tick) => (
                <div
                  key={tick}
                  className="absolute top-0 h-full border-l border-border/30"
                  style={{ left: (tick - windowStart) * PX_PER_MINUTE }}
                />
              ))}

              {row.positioned.map(({ offering, startMin, endMin, lane }) => {
                const color = getBlockColor(
                  offering.courseId,
                  offering.hasConflict,
                );
                const left = (startMin - windowStart) * PX_PER_MINUTE;
                const width = Math.max(
                  (endMin - startMin) * PX_PER_MINUTE - 2,
                  24,
                );
                const top = lane * (LANE_HEIGHT + LANE_GAP) + ROW_PADDING;
                return (
                  <Tooltip key={offering.scheduleId}>
                    <TooltipTrigger asChild>
                      <button
                        type="button"
                        onClick={() => onSelectOffering(offering)}
                        className={cn(
                          "absolute overflow-hidden rounded-md border px-2 py-1 text-left transition-shadow hover:shadow-md focus:outline-none focus-visible:ring-2 focus-visible:ring-ring",
                          color.block,
                        )}
                        style={{
                          left,
                          width,
                          top,
                          height: LANE_HEIGHT,
                        }}
                      >
                        <div className="flex items-center gap-1 text-[11px] font-semibold leading-tight">
                          {offering.hasConflict && (
                            <AlertTriangle className="size-3 shrink-0" />
                          )}
                          <span className="truncate">
                            {offering.subjectCode}
                          </span>
                        </div>
                        <div className="truncate text-[10px] leading-tight opacity-80">
                          {offering.sectionName}
                          {offering.teacherName
                            ? ` · ${offering.teacherName}`
                            : ""}
                        </div>
                      </button>
                    </TooltipTrigger>
                    <TooltipContent
                      side="top"
                      className="max-w-[240px] text-xs"
                    >
                      <p className="font-semibold">{offering.subjectCode}</p>
                      <p className="text-muted-foreground">
                        {offering.subjectTitle}
                      </p>
                      <p className="mt-1">
                        {offering.sectionName} · {offering.courseCode}
                      </p>
                      <p>
                        {formatTimeRange(offering.startTime, offering.endTime)}
                      </p>
                      {offering.teacherName && <p>{offering.teacherName}</p>}
                      {offering.hasConflict && (
                        <p className="mt-1 flex items-center gap-1 text-destructive">
                          <AlertTriangle className="size-3" />
                          Room double-booked
                        </p>
                      )}
                    </TooltipContent>
                  </Tooltip>
                );
              })}

              {row.positioned.length === 0 && (
                <div className="flex h-full items-center pl-2 text-[11px] italic text-muted-foreground/50">
                  No classes scheduled
                </div>
              )}
            </div>
          </div>
        ))}

        {rows.length === 0 && (
          <div className="p-8 text-center text-sm text-muted-foreground">
            No rooms match the current filters.
          </div>
        )}
      </div>
      <ScrollBar orientation="horizontal" />
    </ScrollArea>
  );
}
