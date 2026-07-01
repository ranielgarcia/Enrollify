import {
  CalendarClock,
  LayoutGrid,
  Plus,
  TableIcon,
  TriangleAlert,
} from "lucide-react";

import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";

import { CollegeSelector } from "./college-selector";

type ContextBarView = "card" | "table";

interface AcademicTermInfo {
  termName: string;
  termNumber?: number;
  academicYearTitle?: string;
}

interface SectionsContextBarProps {
  view: ContextBarView;
  onViewChange: (view: ContextBarView) => void;
  academicTerm: AcademicTermInfo | null | undefined;
  onCreateClick?: () => void;
  createDisabled?: boolean;
  createLabel?: string;
  collegeSelectorOpen?: boolean;
  onCollegeSelectorOpenChange?: (open: boolean) => void;
  className?: string;
}

/**
 * Sticky workspace context bar that anchors the sections page:
 * - College selector (primary filter)
 * - Academic term badge (or inline alert if missing)
 * - Card / Table view toggle
 * - Optional create action
 */
export function SectionsContextBar({
  view,
  onViewChange,
  academicTerm,
  onCreateClick,
  createDisabled = true,
  createLabel = "Bulk Initialize",
  collegeSelectorOpen,
  onCollegeSelectorOpenChange,
  className,
}: SectionsContextBarProps) {
  return (
    <div
      className={cn(
        "sticky top-0 z-20 -mx-4 mb-2 border-b bg-background/95 px-4 backdrop-blur supports-backdrop-filter:bg-background/80 lg:-mx-6 lg:px-6",
        className,
      )}
    >
      <div className="flex flex-wrap items-center gap-x-3 gap-y-2 py-3">
        <div className="flex min-w-0 flex-1 flex-wrap items-center gap-2">
          <CollegeSelector
            dialogOpen={collegeSelectorOpen}
            onDialogOpenChange={onCollegeSelectorOpenChange}
          />
          {academicTerm ? (
            <Tooltip>
              <TooltipTrigger asChild>
                <Badge
                  variant="secondary"
                  className="h-9 gap-1.5 rounded-md px-3 font-medium"
                >
                  <CalendarClock className="size-3.5 text-muted-foreground" />
                  {academicTerm.termName}
                </Badge>
              </TooltipTrigger>
              <TooltipContent side="bottom" align="start">
                {academicTerm.academicYearTitle ? (
                  <>
                    {academicTerm.academicYearTitle} — {academicTerm.termName}
                  </>
                ) : (
                  academicTerm.termName
                )}
              </TooltipContent>
            </Tooltip>
          ) : (
            <Alert
              variant="destructive"
              className="flex h-9 items-center gap-2 py-0"
            >
              <TriangleAlert className="size-4" />
              <AlertDescription className="text-xs font-medium">
                Select an academic term to load sections.
              </AlertDescription>
            </Alert>
          )}
        </div>
        <div className="flex items-center gap-2">
          <ToggleGroup
            type="single"
            value={view}
            onValueChange={(v) => {
              if (v === "card" || v === "table") onViewChange(v);
            }}
            className="h-9 rounded-md border bg-muted/40 p-0.5"
            aria-label="Layout"
          >
            <ToggleGroupItem
              value="card"
              aria-label="Card view"
              className="h-8 gap-1.5 px-2.5 text-xs data-[state=on]:bg-background data-[state=on]:shadow-sm"
            >
              <LayoutGrid className="size-3.5" />
              <span className="hidden sm:inline">Cards</span>
            </ToggleGroupItem>
            <ToggleGroupItem
              value="table"
              aria-label="Table view"
              className="h-8 gap-1.5 px-2.5 text-xs data-[state=on]:bg-background data-[state=on]:shadow-sm"
            >
              <TableIcon className="size-3.5" />
              <span className="hidden sm:inline">Table</span>
            </ToggleGroupItem>
          </ToggleGroup>
          <Button
            type="button"
            size="sm"
            onClick={onCreateClick}
            disabled={createDisabled}
            className="h-9 gap-1.5"
          >
            <Plus className="size-4" />
            <span className="hidden sm:inline">{createLabel}</span>
          </Button>
        </div>
      </div>
    </div>
  );
}
