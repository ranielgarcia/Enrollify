import { useState } from "react";
import { deleteOfferingOptions } from "@/api/collections/offering-collection";
import type { OfferingWithSchedules } from "@/api/models/offering";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { useMutation } from "@tanstack/react-query";
import {
  ChevronDown,
  ChevronRight,
  AlertCircle,
  Trash2,
  LayoutGrid,
} from "lucide-react";
import { ScheduleRowList } from "./schedule-row-list";

interface OfferingCardProps {
  offering: OfferingWithSchedules;
}

export function OfferingCard({ offering }: OfferingCardProps) {
  const [isOpen, setIsOpen] = useState(false);
  const { mutateAsync: deleteOffering } = useMutation(
    deleteOfferingOptions(offering.id),
  );

  const conflictCount = offering.conflicts?.length ?? 0;
  const hasHardConflict = offering.conflicts?.some(
    (c) => c.severity === "error",
  );

  return (
    <Collapsible open={isOpen} onOpenChange={setIsOpen}>
      <div className="rounded-lg border bg-card shadow-sm overflow-hidden">
        <div className="flex items-center gap-3 px-4 py-3">
          <CollapsibleTrigger asChild>
            <Button variant="ghost" size="sm" className="h-7 w-7 p-0 shrink-0">
              {isOpen ? (
                <ChevronDown className="size-4" />
              ) : (
                <ChevronRight className="size-4" />
              )}
            </Button>
          </CollapsibleTrigger>

          <div className="rounded-md bg-primary/10 p-1.5 shrink-0">
            <LayoutGrid className="size-4 text-primary" />
          </div>

          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 flex-wrap">
              <span className="font-semibold text-sm">
                {offering.subject.code}
              </span>
              <span className="text-sm text-muted-foreground truncate">
                {offering.subject.title}
              </span>
              <Badge variant="secondary" className="text-xs">
                {offering.subject.units} units
              </Badge>
            </div>
            <div className="flex items-center gap-3 mt-0.5 text-xs text-muted-foreground">
              <span>
                {offering.teacher.firstName} {offering.teacher.lastName}
              </span>
              <span>·</span>
              <span>{offering.room.roomNumber}</span>
              {offering.dayPattern && (
                <>
                  <span>·</span>
                  <span className="font-mono">{offering.dayPattern}</span>
                </>
              )}
              {offering.schedules.length > 0 && (
                <>
                  <span>·</span>
                  <span>{offering.schedules.length} schedule row(s)</span>
                </>
              )}
            </div>
          </div>

          <div className="flex items-center gap-2 shrink-0">
            {conflictCount > 0 && (
              <Badge
                variant={hasHardConflict ? "destructive" : "secondary"}
                className="gap-1 text-xs"
              >
                <AlertCircle className="size-3" />
                {conflictCount} conflict{conflictCount > 1 ? "s" : ""}
              </Badge>
            )}
            <Button
              variant="ghost"
              size="sm"
              className="h-7 w-7 p-0 hover:bg-destructive/10 hover:text-destructive"
              onClick={() => deleteOffering(undefined as any)}
              title="Remove offering"
            >
              <Trash2 className="size-3.5" />
            </Button>
          </div>
        </div>

        <CollapsibleContent>
          <div className="border-t px-4 py-3 bg-muted/20 space-y-3">
            <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide">
              Schedule Rows
            </p>
            <ScheduleRowList
              offeringId={offering.id}
              schedules={offering.schedules}
            />
          </div>
        </CollapsibleContent>
      </div>
    </Collapsible>
  );
}
