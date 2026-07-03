import { useMutation, useQueryClient } from "@tanstack/react-query";
import { CalendarDays, MapPin, Pencil, UserRound } from "lucide-react";

import { openClassSectionOptions } from "@/api/collections/class-section-collection";
import type { ClassSectionMinimal } from "@/api/models/class-scheduling/class-section";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Progress } from "@/components/ui/progress";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";

import { IssuesChip } from "./components/issues-chip";
import { MetaRow } from "./components/meta-row";
import { SchedulePeek } from "./components/schedule-peek";
import { InlineTransitionButtons } from "./inline-transition-buttons";
import {
  getSchedulingProgress,
  getValidationSummary,
  type SchedulingProgress,
} from "./lib/section-filters";
import { SectionStatusBadge } from "./section-status-badge";

interface SectionCardProps {
  section: ClassSectionMinimal;
  courseName: string;
  termName: string;
  isSelected: boolean;
  onSelectionChange: (sectionId: number, selected: boolean) => void;
  onCancelSection: (section: ClassSectionMinimal) => void;
  onViewDetails: (section: ClassSectionMinimal) => void;
  onChangeAdviser: (section: ClassSectionMinimal) => void;
  onViewConflicts: (section: ClassSectionMinimal) => void;
  className?: string;
}

const PROGRESS_BAR_CLASS: Record<SchedulingProgress["tone"], string> = {
  success: "[&>div]:bg-emerald-500",
  warn: "[&>div]:bg-amber-500",
  danger: "[&>div]:bg-rose-500",
  neutral: "[&>div]:bg-muted-foreground/40",
};

const PROGRESS_TEXT_CLASS: Record<SchedulingProgress["tone"], string> = {
  success: "text-emerald-600 dark:text-emerald-400",
  warn: "text-amber-600 dark:text-amber-400",
  danger: "text-rose-600 dark:text-rose-400",
  neutral: "text-muted-foreground",
};

export function SectionCard({
  section,
  courseName,
  termName,
  isSelected,
  onSelectionChange,
  onCancelSection,
  onViewDetails,
  onChangeAdviser,
  onViewConflicts,
  className,
}: SectionCardProps) {
  const isDraft = section.status.name === "Draft";
  const isCancelled = section.status.name === "Cancelled";

  const queryClient = useQueryClient();
  const openMutation = useMutation(openClassSectionOptions(section.id));

  const handleOpenSection = async () => {
    await openMutation.mutateAsync({});
    await queryClient.invalidateQueries({ queryKey: ["sections"] });
  };

  const progress = getSchedulingProgress(section);
  const validation = getValidationSummary(section);
  const adviserName = section.adviser
    ? `${section.adviser.firstName} ${section.adviser.lastName}`.trim()
    : null;

  const accentClass = validation?.hasIssues
    ? "border-l-2 border-l-amber-400 dark:border-l-amber-600"
    : "border-l-2 border-l-transparent";

  return (
    <Card
      className={cn(
        "group relative transition-shadow hover:shadow-md",
        accentClass,
        isSelected && "ring-1 ring-primary",
        isCancelled && "opacity-75",
        className,
      )}
    >
      <CardContent className="space-y-3 p-4">
        {/* Zone A — header */}
        <div className="flex items-start justify-between gap-3">
          <div className="flex min-w-0 items-start gap-3">
            {isDraft ? (
              <Checkbox
                checked={isSelected}
                onCheckedChange={(checked) =>
                  onSelectionChange(section.id, !!checked)
                }
                aria-label={`Select ${section.fullName}`}
                className="mt-1 shrink-0"
              />
            ) : (
              <Tooltip>
                <TooltipTrigger asChild>
                  <span className="inline-flex">
                    <Checkbox
                      checked={false}
                      disabled
                      aria-label="Batch operations only available for Draft sections"
                      className="mt-1 shrink-0 cursor-not-allowed opacity-30"
                    />
                  </span>
                </TooltipTrigger>
                <TooltipContent>
                  Batch operations only available for Draft sections.
                </TooltipContent>
              </Tooltip>
            )}

            <div className="min-w-0">
              <div className="flex items-baseline gap-2">
                <span className="text-2xl font-bold leading-none tabular-nums">
                  {section.sectionCode ?? section.name}
                </span>
                <span className="truncate text-sm text-muted-foreground">
                  {section.fullName}
                </span>
              </div>
              <div className="mt-1 truncate text-[11px] text-muted-foreground">
                {courseName} · {termName}
              </div>
            </div>
          </div>

          <div className="flex shrink-0 flex-col items-end gap-1.5">
            <SectionStatusBadge status={section.status} />
            <IssuesChip
              section={section}
              onClick={() => onViewConflicts(section)}
            />
          </div>
        </div>

        {/* Zone B — meta grid */}
        <div className="grid grid-cols-1 gap-x-4 gap-y-2 sm:grid-cols-2">
          <MetaRow
            icon={UserRound}
            label="Adviser"
            value={adviserName ?? "Unassigned"}
            valueClassName={cn(
              !adviserName && "italic text-muted-foreground/80",
            )}
            action={
              isDraft ? (
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() => onChangeAdviser(section)}
                  className="h-6 gap-1 px-2 text-[11px] opacity-0 transition-opacity group-hover:opacity-100 group-focus-within:opacity-100"
                >
                  <Pencil className="size-3" />
                  Change
                </Button>
              ) : null
            }
          />

          {progress.total > 0 ? (
            <MetaRow
              icon={CalendarDays}
              label="Scheduling"
              valueAs="custom"
              value={
                <div className="flex w-full items-center gap-2">
                  <span
                    className={cn(
                      "text-xs font-semibold tabular-nums",
                      PROGRESS_TEXT_CLASS[progress.tone],
                    )}
                  >
                    {progress.scheduled}/{progress.total}
                  </span>
                  <Progress
                    value={progress.pct}
                    className={cn(
                      "h-1.5 flex-1",
                      PROGRESS_BAR_CLASS[progress.tone],
                    )}
                    aria-label={`${progress.pct}% of offerings scheduled`}
                  />
                  <span
                    className={cn(
                      "text-[11px] tabular-nums",
                      PROGRESS_TEXT_CLASS[progress.tone],
                    )}
                  >
                    {progress.pct}%
                  </span>
                </div>
              }
            />
          ) : (
            <MetaRow
              icon={CalendarDays}
              label="Scheduling"
              value="No offerings yet"
              valueClassName="italic text-muted-foreground/80"
            />
          )}

          {validation && validation.totalOfferings > 0 && (
            <>
              {validation.missingTeacher > 0 && (
                <MetaRow
                  icon={UserRound}
                  label="No teacher"
                  value={`${validation.missingTeacher} offering${validation.missingTeacher === 1 ? "" : "s"}`}
                  valueClassName="text-amber-700 dark:text-amber-400"
                />
              )}
              {validation.missingSchedule > 0 && (
                <MetaRow
                  icon={CalendarDays}
                  label="No schedule"
                  value={`${validation.missingSchedule} offering${validation.missingSchedule === 1 ? "" : "s"}`}
                  valueClassName="text-amber-700 dark:text-amber-400"
                />
              )}
              {validation.missingRoom > 0 && (
                <MetaRow
                  icon={MapPin}
                  label="No room"
                  value={`${validation.missingRoom} offering${validation.missingRoom === 1 ? "" : "s"}`}
                  valueClassName="text-amber-700 dark:text-amber-400"
                />
              )}
            </>
          )}
        </div>

        {/* Zone C — footer */}
        <div className="flex items-center justify-between gap-2 pt-1">
          <InlineTransitionButtons
            section={section}
            onOpenSection={() => handleOpenSection()}
            onCancelSection={onCancelSection}
            onViewDetails={onViewDetails}
            isOpenPending={openMutation.isPending}
            size="sm"
          />
          <SchedulePeek section={section} onViewDetails={onViewDetails} />
        </div>
      </CardContent>
    </Card>
  );
}
