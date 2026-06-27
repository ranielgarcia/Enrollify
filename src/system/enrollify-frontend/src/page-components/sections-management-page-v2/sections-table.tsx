import { useMemo } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import {
  ArrowUpRight,
  CheckCircle2,
  Loader2,
  UserRound,
  XCircle,
} from "lucide-react";

import { openClassSectionOptions } from "@/api/collections/class-section-collection";
import type {
  ClassSectionMinimal,
  CourseWithClassSections,
} from "@/api/models/class-scheduling/class-section";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Progress } from "@/components/ui/progress";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";

import { EmptyState } from "./empty-state";
import { IssuesChip } from "./components/issues-chip";
import type { UseSectionSelectionReturn } from "./hooks/use-section-selection";
import {
  filterCourseGroupsByQuickFilter,
  getSchedulingProgress,
  getValidationSummary,
  type SchedulingProgress,
} from "./lib/section-filters";
import { SectionStatusBadge } from "./section-status-badge";
import type { QuickFilter } from "./searchParams";

interface SectionsTableProps {
  coursesWithSections: CourseWithClassSections[];
  quickFilter: QuickFilter;
  selection: UseSectionSelectionReturn;
  onClearFilters: () => void;
  onCancelSection: (section: ClassSectionMinimal) => void;
  onViewDetails: (section: ClassSectionMinimal) => void;
  onViewConflicts: (section: ClassSectionMinimal) => void;
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

function ProgressCell({ section }: { section: ClassSectionMinimal }) {
  const progress = getSchedulingProgress(section);
  if (progress.total === 0) {
    return <span className="text-xs text-muted-foreground">—</span>;
  }
  return (
    <div className="flex min-w-20 items-center gap-2">
      <Progress
        value={progress.pct}
        className={cn("h-1.5 w-16", PROGRESS_BAR_CLASS[progress.tone])}
        aria-label={`${progress.pct}% scheduled`}
      />
      <span
        className={cn(
          "text-xs font-medium tabular-nums",
          PROGRESS_TEXT_CLASS[progress.tone],
        )}
      >
        {progress.pct}%
      </span>
    </div>
  );
}

interface SectionTableRowProps {
  section: ClassSectionMinimal;
  selection: UseSectionSelectionReturn;
  onCancelSection: (s: ClassSectionMinimal) => void;
  onViewDetails: (s: ClassSectionMinimal) => void;
  onViewConflicts: (s: ClassSectionMinimal) => void;
}

function SectionTableRow({
  section,
  selection,
  onCancelSection,
  onViewDetails,
  onViewConflicts,
}: SectionTableRowProps) {
  const isDraft = section.status.name === "Draft";
  const isOpen = section.status.name === "Open";
  const isCancelled = section.status.name === "Cancelled";

  const queryClient = useQueryClient();
  const openMutation = useMutation(openClassSectionOptions(section.id));
  const handleOpen = async () => {
    await openMutation.mutateAsync({});
    await queryClient.invalidateQueries({ queryKey: ["sections"] });
  };

  const validation = getValidationSummary(section);
  const hasIssues = !!validation?.hasIssues;
  const isSelected = selection.isSelected(section.id);

  return (
    <TableRow
      className={cn(
        "group transition-colors",
        hasIssues && "border-l-2 border-l-amber-400 dark:border-l-amber-600",
        isCancelled && "text-muted-foreground opacity-75",
        isSelected && "bg-primary/5",
      )}
    >
      <TableCell className="w-10">
        {isDraft ? (
          <Checkbox
            checked={isSelected}
            onCheckedChange={() => selection.toggle(section)}
            aria-label={`Select ${section.fullName}`}
          />
        ) : (
          <Tooltip>
            <TooltipTrigger asChild>
              <span>
                <Checkbox
                  checked={false}
                  disabled
                  className="cursor-not-allowed opacity-30"
                />
              </span>
            </TooltipTrigger>
            <TooltipContent>
              Batch actions only apply to Draft sections
            </TooltipContent>
          </Tooltip>
        )}
      </TableCell>

      <TableCell className="font-mono text-xs font-bold">
        {section.sectionCode ?? "—"}
      </TableCell>

      <TableCell>
        <div className="min-w-0">
          <div className="truncate text-sm font-medium">
            {section.fullName ?? section.name}
          </div>
          {section.adviser ? (
            <div className="mt-0.5 flex items-center gap-1 text-xs text-muted-foreground">
              <UserRound className="size-3" />
              {section.adviser.firstName} {section.adviser.lastName}
            </div>
          ) : (
            <div className="mt-0.5 text-xs text-muted-foreground/70 italic">
              No adviser
            </div>
          )}
        </div>
      </TableCell>

      <TableCell>
        <SectionStatusBadge status={section.status} size="sm" />
      </TableCell>

      <TableCell>
        <ProgressCell section={section} />
      </TableCell>

      <TableCell>
        <IssuesChip
          validation={validation}
          onClick={
            validation?.hasIssues ? () => onViewConflicts(section) : undefined
          }
        />
      </TableCell>

      <TableCell className="text-right">
        <div className="flex items-center justify-end gap-1">
          {isDraft && (
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  variant="ghost"
                  size="sm"
                  disabled={openMutation.isPending}
                  onClick={handleOpen}
                  className="h-7 w-7 p-0 text-emerald-600 hover:bg-emerald-50 hover:text-emerald-700 dark:text-emerald-400 dark:hover:bg-emerald-950/30"
                >
                  {openMutation.isPending ? (
                    <Loader2 className="size-3.5 animate-spin" />
                  ) : (
                    <CheckCircle2 className="size-3.5" />
                  )}
                </Button>
              </TooltipTrigger>
              <TooltipContent>Open for enrollment</TooltipContent>
            </Tooltip>
          )}

          {(isDraft || isOpen) && (
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => onCancelSection(section)}
                  className="h-7 w-7 p-0 text-destructive hover:bg-destructive/10 hover:text-destructive"
                >
                  <XCircle className="size-3.5" />
                </Button>
              </TooltipTrigger>
              <TooltipContent>Cancel section</TooltipContent>
            </Tooltip>
          )}

          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onViewDetails(section)}
                className="h-7 w-7 p-0 text-muted-foreground hover:text-foreground"
              >
                <ArrowUpRight className="size-3.5" />
              </Button>
            </TooltipTrigger>
            <TooltipContent>View details</TooltipContent>
          </Tooltip>
        </div>
      </TableCell>
    </TableRow>
  );
}

export function SectionsTable({
  coursesWithSections,
  quickFilter,
  selection,
  onClearFilters,
  onCancelSection,
  onViewDetails,
  onViewConflicts,
}: SectionsTableProps) {
  const filteredCourses = useMemo(
    () => filterCourseGroupsByQuickFilter(coursesWithSections, quickFilter),
    [coursesWithSections, quickFilter],
  );

  const totalFilteredSections = filteredCourses.reduce(
    (sum, c) => sum + (c.classSections?.length ?? 0),
    0,
  );

  if (coursesWithSections.length === 0) {
    return <EmptyState variant="no-sections" />;
  }

  if (totalFilteredSections === 0) {
    return (
      <EmptyState variant="filtered-empty" onClearFilters={onClearFilters} />
    );
  }

  return (
    <Card className="overflow-hidden">
      <Table>
        <TableHeader>
          <TableRow className="bg-muted/50 hover:bg-muted/50">
            <TableHead className="w-10" />
            <TableHead className="w-14">Code</TableHead>
            <TableHead>Section</TableHead>
            <TableHead className="w-28">Status</TableHead>
            <TableHead className="w-28">Progress</TableHead>
            <TableHead className="w-28">Issues</TableHead>
            <TableHead className="w-28 text-right">Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {filteredCourses.map((course) => {
            const sections = course.classSections ?? [];
            if (sections.length === 0) return null;
            const courseState = selection.getCourseSelectionState(course);

            return [
              <TableRow
                key={`group-${course.id}`}
                className="border-t-2 border-t-border/70 bg-muted/60 hover:bg-muted/70"
              >
                <TableCell className="py-2">
                  {courseState.selectableCount > 0 && (
                    <Checkbox
                      checked={
                        courseState.allSelected
                          ? true
                          : courseState.indeterminate
                            ? "indeterminate"
                            : false
                      }
                      onCheckedChange={(checked) => {
                        if (checked === true)
                          selection.selectAllInCourse(course);
                        else selection.clearCourseSelection(course);
                      }}
                      aria-label={`Select all in ${course.name}`}
                    />
                  )}
                </TableCell>
                <TableCell colSpan={6} className="py-2">
                  <div className="flex items-center gap-2">
                    <span className="text-sm font-semibold">
                      {course.code} — {course.name}
                    </span>
                    <span className="text-xs text-muted-foreground">
                      ({sections.length} section
                      {sections.length === 1 ? "" : "s"})
                    </span>
                    {courseState.selectedCount > 0 && (
                      <span className="text-xs text-primary">
                        · {courseState.selectedCount} selected
                      </span>
                    )}
                  </div>
                </TableCell>
              </TableRow>,
              ...sections.map((section) => (
                <SectionTableRow
                  key={section.id}
                  section={section}
                  selection={selection}
                  onCancelSection={onCancelSection}
                  onViewDetails={onViewDetails}
                  onViewConflicts={onViewConflicts}
                />
              )),
            ];
          })}
        </TableBody>
      </Table>

      <div className="flex items-center justify-between border-t px-4 py-3 text-sm text-muted-foreground">
        <span>
          {totalFilteredSections} section
          {totalFilteredSections === 1 ? "" : "s"}
          {selection.selectedCount > 0 && (
            <> · {selection.selectedCount} selected</>
          )}
        </span>
        <div className="flex items-center gap-3 text-xs">
          <span className="flex items-center gap-1">
            <span className="inline-block size-2.5 rounded-full bg-emerald-500" />
            Open
          </span>
          <span className="flex items-center gap-1">
            <span className="inline-block size-2.5 rounded-full bg-secondary-foreground/30" />
            Draft
          </span>
          <span className="flex items-center gap-1">
            <span className="inline-block size-2.5 rounded-full bg-amber-400" />
            Has issues
          </span>
        </div>
      </div>
    </Card>
  );
}
