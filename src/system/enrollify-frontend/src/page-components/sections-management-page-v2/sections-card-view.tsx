import { useCallback, useMemo, useState } from "react";
import { ChevronDown, ChevronRight } from "lucide-react";

import type {
  ClassSectionMinimal,
  CourseWithClassSections,
} from "@/api/models/class-scheduling/class-section";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { Progress } from "@/components/ui/progress";
import { cn } from "@/lib/utils";

import { EmptyState } from "./empty-state";
import type { UseSectionSelectionReturn } from "./hooks/use-section-selection";
import {
  filterCourseGroupsByQuickFilter,
  getAggregateSchedulingProgress,
  type SchedulingProgress,
} from "./lib/section-filters";
import { SectionCard } from "./section-card";
import type { QuickFilter } from "./searchParams";

interface SectionsCardViewProps {
  coursesWithSections: CourseWithClassSections[];
  collegeName: string;
  quickFilter: QuickFilter;
  selection: UseSectionSelectionReturn;
  onClearFilters: () => void;
  onCancelSection: (section: ClassSectionMinimal) => void;
  onViewDetails: (section: ClassSectionMinimal) => void;
  onEditDetails: (section: ClassSectionMinimal) => void;
  onChangeAdviser: (section: ClassSectionMinimal) => void;
  onViewConflicts: (section: ClassSectionMinimal) => void;
}

const PROGRESS_BAR_CLASS: Record<SchedulingProgress["tone"], string> = {
  success: "[&>div]:bg-emerald-500",
  warn: "[&>div]:bg-amber-500",
  danger: "[&>div]:bg-rose-500",
  neutral: "[&>div]:bg-muted-foreground/40",
};

interface CourseGroupProps {
  course: CourseWithClassSections;
  courseName: string;
  isExpanded: boolean;
  onToggle: () => void;
  selection: UseSectionSelectionReturn;
  onCancelSection: (section: ClassSectionMinimal) => void;
  onViewDetails: (section: ClassSectionMinimal) => void;
  onChangeAdviser: (section: ClassSectionMinimal) => void;
  onViewConflicts: (section: ClassSectionMinimal) => void;
  onEditDetails: (section: ClassSectionMinimal) => void;
}

function CourseGroup({
  course,
  courseName,
  isExpanded,
  onToggle,
  selection,
  onCancelSection,
  onViewDetails,
  onChangeAdviser,
  onViewConflicts,
  onEditDetails,
}: CourseGroupProps) {
  const sections = course.classSections ?? [];
  const progress = getAggregateSchedulingProgress(sections);
  const courseState = selection.getCourseSelectionState(course);

  const handleSelectAll = (checked: boolean | "indeterminate") => {
    if (checked === true) selection.selectAllInCourse(course);
    else selection.clearCourseSelection(course);
  };

  return (
    <Collapsible open={isExpanded} onOpenChange={onToggle}>
      <div className="rounded-lg border bg-card">
        <CollapsibleTrigger asChild>
          <button
            type="button"
            className="group flex w-full items-center gap-3 rounded-t-lg px-4 py-3 text-left transition-colors hover:bg-muted/60 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring data-[state=closed]:rounded-b-lg"
            aria-label={`Toggle ${courseName} group`}
            data-state={isExpanded ? "open" : "closed"}
          >
            <span className="text-muted-foreground transition-colors group-hover:text-foreground">
              {isExpanded ? (
                <ChevronDown className="size-4" />
              ) : (
                <ChevronRight className="size-4" />
              )}
            </span>

            <div className="min-w-0 flex-1">
              <div className="flex flex-wrap items-center gap-2">
                <span className="truncate text-sm font-semibold">
                  {courseName}
                </span>
                <Badge variant="secondary" className="text-xs">
                  {sections.length} section{sections.length === 1 ? "" : "s"}
                </Badge>
                {courseState.selectedCount > 0 && (
                  <Badge
                    variant="outline"
                    className="border-primary/30 bg-primary/5 text-xs text-primary"
                  >
                    {courseState.selectedCount} selected
                  </Badge>
                )}
              </div>
              {progress.total > 0 && (
                <div className="mt-2 flex items-center gap-2">
                  <Progress
                    value={progress.pct}
                    className={cn(
                      "h-1.5 flex-1",
                      PROGRESS_BAR_CLASS[progress.tone],
                    )}
                    aria-label={`${progress.pct}% scheduled`}
                  />
                  <span className="shrink-0 text-xs tabular-nums text-muted-foreground">
                    {progress.scheduled}/{progress.total} scheduled ·{" "}
                    {progress.pct}%
                  </span>
                </div>
              )}
            </div>
          </button>
        </CollapsibleTrigger>

        <CollapsibleContent>
          <div className="space-y-3 border-t px-4 py-3">
            {courseState.selectableCount > 0 && (
              <div className="flex items-center gap-2">
                <Checkbox
                  checked={
                    courseState.allSelected
                      ? true
                      : courseState.indeterminate
                        ? "indeterminate"
                        : false
                  }
                  onCheckedChange={handleSelectAll}
                  aria-label={`Select all Draft sections in ${courseName}`}
                  className="shrink-0"
                />
                <span className="text-xs text-muted-foreground">
                  Select all Draft sections ({courseState.selectableCount})
                </span>
              </div>
            )}

            {sections.length === 0 ? (
              <div className="py-4 text-center text-sm text-muted-foreground">
                No sections match the current filter.
              </div>
            ) : (
              <div className="grid grid-cols-1 gap-3 md:grid-cols-2 2xl:grid-cols-3">
                {sections.map((section) => (
                  <SectionCard
                    key={section.id}
                    section={section}
                    courseName={courseName}
                    termName="Current Term"
                    isSelected={selection.isSelected(section.id)}
                    onSelectionChange={() => selection.toggle(section)}
                    onCancelSection={onCancelSection}
                    onViewDetails={onViewDetails}
                    onChangeAdviser={onChangeAdviser}
                    onViewConflicts={onViewConflicts}
                    onEdit={onEditDetails}
                  />
                ))}
              </div>
            )}
          </div>
        </CollapsibleContent>
      </div>
    </Collapsible>
  );
}

export function SectionsCardView({
  coursesWithSections,
  collegeName,
  quickFilter,
  selection,
  onClearFilters,
  onCancelSection,
  onViewDetails,
  onChangeAdviser,
  onViewConflicts,
  onEditDetails,
}: SectionsCardViewProps) {
  const filteredCourses = useMemo(
    () => filterCourseGroupsByQuickFilter(coursesWithSections, quickFilter),
    [coursesWithSections, quickFilter],
  );

  const [expandedCourses, setExpandedCourses] = useState<Set<number>>(
    () => new Set(coursesWithSections.map((c) => c.id)),
  );

  const toggleCourse = useCallback((courseId: number) => {
    setExpandedCourses((prev) => {
      const next = new Set(prev);
      if (next.has(courseId)) next.delete(courseId);
      else next.add(courseId);
      return next;
    });
  }, []);

  if (coursesWithSections.length === 0) {
    return <EmptyState variant="no-sections" />;
  }

  if (filteredCourses.length === 0) {
    return (
      <EmptyState variant="filtered-empty" onClearFilters={onClearFilters} />
    );
  }

  const totalFilteredSections = filteredCourses.reduce(
    (sum, c) => sum + (c.classSections?.length ?? 0),
    0,
  );

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-2 px-1">
        <h2 className="text-base font-semibold text-foreground">
          {collegeName}
        </h2>
        <Badge variant="outline" className="text-xs">
          {totalFilteredSections} section
          {totalFilteredSections === 1 ? "" : "s"}
        </Badge>
        {selection.selectedCount > 0 && (
          <Badge variant="outline" className="text-xs">
            {selection.selectedCount} selected
          </Badge>
        )}
      </div>

      <div className="space-y-2">
        {filteredCourses.map((course) => (
          <CourseGroup
            key={course.id}
            course={course}
            courseName={`${course.code} — ${course.name}`}
            isExpanded={expandedCourses.has(course.id)}
            onToggle={() => toggleCourse(course.id)}
            selection={selection}
            onCancelSection={onCancelSection}
            onViewDetails={onViewDetails}
            onChangeAdviser={onChangeAdviser}
            onViewConflicts={onViewConflicts}
            onEditDetails={onEditDetails}
          />
        ))}
      </div>
    </div>
  );
}
