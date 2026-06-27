import { useState, useRef } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { Progress } from "@/components/ui/progress";
import {
  ClassSectionStatusEnum,
  type ClassSectionMinimal,
} from "@/api/models/class-scheduling/class-section";
import {
  AlertTriangle,
  BookUser,
  CalendarDays,
  CheckCircle2,
  Info,
  Siren,
  UserRound,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { SectionStatusBadge } from "./section-status-badge";
import { InlineTransitionButtons } from "./inline-transition-buttons";
import { SectionCardMiniSchedule } from "./section-card-mini-schedule";

interface SectionCardProps {
  section: ClassSectionMinimal;
  courseName: string;
  termName: string;
  isSelected: boolean;
  onSelectionChange: (sectionId: number, selected: boolean) => void;
  onOpenSection: (section: ClassSectionMinimal) => void;
  onCancelSection: (section: ClassSectionMinimal) => void;
  onViewDetails: (section: ClassSectionMinimal) => void;
  onChangeAdviser: (section: ClassSectionMinimal) => void;
  onViewConflicts: (section: ClassSectionMinimal) => void;
  isOpenPending?: boolean;
  isCancelPending?: boolean;
  className?: string;
}

function getSchedulingProgress(section: ClassSectionMinimal): {
  scheduled: number;
  total: number;
  pct: number;
} {
  const vs = section.validationSummary;
  if (!vs || vs.totalOfferings === 0) return { scheduled: 0, total: 0, pct: 0 };
  const scheduled =
    vs.totalOfferings -
    vs.missingTeacherCount -
    vs.missingRoomCount -
    vs.missingScheduleCount;
  const safeScheduled = Math.max(0, Math.min(scheduled, vs.totalOfferings));
  return {
    scheduled: safeScheduled,
    total: vs.totalOfferings,
    pct: Math.round((safeScheduled / vs.totalOfferings) * 100),
  };
}

function getIssuesSummary(section: ClassSectionMinimal): string[] {
  const vs = section.validationSummary;
  if (!vs) return [];
  const issues: string[] = [];
  if (vs.missingTeacherCount > 0)
    issues.push(
      `${vs.missingTeacherCount} offering${vs.missingTeacherCount !== 1 ? "s" : ""} missing teacher`,
    );
  if (vs.missingRoomCount > 0)
    issues.push(
      `${vs.missingRoomCount} offering${vs.missingRoomCount !== 1 ? "s" : ""} missing room`,
    );
  if (vs.missingScheduleCount > 0)
    issues.push(
      `${vs.missingScheduleCount} offering${vs.missingScheduleCount !== 1 ? "s" : ""} missing schedule`,
    );
  if (vs.offeringsWithConflicts > 0)
    issues.push(
      `${vs.offeringsWithConflicts} conflict${vs.offeringsWithConflicts !== 1 ? "s" : ""} detected`,
    );
  return issues;
}

function ErrorConflictBadge({
  section,
  onViewConflicts,
}: {
  section: ClassSectionMinimal;
  onViewConflicts: (section: ClassSectionMinimal) => void;
}) {
  const vs = section.validationSummary;
  if (!vs) return null;

  const errors = vs.offeringsWithErrors;
  const conflicts = vs.offeringsWithConflicts;

  if (errors === 0 && conflicts === 0) {
    return (
      <Badge
        variant="outline"
        className="gap-1 text-xs text-emerald-600 border-emerald-200 dark:text-emerald-400 dark:border-emerald-800"
      >
        <CheckCircle2 className="size-3" />
        No Issues
      </Badge>
    );
  }

  const hasConflicts = conflicts > 0;
  return (
    <button
      type="button"
      onClick={() => onViewConflicts(section)}
      className={cn(
        "inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium border transition-colors cursor-pointer",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
        hasConflicts
          ? "bg-rose-50 text-rose-700 border-rose-200 hover:bg-rose-100 dark:bg-rose-950/30 dark:text-rose-400 dark:border-rose-800"
          : "bg-amber-50 text-amber-700 border-amber-200 hover:bg-amber-100 dark:bg-amber-950/30 dark:text-amber-400 dark:border-amber-800",
      )}
      title="View errors and conflicts"
    >
      {errors > 0 && (
        <span className="flex items-center gap-0.5">
          <AlertTriangle className="size-3" />
          {errors}
        </span>
      )}
      {errors > 0 && conflicts > 0 && <span className="opacity-50">/</span>}
      {conflicts > 0 && (
        <span className="flex items-center gap-0.5">
          <Siren className="size-3" />
          {conflicts}
        </span>
      )}
    </button>
  );
}

export function SectionCard({
  section,
  courseName,
  termName,
  isSelected,
  onSelectionChange,
  onOpenSection,
  onCancelSection,
  onViewDetails,
  onChangeAdviser,
  onViewConflicts,
  isOpenPending = false,
  isCancelPending = false,
  className,
}: SectionCardProps) {
  const isDraft = section.status.value === ClassSectionStatusEnum.Draft;
  const hasIssues =
    (section.validationSummary?.offeringsWithErrors ?? 0) > 0 ||
    (section.validationSummary?.offeringsWithConflicts ?? 0) > 0;

  const { scheduled, total, pct } = getSchedulingProgress(section);
  const issuesList = getIssuesSummary(section);

  // Mini schedule hover
  const [isHovered, setIsHovered] = useState(false);
  const [isSchedulePopoverOpen, setIsSchedulePopoverOpen] = useState(false);
  const hoverTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const handleMouseEnter = () => {
    hoverTimerRef.current = setTimeout(() => {
      setIsHovered(true);
      setIsSchedulePopoverOpen(true);
    }, 400);
  };

  const handleMouseLeave = () => {
    if (hoverTimerRef.current) {
      clearTimeout(hoverTimerRef.current);
    }
    setIsSchedulePopoverOpen(false);
    // keep isHovered true to avoid re-fetching on re-hover
  };

  return (
    <Card
      className={cn(
        "relative transition-all duration-150",
        "hover:shadow-md",
        hasIssues &&
          "border-l-4 border-l-amber-400 dark:border-l-amber-600",
        !hasIssues && "border-l-4 border-l-transparent",
        isDraft ? "bg-card" : "bg-muted/20",
        isSelected && "ring-2 ring-primary/40",
        className,
      )}
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
    >
      <CardContent className="p-4">
        {/* Header row */}
        <div className="flex items-start justify-between gap-3 mb-2.5">
          <div className="flex items-center gap-2.5 min-w-0">
            {/* Checkbox */}
            {isDraft ? (
              <Checkbox
                checked={isSelected}
                onCheckedChange={(checked) =>
                  onSelectionChange(section.id, !!checked)
                }
                aria-label={`Select ${section.fullName}`}
                className="mt-0.5 shrink-0"
              />
            ) : (
              <Tooltip>
                <TooltipTrigger asChild>
                  <span>
                    <Checkbox
                      checked={false}
                      disabled
                      className="mt-0.5 shrink-0 opacity-40 cursor-not-allowed"
                      aria-label="Batch operations not available for this status"
                    />
                  </span>
                </TooltipTrigger>
                <TooltipContent>
                  Batch operations only available for Draft sections
                </TooltipContent>
              </Tooltip>
            )}

            <div className="min-w-0">
              <div className="flex items-center gap-2 flex-wrap">
                <span className="font-bold text-base leading-none">
                  {section.sectionCode ?? section.name}
                </span>
                <span className="text-sm text-muted-foreground font-normal">
                  {section.fullName}
                </span>
              </div>
              <div className="text-xs text-muted-foreground mt-1">
                {courseName} · {termName}
              </div>
            </div>
          </div>

          {/* Status + Error badge */}
          <div className="flex items-center gap-2 shrink-0">
            <ErrorConflictBadge
              section={section}
              onViewConflicts={onViewConflicts}
            />
            <SectionStatusBadge status={section.status} />
          </div>
        </div>

        {/* Divider */}
        <div className="border-t my-3" />

        {/* Details rows */}
        <div className="space-y-2">
          {/* Adviser row */}
          <div className="flex items-center gap-2 text-sm">
            <UserRound className="size-3.5 text-muted-foreground shrink-0" />
            <span className="text-muted-foreground text-xs">Adviser:</span>
            {section.adviser ? (
              <span className="font-medium text-xs">
                {section.adviser.firstName} {section.adviser.lastName}
              </span>
            ) : (
              <span className="text-xs italic text-muted-foreground">
                Unassigned
              </span>
            )}
            {isDraft && (
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onChangeAdviser(section)}
                className="h-5 px-1.5 text-xs ml-auto text-muted-foreground hover:text-foreground"
              >
                Change
              </Button>
            )}
          </div>

          {/* Scheduling progress */}
          {total > 0 && (
            <div className="space-y-1">
              <div className="flex items-center justify-between text-xs">
                <span className="flex items-center gap-1.5 text-muted-foreground">
                  <CalendarDays className="size-3.5" />
                  Scheduling:
                </span>
                <span
                  className={cn(
                    "font-medium tabular-nums",
                    pct === 100
                      ? "text-emerald-600 dark:text-emerald-400"
                      : pct >= 66
                        ? "text-amber-600 dark:text-amber-400"
                        : "text-destructive",
                  )}
                >
                  {scheduled}/{total} offerings ({pct}%)
                </span>
              </div>
              <Progress
                value={pct}
                className={cn(
                  "h-1.5",
                  pct === 100
                    ? "[&>div]:bg-emerald-500"
                    : pct >= 66
                      ? "[&>div]:bg-amber-500"
                      : "[&>div]:bg-destructive",
                )}
                aria-label={`${pct}% of offerings scheduled`}
              />
            </div>
          )}

          {/* Issues row — only for Draft with issues */}
          {isDraft && issuesList.length > 0 && (
            <div className="flex items-start gap-1.5 text-xs">
              <Info className="size-3.5 text-amber-500 mt-0.5 shrink-0" />
              <span className="text-amber-700 dark:text-amber-400 leading-relaxed">
                {issuesList.join(" · ")}
              </span>
            </div>
          )}
        </div>

        {/* Divider */}
        <div className="border-t my-3" />

        {/* Action footer */}
        <div className="flex items-center justify-between gap-2">
          <InlineTransitionButtons
            section={section}
            onOpenSection={onOpenSection}
            onCancelSection={onCancelSection}
            onViewDetails={onViewDetails}
            isOpenPending={isOpenPending}
            isCancelPending={isCancelPending}
            size="sm"
          />

          {/* Mini schedule hover hint */}
          <Popover
            open={isSchedulePopoverOpen}
            onOpenChange={setIsSchedulePopoverOpen}
          >
            <PopoverTrigger asChild>
              <button
                type="button"
                className="flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground transition-colors cursor-default focus:outline-none"
                aria-label="Hover to see weekly schedule"
              >
                <BookUser className="size-3.5" />
                <span className="hidden sm:inline">Schedule</span>
              </button>
            </PopoverTrigger>
            <PopoverContent
              side="top"
              align="end"
              className="p-3 w-auto"
              onOpenAutoFocus={(e) => e.preventDefault()}
            >
              <SectionCardMiniSchedule
                section={section}
                isHovered={isHovered}
                onViewDetails={() => onViewDetails(section)}
              />
            </PopoverContent>
          </Popover>
        </div>
      </CardContent>
    </Card>
  );
}
