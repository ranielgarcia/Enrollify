import { useMemo, useState } from "react";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
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
import {
  ClassSectionStatusEnum,
  type ClassSectionMinimal,
  type CourseWithClassSections,
} from "@/api/models/class-scheduling/class-section";
import {
  AlertTriangle,
  ArrowUpRight,
  CheckCircle2,
  Siren,
  XCircle,
  UserRound,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { SectionStatusBadge } from "./section-status-badge";
import { BatchActionsToolbar } from "./batch-actions-toolbar";
import { EmptyState } from "./empty-state";
import type { QuickFilter } from "./searchParams";

interface SectionsTableProps {
  coursesWithSections: CourseWithClassSections[];
  quickFilter: QuickFilter;
  onClearFilters: () => void;
  onOpenSection: (section: ClassSectionMinimal) => void;
  onCancelSection: (section: ClassSectionMinimal) => void;
  onViewDetails: (section: ClassSectionMinimal) => void;
  onViewConflicts: (section: ClassSectionMinimal) => void;
  onBulkOpen: (sectionIds: number[]) => void;
  onBulkCancel: (sectionIds: number[]) => void;
  onBulkAssignAdviser: (sectionIds: number[]) => void;
  openSectionPendingId?: number | null;
  cancelSectionPendingId?: number | null;
}

function applyQuickFilter(
  sections: ClassSectionMinimal[],
  filter: QuickFilter,
): ClassSectionMinimal[] {
  switch (filter) {
    case "draft":
      return sections.filter(
        (s) => s.status.value === ClassSectionStatusEnum.Draft,
      );
    case "open":
      return sections.filter(
        (s) => s.status.value === ClassSectionStatusEnum.Open,
      );
    case "cancelled":
      return sections.filter(
        (s) => s.status.value === ClassSectionStatusEnum.Cancelled,
      );
    case "errors":
      return sections.filter(
        (s) =>
          s.status.value === ClassSectionStatusEnum.Draft &&
          (s.validationSummary?.offeringsWithErrors ?? 0) > 0,
      );
    case "conflicts":
      return sections.filter(
        (s) =>
          s.status.value === ClassSectionStatusEnum.Draft &&
          (s.validationSummary?.offeringsWithConflicts ?? 0) > 0,
      );
    case "needs-attention":
      return sections.filter(
        (s) =>
          s.status.value === ClassSectionStatusEnum.Draft &&
          ((s.validationSummary?.offeringsWithErrors ?? 0) > 0 ||
            (s.validationSummary?.offeringsWithConflicts ?? 0) > 0),
      );
    default:
      return sections;
  }
}

function ProgressCell({
  section,
}: {
  section: ClassSectionMinimal;
}) {
  const vs = section.validationSummary;
  if (!vs || vs.totalOfferings === 0) {
    return (
      <span className="text-xs text-muted-foreground">—</span>
    );
  }
  const scheduled = Math.max(
    0,
    vs.totalOfferings - vs.missingTeacherCount - vs.missingRoomCount - vs.missingScheduleCount,
  );
  const pct = Math.round((scheduled / vs.totalOfferings) * 100);

  return (
    <div className="flex items-center gap-2 min-w-[80px]">
      <Progress
        value={pct}
        className={cn(
          "h-1.5 w-16",
          pct === 100
            ? "[&>div]:bg-emerald-500"
            : pct >= 66
              ? "[&>div]:bg-amber-500"
              : "[&>div]:bg-destructive",
        )}
        aria-label={`${pct}% scheduled`}
      />
      <span
        className={cn(
          "text-xs tabular-nums font-medium",
          pct === 100
            ? "text-emerald-600 dark:text-emerald-400"
            : pct >= 66
              ? "text-amber-600 dark:text-amber-400"
              : "text-destructive",
        )}
      >
        {pct}%
      </span>
    </div>
  );
}

function ErrorConflictCell({
  section,
  onViewConflicts,
}: {
  section: ClassSectionMinimal;
  onViewConflicts: (section: ClassSectionMinimal) => void;
}) {
  const vs = section.validationSummary;
  const errors = vs?.offeringsWithErrors ?? 0;
  const conflicts = vs?.offeringsWithConflicts ?? 0;

  if (errors === 0 && conflicts === 0) {
    return (
      <span className="flex items-center gap-1 text-xs text-emerald-600 dark:text-emerald-400">
        <CheckCircle2 className="size-3.5" />
        Clean
      </span>
    );
  }

  const hasConflicts = conflicts > 0;
  return (
    <button
      type="button"
      onClick={() => onViewConflicts(section)}
      className={cn(
        "inline-flex items-center gap-1 rounded-full px-1.5 py-0.5 text-xs font-medium border cursor-pointer transition-colors",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
        hasConflicts
          ? "bg-rose-50 text-rose-700 border-rose-200 hover:bg-rose-100 dark:bg-rose-950/30 dark:text-rose-400 dark:border-rose-800"
          : "bg-amber-50 text-amber-700 border-amber-200 hover:bg-amber-100 dark:bg-amber-950/30 dark:text-amber-400 dark:border-amber-800",
      )}
    >
      {errors > 0 && (
        <span className="flex items-center gap-0.5">
          <AlertTriangle className="size-3" />
          {errors}
        </span>
      )}
      {errors > 0 && conflicts > 0 && <span className="opacity-40">/</span>}
      {conflicts > 0 && (
        <span className="flex items-center gap-0.5">
          <Siren className="size-3" />
          {conflicts}
        </span>
      )}
    </button>
  );
}

function SectionTableRow({
  section,
  isSelected,
  onSelectionChange,
  onOpenSection,
  onCancelSection,
  onViewDetails,
  onViewConflicts,
  isOpenPending,
  isCancelPending,
}: {
  section: ClassSectionMinimal;
  isSelected: boolean;
  onSelectionChange: (id: number, selected: boolean) => void;
  onOpenSection: (s: ClassSectionMinimal) => void;
  onCancelSection: (s: ClassSectionMinimal) => void;
  onViewDetails: (s: ClassSectionMinimal) => void;
  onViewConflicts: (s: ClassSectionMinimal) => void;
  isOpenPending: boolean;
  isCancelPending: boolean;
}) {
  const isDraft = section.status.value === ClassSectionStatusEnum.Draft;
  const isOpen = section.status.value === ClassSectionStatusEnum.Open;
  const hasIssues =
    (section.validationSummary?.offeringsWithErrors ?? 0) > 0 ||
    (section.validationSummary?.offeringsWithConflicts ?? 0) > 0;

  return (
    <TableRow
      className={cn(
        "group transition-colors",
        hasIssues && "border-l-2 border-l-amber-400 dark:border-l-amber-600",
        !isDraft && !isOpen && "bg-muted/30 text-muted-foreground",
        isSelected && "bg-primary/5",
      )}
    >
      {/* Checkbox */}
      <TableCell className="w-10">
        {isDraft ? (
          <Checkbox
            checked={isSelected}
            onCheckedChange={(checked) =>
              onSelectionChange(section.id, !!checked)
            }
            aria-label={`Select ${section.fullName}`}
          />
        ) : (
          <Tooltip>
            <TooltipTrigger asChild>
              <span>
                <Checkbox
                  checked={false}
                  disabled
                  className="opacity-30 cursor-not-allowed"
                />
              </span>
            </TooltipTrigger>
            <TooltipContent>
              Batch operations only available for Draft sections
            </TooltipContent>
          </Tooltip>
        )}
      </TableCell>

      {/* Code */}
      <TableCell className="font-mono text-xs font-bold">
        {section.sectionCode ?? "—"}
      </TableCell>

      {/* Section name */}
      <TableCell>
        <div>
          <div className="font-medium text-sm">{section.fullName}</div>
          {section.adviser && (
            <div className="text-xs text-muted-foreground mt-0.5 flex items-center gap-1">
              <UserRound className="size-3" />
              {section.adviser.firstName} {section.adviser.lastName}
            </div>
          )}
        </div>
      </TableCell>

      {/* Status */}
      <TableCell>
        <SectionStatusBadge status={section.status} size="sm" />
      </TableCell>

      {/* Progress */}
      <TableCell>
        <ProgressCell section={section} />
      </TableCell>

      {/* Errors / Conflicts */}
      <TableCell>
        <ErrorConflictCell section={section} onViewConflicts={onViewConflicts} />
      </TableCell>

      {/* Actions */}
      <TableCell className="text-right">
        <div className="flex items-center justify-end gap-1">
          {isDraft && (
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  variant="ghost"
                  size="sm"
                  disabled={isOpenPending}
                  onClick={() => onOpenSection(section)}
                  className="h-7 w-7 p-0 text-emerald-600 hover:text-emerald-700 hover:bg-emerald-50 dark:text-emerald-400 dark:hover:bg-emerald-950/30"
                >
                  <CheckCircle2 className="size-3.5" />
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
                  disabled={isCancelPending}
                  onClick={() => onCancelSection(section)}
                  className="h-7 w-7 p-0 text-destructive hover:text-destructive hover:bg-destructive/10"
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
  onClearFilters,
  onOpenSection,
  onCancelSection,
  onViewDetails,
  onViewConflicts,
  onBulkOpen,
  onBulkCancel,
  onBulkAssignAdviser,
  openSectionPendingId,
  cancelSectionPendingId,
}: SectionsTableProps) {
  // Per-course selection state
  const [courseSelections, setCourseSelections] = useState<
    Map<number, Set<number>>
  >(new Map());

  const filteredCourses = useMemo(() => {
    return coursesWithSections.map((course) => ({
      course,
      filteredSections: applyQuickFilter(
        course.classSections ?? [],
        quickFilter,
      ),
    }));
  }, [coursesWithSections, quickFilter]);

  const totalFilteredSections = filteredCourses.reduce(
    (sum, { filteredSections }) => sum + filteredSections.length,
    0,
  );

  const handleSelectionChange = (
    courseId: number,
    sectionId: number,
    selected: boolean,
  ) => {
    setCourseSelections((prev) => {
      const next = new Map(prev);
      const courseSet = new Set(next.get(courseId) ?? []);
      if (selected) courseSet.add(sectionId);
      else courseSet.delete(sectionId);
      next.set(courseId, courseSet);
      return next;
    });
  };

  const handleSelectAllInCourse = (
    courseId: number,
    draftIds: number[],
    checked: boolean,
  ) => {
    setCourseSelections((prev) => {
      const next = new Map(prev);
      if (checked) next.set(courseId, new Set(draftIds));
      else next.delete(courseId);
      return next;
    });
  };

  const handleClearCourseSelection = (courseId: number) => {
    setCourseSelections((prev) => {
      const next = new Map(prev);
      next.delete(courseId);
      return next;
    });
  };

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
            <TableHead className="w-24">Issues</TableHead>
            <TableHead className="w-28 text-right">Actions</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {filteredCourses.map(({ course, filteredSections }) => {
            if (filteredSections.length === 0) return null;

            const courseSelectionSet =
              courseSelections.get(course.id) ?? new Set<number>();
            const draftSections = filteredSections.filter(
              (s) => s.status.value === ClassSectionStatusEnum.Draft,
            );
            const draftIds = draftSections.map((s) => s.id);
            const allSelected =
              draftIds.length > 0 &&
              draftIds.every((id) => courseSelectionSet.has(id));
            const someSelected = draftIds.some((id) =>
              courseSelectionSet.has(id),
            );

            return [
              // Course group header row
              <TableRow
                key={`group-${course.id}`}
                className="bg-muted/60 hover:bg-muted/70 border-t-2 border-t-border/70"
              >
                <TableCell className="py-2">
                  {draftIds.length > 0 && (
                    <Checkbox
                      checked={
                        allSelected
                          ? true
                          : someSelected
                            ? "indeterminate"
                            : false
                      }
                      onCheckedChange={(checked) =>
                        handleSelectAllInCourse(
                          course.id,
                          draftIds,
                          !!checked,
                        )
                      }
                      aria-label={`Select all in ${course.name}`}
                    />
                  )}
                </TableCell>
                <TableCell colSpan={6} className="py-2">
                  <div className="flex items-center justify-between gap-3">
                    <div className="flex items-center gap-2">
                      <span className="font-semibold text-sm">
                        {course.code} — {course.name}
                      </span>
                      <span className="text-xs text-muted-foreground">
                        ({filteredSections.length} section
                        {filteredSections.length !== 1 ? "s" : ""})
                      </span>
                    </div>
                    {/* Course-level batch toolbar */}
                    {someSelected && (
                      <BatchActionsToolbar
                        selectedSectionIds={courseSelectionSet}
                        sections={filteredSections}
                        onClearSelection={() =>
                          handleClearCourseSelection(course.id)
                        }
                        onBulkOpen={onBulkOpen}
                        onBulkCancel={onBulkCancel}
                        onAssignAdviser={onBulkAssignAdviser}
                        className="py-1 px-2"
                      />
                    )}
                  </div>
                </TableCell>
              </TableRow>,

              // Section rows
              ...filteredSections.map((section) => (
                <SectionTableRow
                  key={section.id}
                  section={section}
                  isSelected={courseSelectionSet.has(section.id)}
                  onSelectionChange={(id, selected) =>
                    handleSelectionChange(course.id, id, selected)
                  }
                  onOpenSection={onOpenSection}
                  onCancelSection={onCancelSection}
                  onViewDetails={onViewDetails}
                  onViewConflicts={onViewConflicts}
                  isOpenPending={openSectionPendingId === section.id}
                  isCancelPending={cancelSectionPendingId === section.id}
                />
              )),
            ];
          })}
        </TableBody>
      </Table>

      {/* Footer */}
      <div className="flex items-center justify-between border-t px-4 py-3 text-sm text-muted-foreground">
        <span>
          {totalFilteredSections} section
          {totalFilteredSections !== 1 ? "s" : ""}
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
            Has Issues
          </span>
        </div>
      </div>
    </Card>
  );
}
