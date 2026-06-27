import { Button } from "@/components/ui/button";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import {
  ClassSectionStatusEnum,
  type ClassSectionMinimal,
} from "@/api/models/class-scheduling/class-section";
import {
  CheckCircle2,
  UserRound,
  X,
  XCircle,
} from "lucide-react";
import { cn } from "@/lib/utils";

interface BatchActionsToolbarProps {
  selectedSectionIds: Set<number>;
  sections: ClassSectionMinimal[];
  onClearSelection: () => void;
  onBulkOpen: (sectionIds: number[]) => void;
  onBulkCancel: (sectionIds: number[]) => void;
  onAssignAdviser: (sectionIds: number[]) => void;
  className?: string;
}

export function BatchActionsToolbar({
  selectedSectionIds,
  sections,
  onClearSelection,
  onBulkOpen,
  onBulkCancel,
  onAssignAdviser,
  className,
}: BatchActionsToolbarProps) {
  if (selectedSectionIds.size === 0) return null;

  const selectedSections = sections.filter((s) =>
    selectedSectionIds.has(s.id),
  );

  const draftSections = selectedSections.filter(
    (s) => s.status.value === ClassSectionStatusEnum.Draft,
  );
  const hasMixedStatuses = selectedSections.some(
    (s) => s.status.value !== ClassSectionStatusEnum.Draft,
  );
  const allDraft = !hasMixedStatuses && draftSections.length > 0;

  const selectedIds = Array.from(selectedSectionIds);
  const draftIds = draftSections.map((s) => s.id);

  return (
    <div
      className={cn(
        "flex items-center gap-2 flex-wrap rounded-lg border bg-muted/50 px-3 py-2",
        className,
      )}
      role="toolbar"
      aria-label="Batch actions"
    >
      {/* Selection count */}
      <div className="flex items-center gap-2 text-sm">
        <span className="font-semibold text-primary tabular-nums">
          {selectedSectionIds.size}
        </span>
        <span className="text-muted-foreground">
          {selectedSectionIds.size === 1 ? "section" : "sections"} selected
        </span>
        <Button
          variant="ghost"
          size="sm"
          onClick={onClearSelection}
          className="h-6 w-6 p-0 text-muted-foreground hover:text-foreground"
          aria-label="Clear selection"
        >
          <X className="size-3.5" />
        </Button>
      </div>

      <div className="h-4 w-px bg-border mx-1" />

      {/* Mixed status warning */}
      {hasMixedStatuses && (
        <span className="text-xs text-amber-600 dark:text-amber-400">
          ⚠ Only Draft sections support batch operations
        </span>
      )}

      {/* Batch actions — only shown for Draft sections */}
      <Tooltip>
        <TooltipTrigger asChild>
          <Button
            variant="outline"
            size="sm"
            disabled={!allDraft && draftIds.length === 0}
            onClick={() => onAssignAdviser(allDraft ? selectedIds : draftIds)}
            className="h-7 gap-1.5 text-xs"
          >
            <UserRound className="size-3.5" />
            Assign Adviser
            {hasMixedStatuses && draftIds.length > 0 && (
              <span className="ml-1 opacity-60">({draftIds.length})</span>
            )}
          </Button>
        </TooltipTrigger>
        <TooltipContent>
          {hasMixedStatuses
            ? `Assign adviser to ${draftIds.length} Draft section${draftIds.length !== 1 ? "s" : ""}`
            : "Assign adviser to selected sections"}
        </TooltipContent>
      </Tooltip>

      <Tooltip>
        <TooltipTrigger asChild>
          <Button
            variant="outline"
            size="sm"
            disabled={!allDraft && draftIds.length === 0}
            onClick={() => onBulkOpen(allDraft ? selectedIds : draftIds)}
            className="h-7 gap-1.5 text-xs border-emerald-200 bg-emerald-50 text-emerald-700 hover:bg-emerald-100 dark:border-emerald-800 dark:bg-emerald-950/30 dark:text-emerald-400"
          >
            <CheckCircle2 className="size-3.5" />
            Open for Enrollment
            {hasMixedStatuses && draftIds.length > 0 && (
              <span className="ml-1 opacity-60">({draftIds.length})</span>
            )}
          </Button>
        </TooltipTrigger>
        <TooltipContent>
          {hasMixedStatuses
            ? `Open ${draftIds.length} Draft section${draftIds.length !== 1 ? "s" : ""} for enrollment`
            : "Open selected sections for enrollment"}
        </TooltipContent>
      </Tooltip>

      <Tooltip>
        <TooltipTrigger asChild>
          <Button
            variant="outline"
            size="sm"
            disabled={selectedIds.length === 0}
            onClick={() => onBulkCancel(selectedIds)}
            className="h-7 gap-1.5 text-xs border-destructive/20 bg-destructive/5 text-destructive hover:bg-destructive/10 dark:border-destructive/30"
          >
            <XCircle className="size-3.5" />
            Cancel
          </Button>
        </TooltipTrigger>
        <TooltipContent>Cancel selected sections</TooltipContent>
      </Tooltip>
    </div>
  );
}
