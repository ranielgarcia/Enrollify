import { CheckCircle2, UserRound, X, XCircle } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";

import type { UseSectionSelectionReturn } from "./hooks/use-section-selection";

interface FloatingSelectionToolbarProps {
  selection: UseSectionSelectionReturn;
  onBulkOpen: (sectionIds: number[]) => void;
  onBulkCancel: (sectionIds: number[]) => void;
  onBulkAssignAdviser: (sectionIds: number[]) => void;
  className?: string;
}

/**
 * Floating toolbar pinned to the bottom-center of the viewport whenever the
 * user has selected at least one Draft section. Only Draft sections can be
 * selected (enforced upstream by `useSectionSelection`), so all bulk actions
 * are always safe to invoke on the current `selectedIds`.
 */
export function FloatingSelectionToolbar({
  selection,
  onBulkOpen,
  onBulkCancel,
  onBulkAssignAdviser,
  className,
}: FloatingSelectionToolbarProps) {
  const count = selection.selectedCount;
  if (count === 0) return null;

  const selectedIds = Array.from(selection.selectedIds);

  return (
    <div
      className={cn(
        "pointer-events-none fixed inset-x-0 bottom-6 z-50 flex justify-center px-4",
        className,
      )}
    >
      <div
        role="toolbar"
        aria-label="Bulk actions on selected sections"
        className="pointer-events-auto flex items-center gap-2 rounded-full border bg-card/95 px-3 py-2 shadow-lg backdrop-blur supports-backdrop-filter:bg-card/80 animate-in slide-in-from-bottom-4 fade-in"
      >
        <div className="flex items-center gap-2 pl-1 pr-2 text-sm">
          <span className="flex size-6 items-center justify-center rounded-full bg-primary text-xs font-semibold text-primary-foreground tabular-nums">
            {count}
          </span>
          <span className="text-muted-foreground">
            {count === 1 ? "section selected" : "sections selected"}
          </span>
        </div>

        <Separator orientation="vertical" className="h-6" />

        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              size="sm"
              variant="ghost"
              onClick={() => onBulkOpen(selectedIds)}
              className="h-8 gap-1.5 text-emerald-600 hover:bg-emerald-50 hover:text-emerald-700 dark:text-emerald-400 dark:hover:bg-emerald-950/30"
            >
              <CheckCircle2 className="size-4" />
              <span className="hidden sm:inline">Open</span>
            </Button>
          </TooltipTrigger>
          <TooltipContent side="top">
            Open {count} draft section{count === 1 ? "" : "s"} for enrollment
          </TooltipContent>
        </Tooltip>

        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              size="sm"
              variant="ghost"
              onClick={() => onBulkAssignAdviser(selectedIds)}
              className="h-8 gap-1.5"
            >
              <UserRound className="size-4" />
              <span className="hidden sm:inline">Assign adviser</span>
            </Button>
          </TooltipTrigger>
          <TooltipContent side="top">
            Assign an adviser to all selected sections
          </TooltipContent>
        </Tooltip>

        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              size="sm"
              variant="ghost"
              onClick={() => onBulkCancel(selectedIds)}
              className="h-8 gap-1.5 text-destructive hover:bg-destructive/10 hover:text-destructive"
            >
              <XCircle className="size-4" />
              <span className="hidden sm:inline">Cancel</span>
            </Button>
          </TooltipTrigger>
          <TooltipContent side="top">
            Cancel {count} selected section{count === 1 ? "" : "s"}
          </TooltipContent>
        </Tooltip>

        <Separator orientation="vertical" className="h-6" />

        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              size="sm"
              variant="ghost"
              onClick={selection.clearAll}
              className="h-8 w-8 p-0 text-muted-foreground hover:text-foreground"
              aria-label="Clear selection"
            >
              <X className="size-4" />
            </Button>
          </TooltipTrigger>
          <TooltipContent side="top">Clear selection</TooltipContent>
        </Tooltip>
      </div>
    </div>
  );
}
