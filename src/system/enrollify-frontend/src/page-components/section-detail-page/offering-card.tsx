import { useState } from "react";
import { deleteOfferingOptions } from "@/api/collections/offering-collection";
import type { Offering } from "@/api/models/offering";
import type { OfferingValidationMessage } from "@/api/models/class-section";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { useMutation } from "@tanstack/react-query";
import {
  ChevronDown,
  ChevronRight,
  AlertCircle,
  Trash2,
  LayoutGrid,
  AlertTriangle,
} from "lucide-react";
import { ScheduleRowList } from "./schedule-row-list";
import { EditOfferingDrawer } from "./edit-offering-drawer";
import { OfferingValidationMessagesDrawer } from "./offering-validation-messages-drawer";

interface OfferingCardProps {
  offering: Offering;
  sectionId: number;
  validationMessages?: OfferingValidationMessage[];
}



export function OfferingCard({
  offering,
  sectionId,
  validationMessages = [],
}: OfferingCardProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [isValidationDrawerOpen, setIsValidationDrawerOpen] = useState(false);

  const { mutateAsync: deleteOffering } = useMutation(
    deleteOfferingOptions(offering.id, sectionId),
  );

  const conflictCount = offering.conflicts?.length ?? 0;
  const hasHardConflict = offering.conflicts?.some(
    (c) => c.severity === "error",
  );

  const displayUnits = offering.effectiveUnits ?? offering.snapshotUnits;
  const hasOverride = offering.subjectUnitsOverride != null;

  const hasValidationMessages = validationMessages.length > 0;
  const hasValidationError = validationMessages.some(
    (msg) => msg.severity.name === "Error",
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
                {offering.snapshotSubjectCode}
              </span>
              <span className="text-sm text-muted-foreground truncate">
                {offering.snapshotSubjectTitle}
              </span>
              <Tooltip>
                <TooltipTrigger asChild>
                  <Badge
                    variant={hasOverride ? "outline" : "secondary"}
                    className={`text-xs cursor-default ${hasOverride ? "border-amber-400 text-amber-700 dark:text-amber-400" : ""}`}
                  >
                    {displayUnits} units
                    {hasOverride && " (override)"}
                  </Badge>
                </TooltipTrigger>
                <TooltipContent>
                  {hasOverride
                    ? `Override: ${offering.subjectUnitsOverride} units (curriculum default: ${offering.snapshotUnits})`
                    : `${displayUnits} units`}
                </TooltipContent>
              </Tooltip>
              {offering.snapshotIsElective && (
                <Badge variant="outline" className="text-xs">
                  {offering.snapshotElectiveGroupName ?? "Elective"}
                </Badge>
              )}
            </div>
            <div className="flex items-center gap-3 mt-0.5 text-xs text-muted-foreground">
              <span>
                {offering.teacher
                  ? `${offering.teacher.firstName} ${offering.teacher.lastName}`
                  : "Unassigned"}
              </span>
              <span>·</span>
              <span>{offering.room?.roomNumber ?? "No room"}</span>
              {(offering.schedules?.length ?? 0) > 0 && (
                <>
                  <span>·</span>
                  <span>{offering.schedules!.length} schedule row(s)</span>
                </>
              )}
            </div>
          </div>

          <div className="flex items-center gap-1 shrink-0">
            {hasOverride && (
              <Tooltip>
                <TooltipTrigger asChild>
                  <span className="text-amber-500 cursor-default">
                    <AlertTriangle className="size-4" />
                  </span>
                </TooltipTrigger>
                <TooltipContent>
                  Units overridden to {offering.subjectUnitsOverride}{" "}
                  (curriculum default: {offering.snapshotUnits})
                </TooltipContent>
              </Tooltip>
            )}
            {hasValidationMessages && (
              <Button
                variant="ghost"
                size="sm"
                className="h-7 px-2 cursor-pointer hover:bg-transparent"
                onClick={() => setIsValidationDrawerOpen(true)}
              >
                <Badge
                  variant={hasValidationError ? "destructive" : "secondary"}
                  className="gap-1 text-xs"
                >
                  <AlertTriangle className="size-3" />
                  {validationMessages.length} issue
                  {validationMessages.length > 1 ? "s" : ""}
                </Badge>
              </Button>
            )}
            {conflictCount > 0 && (
              <Badge
                variant={hasHardConflict ? "destructive" : "secondary"}
                className="gap-1 text-xs"
              >
                <AlertCircle className="size-3" />
                {conflictCount} conflict{conflictCount > 1 ? "s" : ""}
              </Badge>
            )}
            <EditOfferingDrawer offering={offering} sectionId={sectionId} />
            <Button
              variant="ghost"
              size="sm"
              className="h-7 w-7 p-0 hover:bg-destructive/10 hover:text-destructive"
              onClick={() => deleteOffering({})}
              title="Remove offering"
            >
              <Trash2 className="size-3.5" />
            </Button>
          </div>
        </div>

        <CollapsibleContent>
          <div className="border-t px-4 py-3 bg-muted/20 space-y-3">
            <div className="grid grid-cols-7 gap-4 text-xs">
              <div>
                <p className="font-semibold text-muted-foreground uppercase tracking-wide mb-0.5">
                  Max Students
                </p>
                <p>{offering.maxNumberOfStudents ?? "No limit"}</p>
              </div>
              <div>
                <p className="font-semibold text-muted-foreground uppercase tracking-wide mb-0.5">
                  Days/Week
                </p>
                <p>{offering.daysPerWeek ?? "—"}</p>
              </div>
              <div>
                <p className="font-semibold text-muted-foreground uppercase tracking-wide mb-0.5">
                  Hrs/Day
                </p>
                <p>{offering.hoursPerDay ?? "—"}</p>
              </div>
            </div>
            <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide">
              Schedule Rows
            </p>
            <ScheduleRowList
              offeringId={offering.id}
              sectionId={sectionId}
              schedules={offering.schedules ?? []}
              hoursPerDay={offering.hoursPerDay}
              daysPerWeek={offering.daysPerWeek}
            />
          </div>
        </CollapsibleContent>
      </div>

      <OfferingValidationMessagesDrawer
        isOpen={isValidationDrawerOpen}
        onOpenChange={setIsValidationDrawerOpen}
        offeringSubjectCode={offering.snapshotSubjectCode ?? "Unknown"}
        validationMessages={validationMessages}
      />
    </Collapsible>
  );
}
