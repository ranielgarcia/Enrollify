import { useQuery } from "@tanstack/react-query";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import {
  Drawer,
  DrawerContent,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
  DrawerDescription,
} from "@/components/ui/drawer";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { getOfferingDetailsForClassSectionOptions } from "@/api/collections/class-section-collection";
import type { Offering } from "@/api/models/class-scheduling/offering";
import {
  AlertTriangle,
  BookOpen,
  CheckCircle2,
  ChevronDown,
  Clock,
  DoorOpen,
  Map,
  Siren,
} from "lucide-react";
import { cn } from "@/lib/utils";

interface ConflictPreviewDrawerProps {
  sectionId: number | null;
  sectionName: string;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

function ConflictItem({
  conflict,
}: {
  conflict: NonNullable<Offering["conflicts"]>[number];
}) {
  const severity = conflict.type?.severity ?? "Error";
  const isError = severity === "Error";

  return (
    <div
      className={cn(
        "rounded-lg border p-3 space-y-2",
        isError
          ? "border-rose-200 bg-rose-50 dark:border-rose-900 dark:bg-rose-950/20"
          : "border-amber-200 bg-amber-50 dark:border-amber-900 dark:bg-amber-950/20",
      )}
    >
      <div className="flex items-start gap-2">
        {isError ? (
          <Siren className="size-4 text-rose-500 mt-0.5 shrink-0" />
        ) : (
          <AlertTriangle className="size-4 text-amber-500 mt-0.5 shrink-0" />
        )}
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 mb-1 flex-wrap">
            <Badge
              variant="outline"
              className={cn(
                "text-xs",
                isError
                  ? "border-rose-300 text-rose-700 dark:text-rose-400"
                  : "border-amber-300 text-amber-700 dark:text-amber-400",
              )}
            >
              {severity}
            </Badge>
            <span className="text-xs font-mono text-muted-foreground">
              {conflict.type?.code}
            </span>
          </div>
          <p className="text-sm font-medium">{conflict.message}</p>

          {/* Conflict time info */}
          {conflict.DayOfWeek && (
            <div className="flex items-center gap-1.5 text-xs text-muted-foreground mt-1">
              <Clock className="size-3" />
              {conflict.DayOfWeek} {conflict.startTime?.substring(0, 5)}–
              {conflict.endTime?.substring(0, 5)}
            </div>
          )}
        </div>
      </div>

      {/* Conflicting offerings */}
      {conflict.ConflictingOfferings &&
        conflict.ConflictingOfferings.length > 0 && (
          <div className="pl-6 space-y-1">
            <div className="text-xs font-medium text-muted-foreground">
              Conflicts with:
            </div>
            {conflict.ConflictingOfferings.map((co, idx) => (
              <div
                key={co.id ?? idx}
                className="flex items-center gap-2 text-xs rounded border bg-background px-2 py-1.5"
              >
                <BookOpen className="size-3 text-muted-foreground shrink-0" />
                <span className="font-medium">{co.subject.code}</span>
                <span className="text-muted-foreground">
                  {co.subject.title}
                </span>
                <span className="text-muted-foreground">·</span>
                <span className="text-muted-foreground">{co.section.name}</span>
                {co.room && (
                  <>
                    <span className="text-muted-foreground">·</span>
                    <span className="flex items-center gap-1 text-muted-foreground">
                      <DoorOpen className="size-3" />
                      {co.room.roomNumber}
                    </span>
                  </>
                )}
              </div>
            ))}
          </div>
        )}
    </div>
  );
}

function OfferingConflictSection({ offering }: { offering: Offering }) {
  const hasConflicts = (offering.conflicts?.length ?? 0) > 0;
  const subjectLabel =
    offering.snapshotSubjectCode ?? `Offering #${offering.id}`;
  const conflictCount = offering.conflicts?.length ?? 0;

  if (!hasConflicts) return null;

  return (
    <Collapsible defaultOpen>
      <CollapsibleTrigger className="flex w-full items-center gap-2 rounded-md border bg-muted/30 px-3 py-2 text-sm transition-colors hover:bg-muted/50 [&[data-state=closed]>svg:first-child]:-rotate-90 [&[data-state=open]>svg:first-child]:rotate-0">
        <ChevronDown className="size-4 shrink-0 text-muted-foreground transition-transform" />
        <BookOpen className="size-4 shrink-0 text-muted-foreground" />
        <span className="font-semibold">{subjectLabel}</span>
        {offering.snapshotSubjectTitle && (
          <span className="text-xs text-muted-foreground">
            — {offering.snapshotSubjectTitle}
          </span>
        )}
        <Badge variant="destructive" className="ml-auto text-xs">
          {conflictCount} {conflictCount === 1 ? "conflict" : "conflicts"}
        </Badge>
      </CollapsibleTrigger>
      <CollapsibleContent>
        <div className="mt-2 space-y-2">
          {offering.conflicts?.map((conflict, idx) => (
            <ConflictItem key={conflict.id ?? idx} conflict={conflict} />
          ))}
        </div>
      </CollapsibleContent>
    </Collapsible>
  );
}

export function ConflictPreviewDrawer({
  sectionId,
  sectionName,
  isOpen,
  onOpenChange,
}: ConflictPreviewDrawerProps) {
  const { data: offerings, isLoading } = useQuery({
    ...getOfferingDetailsForClassSectionOptions(sectionId ?? 0),
    enabled: isOpen && !!sectionId,
  });

  const offeringsWithConflicts =
    offerings?.filter((o) => (o.conflicts?.length ?? 0) > 0) ?? [];

  const totalConflicts = offeringsWithConflicts.reduce(
    (sum, o) => sum + (o.conflicts?.length ?? 0),
    0,
  );

  const errorConflicts = offeringsWithConflicts.reduce(
    (sum, o) =>
      sum +
      (o.conflicts?.filter((c) => c.type?.severity === "Error").length ?? 0),
    0,
  );

  return (
    <Drawer open={isOpen} onOpenChange={onOpenChange} direction="right">
      <DrawerContent className="data-[vaul-drawer-direction=right]:w-[480px] data-[vaul-drawer-direction=right]:sm:max-w-none h-full overflow-y-auto overflow-x-hidden">
        <DrawerHeader className="border-b pb-4">
          <DrawerTitle className="flex items-center gap-2">
            <Siren className="size-5 text-rose-500" />
            Conflict Details
          </DrawerTitle>
          <DrawerDescription>
            Scheduling conflicts for{" "}
            <span className="font-semibold text-foreground">{sectionName}</span>
          </DrawerDescription>
        </DrawerHeader>

        <div className="flex flex-col gap-4 p-4">
          {isLoading ? (
            <div className="space-y-3">
              {Array.from({ length: 3 }).map((_, i) => (
                <Skeleton key={i} className="h-24 w-full rounded-lg" />
              ))}
            </div>
          ) : offeringsWithConflicts.length === 0 ? (
            <div className="flex flex-col items-center gap-3 py-12 text-center">
              <CheckCircle2 className="size-10 text-emerald-400" />
              <div>
                <p className="font-medium text-sm">No Conflicts Detected</p>
                <p className="text-xs text-muted-foreground mt-0.5">
                  This section has no scheduling conflicts.
                </p>
              </div>
            </div>
          ) : (
            <>
              {errorConflicts > 0 && (
                <Alert variant="destructive">
                  <AlertTitle>Cannot open for enrollment</AlertTitle>
                  <AlertDescription>
                    {errorConflicts} Error-severity conflict
                    {errorConflicts !== 1 ? "s" : ""} must be resolved in the
                    Room Scheduler before this section can be opened.
                  </AlertDescription>
                </Alert>
              )}

              {/* Summary */}
              <div className="flex items-center gap-3 rounded-lg border bg-muted/30 p-3">
                <div className="flex items-center gap-2 text-sm">
                  <Siren className="size-4 text-rose-500" />
                  <span className="font-semibold tabular-nums text-rose-600 dark:text-rose-400">
                    {totalConflicts}
                  </span>
                  <span className="text-muted-foreground">
                    total conflict{totalConflicts !== 1 ? "s" : ""}
                  </span>
                </div>
                <div className="h-4 w-px bg-border" />
                <div className="flex items-center gap-2 text-sm">
                  <AlertTriangle className="size-4 text-rose-500" />
                  <span className="font-semibold tabular-nums text-rose-600 dark:text-rose-400">
                    {errorConflicts}
                  </span>
                  <span className="text-muted-foreground">
                    blocking enrollment
                  </span>
                </div>
              </div>

              {/* Conflict list by offering */}
              <div className="space-y-4">
                {offeringsWithConflicts.map((offering) => (
                  <OfferingConflictSection
                    key={offering.id}
                    offering={offering}
                  />
                ))}
              </div>
            </>
          )}
        </div>

        <DrawerFooter className="border-t pt-4">
          <Button
            variant="outline"
            onClick={() => {
              // stub: navigation to room scheduler TBD
            }}
            className="w-full gap-2"
          >
            <Map className="size-4" />
            Open Room Scheduler
          </Button>
        </DrawerFooter>
      </DrawerContent>
    </Drawer>
  );
}
