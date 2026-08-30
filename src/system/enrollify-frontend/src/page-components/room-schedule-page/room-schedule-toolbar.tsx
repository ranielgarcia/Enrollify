import { useQuery } from "@tanstack/react-query";
import { X } from "lucide-react";

import { getAllBuildingsOptions } from "@/api/collections/building-collection";
import { getAllRoomTypesOptions } from "@/api/collections/room-type-collection";
import { getAllCollegesOptions } from "@/api/collections/college-collection";
import { getAllCoursesOptions } from "@/api/collections/course-collection";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";
import { cn } from "@/lib/utils";

import { DAY_OPTIONS, type DayKey } from "./searchParams";

const ALL = "all";

export interface RoomScheduleFilterValues {
  buildingId: number | null;
  roomTypeId: number | null;
  collegeId: number | null;
  courseId: number | null;
}

interface RoomScheduleToolbarProps {
  dayOfWeek: DayKey;
  onDayChange: (day: DayKey) => void;
  filters: RoomScheduleFilterValues;
  onFilterChange: (patch: Partial<RoomScheduleFilterValues>) => void;
  onClearFilters: () => void;
}

interface FilterSelectProps {
  placeholder: string;
  value: number | null;
  options: { id: number; label: string }[];
  onChange: (value: number | null) => void;
}

function FilterSelect({
  placeholder,
  value,
  options,
  onChange,
}: FilterSelectProps) {
  return (
    <Select
      value={value != null ? String(value) : ALL}
      onValueChange={(v) => onChange(v === ALL ? null : Number(v))}
    >
      <SelectTrigger className="h-9 w-[170px] text-xs">
        <SelectValue placeholder={placeholder} />
      </SelectTrigger>
      <SelectContent>
        <SelectItem value={ALL}>All {placeholder.toLowerCase()}</SelectItem>
        {options.map((o) => (
          <SelectItem key={o.id} value={String(o.id)}>
            {o.label}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}

export function RoomScheduleToolbar({
  dayOfWeek,
  onDayChange,
  filters,
  onFilterChange,
  onClearFilters,
}: RoomScheduleToolbarProps) {
  const { data: buildings } = useQuery(getAllBuildingsOptions());
  const { data: roomTypes } = useQuery(getAllRoomTypesOptions());
  const { data: colleges } = useQuery(getAllCollegesOptions());
  const { data: courses } = useQuery(getAllCoursesOptions());

  const hasActiveFilters =
    filters.buildingId != null ||
    filters.roomTypeId != null ||
    filters.collegeId != null ||
    filters.courseId != null;

  return (
    <div className="flex flex-col gap-3">
      {/* Day selector */}
      <ToggleGroup
        type="single"
        value={dayOfWeek}
        onValueChange={(v) => v && onDayChange(v as DayKey)}
        className="w-fit rounded-lg border bg-card p-1"
      >
        {DAY_OPTIONS.map((d) => (
          <ToggleGroupItem
            key={d.key}
            value={d.key}
            className={cn(
              "h-8 rounded-md px-3 text-xs data-[state=on]:bg-primary data-[state=on]:text-primary-foreground",
            )}
          >
            <span className="hidden sm:inline">{d.label}</span>
            <span className="sm:hidden">{d.short}</span>
          </ToggleGroupItem>
        ))}
      </ToggleGroup>

      {/* Filters */}
      <div className="flex flex-wrap items-center gap-2">
        <FilterSelect
          placeholder="Buildings"
          value={filters.buildingId}
          options={(buildings ?? []).map((b) => ({ id: b.id, label: b.name }))}
          onChange={(v) => onFilterChange({ buildingId: v })}
        />
        <FilterSelect
          placeholder="Room types"
          value={filters.roomTypeId}
          options={(roomTypes ?? []).map((r) => ({ id: r.id, label: r.name }))}
          onChange={(v) => onFilterChange({ roomTypeId: v })}
        />
        <FilterSelect
          placeholder="Colleges"
          value={filters.collegeId}
          options={(colleges ?? []).map((c) => ({
            id: c.id,
            label: c.code,
          }))}
          onChange={(v) => onFilterChange({ collegeId: v })}
        />
        <FilterSelect
          placeholder="Courses"
          value={filters.courseId}
          options={(courses ?? []).map((c) => ({ id: c.id, label: c.code }))}
          onChange={(v) => onFilterChange({ courseId: v })}
        />
        {hasActiveFilters && (
          <Button
            variant="ghost"
            size="sm"
            onClick={onClearFilters}
            className="h-9 text-xs text-muted-foreground"
          >
            <X className="size-3.5" />
            Clear
          </Button>
        )}
      </div>
    </div>
  );
}
