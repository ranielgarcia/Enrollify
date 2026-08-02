import { Link } from "@tanstack/react-router";
import { AlertTriangle, ArrowUpRight, Clock, DoorOpen } from "lucide-react";

import type { RoomScheduleOffering } from "@/api/models/class-scheduling/room-schedule";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Drawer,
  DrawerContent,
  DrawerDescription,
  DrawerHeader,
  DrawerTitle,
} from "@/components/ui/drawer";

interface OfferingDetailDrawerProps {
  offering: RoomScheduleOffering | null;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

function formatTime(time: string): string {
  const [h, m] = time.split(":").map(Number);
  const hour = h ?? 0;
  const ampm = hour < 12 ? "AM" : "PM";
  const display = hour === 0 ? 12 : hour > 12 ? hour - 12 : hour;
  return `${display}:${String(m ?? 0).padStart(2, "0")} ${ampm}`;
}

function DetailRow({
  label,
  children,
}: {
  label: string;
  children: React.ReactNode;
}) {
  return (
    <div className="flex flex-col gap-0.5 py-2">
      <span className="text-xs font-medium text-muted-foreground">{label}</span>
      <span className="text-sm">{children}</span>
    </div>
  );
}

export function OfferingDetailDrawer({
  offering,
  isOpen,
  onOpenChange,
}: OfferingDetailDrawerProps) {
  return (
    <Drawer direction="right" open={isOpen} onOpenChange={onOpenChange}>
      <DrawerContent className="flex h-full flex-col overflow-hidden data-[vaul-drawer-direction=right]:w-[440px] data-[vaul-drawer-direction=right]:sm:max-w-none">
        {offering && (
          <>
            <DrawerHeader className="shrink-0 border-b pb-4">
              <DrawerTitle className="flex flex-wrap items-center gap-2">
                {offering.subjectCode}
                {offering.hasConflict && (
                  <Badge variant="destructive" className="gap-1">
                    <AlertTriangle className="size-3" />
                    Room double-booked
                  </Badge>
                )}
              </DrawerTitle>
              <DrawerDescription>{offering.subjectTitle}</DrawerDescription>
            </DrawerHeader>

            <div className="flex-1 overflow-y-auto px-4">
              <div className="divide-y">
                <DetailRow label="Section">{offering.sectionName}</DetailRow>
                <DetailRow label="Course">{offering.courseCode}</DetailRow>
                <DetailRow label="Teacher">
                  {offering.teacherName ?? (
                    <span className="text-muted-foreground italic">
                      Unassigned
                    </span>
                  )}
                </DetailRow>
                <DetailRow label="Schedule">
                  <span className="flex items-center gap-1.5">
                    <Clock className="size-3.5 text-muted-foreground" />
                    {offering.dayOfWeek} · {formatTime(offering.startTime)} –{" "}
                    {formatTime(offering.endTime)}
                  </span>
                </DetailRow>
                <DetailRow label="Room">
                  {offering.roomId ? (
                    <span className="flex items-center gap-1.5">
                      <DoorOpen className="size-3.5 text-muted-foreground" />
                      Assigned
                    </span>
                  ) : (
                    <span className="text-muted-foreground italic">
                      No room assigned
                    </span>
                  )}
                </DetailRow>
              </div>

              {offering.conflicts.length > 0 && (
                <div className="mt-4">
                  <h4 className="mb-2 flex items-center gap-1.5 text-sm font-semibold text-destructive">
                    <AlertTriangle className="size-4" />
                    Conflicting bookings ({offering.conflicts.length})
                  </h4>
                  <ul className="space-y-2">
                    {offering.conflicts.map((c) => (
                      <li
                        key={c.offeringId}
                        className="rounded-md border border-destructive/40 bg-destructive/5 p-2 text-xs"
                      >
                        <div className="font-medium">
                          {c.subjectCode} · {c.sectionName}
                        </div>
                        <div className="text-muted-foreground">
                          {formatTime(c.startTime)} – {formatTime(c.endTime)}
                          {c.teacherName ? ` · ${c.teacherName}` : ""}
                        </div>
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>

            <div className="shrink-0 border-t p-4">
              <Button asChild className="w-full">
                <Link
                  to="/portal/curriculum-and-scheduling/sections-details/$sectionId"
                  params={{ sectionId: offering.sectionId.toString() }}
                >
                  Open section details
                  <ArrowUpRight className="size-4" />
                </Link>
              </Button>
            </div>
          </>
        )}
      </DrawerContent>
    </Drawer>
  );
}
