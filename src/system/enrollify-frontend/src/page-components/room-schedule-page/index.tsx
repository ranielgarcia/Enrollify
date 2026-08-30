import { Suspense, useMemo, useState } from "react";
import { useQueryStates } from "nuqs";
import { useSuspenseQuery } from "@tanstack/react-query";
import { CalendarX } from "lucide-react";

import { getRoomScheduleOptions } from "@/api/collections/room-schedule-collection";
import type { RoomScheduleOffering } from "@/api/models/class-scheduling/room-schedule";
import { ManagementPageLayout } from "@/components/page-layouts/management-page-layout";
import { Skeleton } from "@/components/ui/skeleton";
import { ModuleIcons } from "@/config/module-icons";
import { useEnrollmentContext } from "@/contexts/enrollment-context/enrollment-context";

import { getCourseColor } from "./color-by-course";
import { OfferingDetailDrawer } from "./offering-detail-drawer";
import { RoomScheduleGrid } from "./room-schedule-grid";
import { RoomScheduleStatsBar } from "./room-schedule-stats-bar";
import { RoomScheduleToolbar } from "./room-schedule-toolbar";
import { searchParams, type DayKey } from "./searchParams";

export default function RoomSchedulePage() {
  const [
    { dayOfWeek, buildingId, roomTypeId, collegeId, courseId },
    setParams,
  ] = useQueryStates(searchParams);
  const { selectedAcademicTerm } = useEnrollmentContext();

  const filters = { buildingId, roomTypeId, collegeId, courseId };

  return (
    <ManagementPageLayout
      title="Room Schedule"
      description="View class bookings by room for the selected day and term."
      icon={<ModuleIcons.rooms className="size-6 text-primary" />}
      createNewItemButton={null}
    >
      <div className="flex flex-col gap-4">
        <RoomScheduleToolbar
          dayOfWeek={dayOfWeek}
          onDayChange={(day: DayKey) => setParams({ dayOfWeek: day })}
          filters={filters}
          onFilterChange={(patch) => setParams(patch)}
          onClearFilters={() =>
            setParams({
              buildingId: null,
              roomTypeId: null,
              collegeId: null,
              courseId: null,
            })
          }
        />

        {!selectedAcademicTerm ? (
          <div className="flex flex-col items-center justify-center gap-2 rounded-md border border-dashed py-16 text-center">
            <CalendarX className="size-8 text-muted-foreground" />
            <p className="text-sm font-medium">No academic term selected</p>
            <p className="text-xs text-muted-foreground">
              Select an academic term to view the room schedule.
            </p>
          </div>
        ) : (
          <Suspense fallback={<ScheduleSkeleton />}>
            <ScheduleContent
              academicTermId={selectedAcademicTerm.id}
              dayOfWeek={dayOfWeek}
              buildingId={buildingId}
              roomTypeId={roomTypeId}
              collegeId={collegeId}
              courseId={courseId}
            />
          </Suspense>
        )}
      </div>
    </ManagementPageLayout>
  );
}

interface ScheduleContentProps {
  academicTermId: number;
  dayOfWeek: string;
  buildingId: number | null;
  roomTypeId: number | null;
  collegeId: number | null;
  courseId: number | null;
}

function ScheduleContent({
  academicTermId,
  dayOfWeek,
  buildingId,
  roomTypeId,
  collegeId,
  courseId,
}: ScheduleContentProps) {
  const { data } = useSuspenseQuery(
    getRoomScheduleOptions({
      academicTermId,
      dayOfWeek,
      buildingId,
      roomTypeId,
      collegeId,
      courseId,
    }),
  );

  const [selectedOffering, setSelectedOffering] =
    useState<RoomScheduleOffering | null>(null);
  const [drawerOpen, setDrawerOpen] = useState(false);

  const legend = useMemo(() => {
    const map = new Map<number, string>();
    for (const room of data.rooms) {
      for (const o of room.offerings) map.set(o.courseId, o.courseCode);
    }
    for (const o of data.unassignedOfferings) map.set(o.courseId, o.courseCode);
    return [...map.entries()]
      .map(([id, code]) => ({ id, code }))
      .sort((a, b) => a.code.localeCompare(b.code));
  }, [data]);

  const handleSelect = (offering: RoomScheduleOffering) => {
    setSelectedOffering(offering);
    setDrawerOpen(true);
  };

  return (
    <div className="flex flex-col gap-4">
      <RoomScheduleStatsBar stats={data.stats} />

      {legend.length > 0 && (
        <div className="flex flex-wrap items-center gap-x-3 gap-y-1.5 text-[11px]">
          <span className="font-medium text-muted-foreground">Courses:</span>
          {legend.map((c) => (
            <span key={c.id} className="flex items-center gap-1.5">
              <span
                className={`inline-block size-2.5 rounded-sm ${getCourseColor(c.id).dot}`}
              />
              {c.code}
            </span>
          ))}
          <span className="flex items-center gap-1.5">
            <span className="inline-block size-2.5 rounded-sm bg-destructive" />
            Conflict
          </span>
        </div>
      )}

      <RoomScheduleGrid
        rooms={data.rooms}
        unassigned={data.unassignedOfferings}
        onSelectOffering={handleSelect}
      />

      <OfferingDetailDrawer
        offering={selectedOffering}
        isOpen={drawerOpen}
        onOpenChange={setDrawerOpen}
      />
    </div>
  );
}

function ScheduleSkeleton() {
  return (
    <div className="flex flex-col gap-4">
      <div className="flex gap-2">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-14 w-32" />
        ))}
      </div>
      <Skeleton className="h-[500px] w-full" />
    </div>
  );
}
