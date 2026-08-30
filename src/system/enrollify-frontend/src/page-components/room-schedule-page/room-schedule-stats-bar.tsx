import {
  AlertTriangle,
  BookOpen,
  DoorClosed,
  DoorOpen,
  Layers,
} from "lucide-react";

import type { RoomScheduleStats } from "@/api/models/class-scheduling/room-schedule";
import { cn } from "@/lib/utils";

interface RoomScheduleStatsBarProps {
  stats: RoomScheduleStats;
}

function StatCard({
  icon,
  label,
  value,
  emphasis,
}: {
  icon: React.ReactNode;
  label: string;
  value: number;
  emphasis?: "danger" | "warning";
}) {
  return (
    <div className="flex items-center gap-2.5 rounded-lg border bg-card px-3 py-2">
      <div
        className={cn(
          "flex size-8 items-center justify-center rounded-md bg-surface-sunken text-muted-foreground",
          emphasis === "danger" &&
            value > 0 &&
            "bg-destructive/10 text-destructive",
          emphasis === "warning" &&
            value > 0 &&
            "bg-amber-500/10 text-amber-600",
        )}
      >
        {icon}
      </div>
      <div className="flex flex-col leading-none">
        <span
          className={cn(
            "text-lg font-bold",
            emphasis === "danger" && value > 0 && "text-destructive",
          )}
        >
          {value}
        </span>
        <span className="text-[11px] text-muted-foreground">{label}</span>
      </div>
    </div>
  );
}

export function RoomScheduleStatsBar({ stats }: RoomScheduleStatsBarProps) {
  return (
    <div className="flex flex-wrap gap-2">
      <StatCard
        icon={<DoorOpen className="size-4" />}
        label="Total rooms"
        value={stats.totalRooms}
      />
      <StatCard
        icon={<DoorClosed className="size-4" />}
        label="Rooms in use"
        value={stats.roomsInUse}
      />
      <StatCard
        icon={<BookOpen className="size-4" />}
        label="Scheduled classes"
        value={stats.totalOfferings}
      />
      <StatCard
        icon={<Layers className="size-4" />}
        label="Unassigned"
        value={stats.unassignedOfferings}
        emphasis="warning"
      />
      <StatCard
        icon={<AlertTriangle className="size-4" />}
        label="Room conflicts"
        value={stats.totalConflicts}
        emphasis="danger"
      />
    </div>
  );
}
