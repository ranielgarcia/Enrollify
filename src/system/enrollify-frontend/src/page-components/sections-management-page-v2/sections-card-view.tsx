import { useState, useCallback, useMemo } from "react";
import { Checkbox } from "@/components/ui/checkbox";
import { Badge } from "@/components/ui/badge";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { Progress } from "@/components/ui/progress";
import {
  ClassSectionStatusEnum,
  type ClassSectionMinimal,
  type CourseWithClassSections,
} from "@/api/models/class-scheduling/class-section";
import { ChevronDown, ChevronRight } from "lucide-react";
import { cn } from "@/lib/utils";
import { SectionCard } from "./section-card";
import { BatchActionsToolbar } from "./batch-actions-toolbar";
import { EmptyState } from "./empty-state";
import type { QuickFilter } from "./searchParams";

interface SectionsCardViewProps {
  coursesWithSections: CourseWithClassSections[];
  collegeName: string;
  quickFilter: QuickFilter;
  onClearFilters: () => void;
  onOpenSection: (section: ClassSectionMinimal) => void;
  onCancelSection: (section: ClassSectionMinimal) => void;
  onViewDetails: (section: ClassSectionMinimal) => void;
  onChangeAdviser: (section: ClassSectionMinimal) => void;
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

function getCourseProgress(sections: ClassSectionMinimal[]): number {
  const allSections = sections.filter((s) => s.validationSummary?.totalOfferings);
  if (allSections.length === 0) return 0;
  let totalOfferings = 0;
  let scheduledOfferings = 0;
  for (const s of allSections) {
    const vs = s.validationSummary!;
    totalOfferings += vs.totalOfferings;
    const scheduled = Math.max(
      0,
      vs.totalOfferings -
        vs.missingTeacherCount -
        vs.missingRoomCount -
        vs.missingScheduleCount,
    );
    scheduledOfferings += scheduled;
  }
  return totalOfferings === 0
    ? 0
    : Math.round((scheduledOfferings / totalOfferings) * 100);
}

interface CourseGroupProps {
  course: CourseWithClassSections;
  filteredSections: ClassSectionMinimal[];
  isExpanded: boolean;
  onToggle: () => void;
  selectedSectionIds: Set<number>;
  onSelectionChange: (sectionId: number, selected: boolean) => void;
  onSelectAll: (courseId: number, sectionIds: number[]) => void;
  onClearSelection: (courseId: number) => void;
  onOpenSection: (section: ClassSectionMinimal) => void;
  onCancelSection: (section: ClassSectionMinimal) => void;
  onViewDetails: (section: ClassSectionMinimal) => void;
  onChangeAdviser: (section: ClassSectionMinimal) => void;
  onViewConflicts: (section: ClassSectionMinimal) => void;
  onBulkOpen: (sectionIds: number[]) => void;
  onBulkCancel: (sectionIds: number[]) => void;
  onBulkAssignAdviser: (sectionIds: number[]) => void;
  openSectionPendingId?: number | null;
  cancelSectionPendingId?: number | null;
}

function CourseGroup({
  course,
  filteredSections,
  isExpanded,
  onToggle,
  selectedSectionIds,
  onSelectionChange,
  onSelectAll,
  onClearSelection,
  onOpenSection,
  onCancelSection,
  onViewDetails,
  onChangeAdviser,
  onViewConflicts,
  onBulkOpen,
  onBulkCancel,
  onBulkAssignAdviser,
  openSectionPendingId,
  cancelSectionPendingId,
}: CourseGroupProps) {
  const allSections = course.classSections ?? [];
  const progress = getCourseProgress(allSections);
  const draftSections = filteredSections.filter(
    (s) => s.status.value === ClassSectionStatusEnum.Draft,
  );

  const allDraftIds = draftSections.map((s) => s.id);
  const allSelected =
    allDraftIds.length > 0 &&
    allDraftIds.every((id) => selectedSectionIds.has(id));
  const someSelected = allDraftIds.some((id) => selectedSectionIds.has(id));

  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      onSelectAll(course.id, allDraftIds);
    } else {
      onClearSelection(course.id);
    }
  };

  return (
    <Collapsible open={isExpanded} onOpenChange={onToggle}>
      {/* Course group header */}
      <CollapsibleTrigger asChild>
        <button
          type="button"
          className="w-full flex items-center gap-3 rounded-lg bg-muted/60 hover:bg-muted px-4 py-3 text-left transition-colors group focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          aria-label={`Toggle ${course.name} group`}
        >
          <span className="text-muted-foreground group-hover:text-foreground transition-colors">
            {isExpanded ? (
              <ChevronDown className="size-4" />
            ) : (
              <ChevronRight className="size-4" />
            )}
          </span>

          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 flex-wrap">
              <span className="font-semibold text-sm">
                {course.code} — {course.name}
              </span>
              <Badge variant="secondary" className="text-xs">
                {filteredSections.length} section
                {filteredSections.length !== 1 ? "s" : ""}
              </Badge>
            </div>
            {/* Progress bar */}
            <div className="mt-2 flex items-center gap-2">
              <Progress
                value={progress}
                className={cn(
                  "h-1.5 flex-1",
                  progress === 100
                    ? "[&>div]:bg-emerald-500"
                    : progress >= 66
                      ? "[&>div]:bg-amber-500"
                      : "[&>div]:bg-destructive",
                )}
                aria-label={`${progress}% scheduled`}
              />
              <span className="text-xs tabular-nums text-muted-foreground shrink-0">
                {progress}% scheduled
              </span>
            </div>
          </div>
        </button>
      </CollapsibleTrigger>

      <CollapsibleContent>
        <div className="pl-4 pr-1 pb-4 space-y-3 mt-2">
          {/* Batch select row */}
          {draftSections.length > 0 && (
            <div className="flex items-center gap-2 px-1">
              <Checkbox
                checked={allSelected ? true : someSelected ? "indeterminate" : false}
                onCheckedChange={handleSelectAll}
                aria-label={`Select all Draft sections in ${course.name}`}
                className="shrink-0"
              />
              <span className="text-xs text-muted-foreground">
                Select all Draft sections ({draftSections.length})
              </span>
            </div>
          )}

          {/* Batch toolbar */}
          {someSelected && (
            <BatchActionsToolbar
              selectedSectionIds={selectedSectionIds}
              sections={filteredSections}
              onClearSelection={() => onClearSelection(course.id)}
              onBulkOpen={onBulkOpen}
              onBulkCancel={onBulkCancel}
              onAssignAdviser={onBulkAssignAdviser}
            />
          )}

          {/* Section cards */}
          {filteredSections.length === 0 ? (
            <div className="text-center py-4 text-sm text-muted-foreground">
              No sections match the current filter.
            </div>
          ) : (
            <div className="grid gap-3 grid-cols-1 xl:grid-cols-2">
              {filteredSections.map((section) => (
                <SectionCard
                  key={section.id}
                  section={section}
                  courseName={`${course.code} — ${course.name}`}
                  termName="Current Term"
                  isSelected={selectedSectionIds.has(section.id)}
                  onSelectionChange={onSelectionChange}
                  onOpenSection={onOpenSection}
                  onCancelSection={onCancelSection}
                  onViewDetails={onViewDetails}
                  onChangeAdviser={onChangeAdviser}
                  onViewConflicts={onViewConflicts}
                  isOpenPending={openSectionPendingId === section.id}
                  isCancelPending={cancelSectionPendingId === section.id}
                />
              ))}
            </div>
          )}
        </div>
      </CollapsibleContent>
    </Collapsible>
  );
}

export function SectionsCardView({
  coursesWithSections,
  collegeName,
  quickFilter,
  onClearFilters,
  onOpenSection,
  onCancelSection,
  onViewDetails,
  onChangeAdviser,
  onViewConflicts,
  onBulkOpen,
  onBulkCancel,
  onBulkAssignAdviser,
  openSectionPendingId,
  cancelSectionPendingId,
}: SectionsCardViewProps) {
  // Expand all course groups by default
  const [expandedCourses, setExpandedCourses] = useState<Set<number>>(
    () => new Set(coursesWithSections.map((c) => c.id)),
  );

  // Batch selection: courseId → Set<sectionId>
  const [courseSelections, setCourseSelections] = useState<
    Map<number, Set<number>>
  >(new Map());

  const toggleCourse = useCallback((courseId: number) => {
    setExpandedCourses((prev) => {
      const next = new Set(prev);
      if (next.has(courseId)) {
        next.delete(courseId);
      } else {
        next.add(courseId);
      }
      return next;
    });
  }, []);

  const handleSelectionChange = useCallback(
    (sectionId: number, selected: boolean) => {
      setCourseSelections((prev) => {
        const next = new Map(prev);
        // Find which course this section belongs to
        for (const course of coursesWithSections) {
          const sections = course.classSections ?? [];
          if (sections.some((s) => s.id === sectionId)) {
            const courseSet = new Set(next.get(course.id) ?? []);
            if (selected) {
              courseSet.add(sectionId);
            } else {
              courseSet.delete(sectionId);
            }
            next.set(course.id, courseSet);
            break;
          }
        }
        return next;
      });
    },
    [coursesWithSections],
  );

  const handleSelectAll = useCallback(
    (courseId: number, sectionIds: number[]) => {
      setCourseSelections((prev) => {
        const next = new Map(prev);
        next.set(courseId, new Set(sectionIds));
        return next;
      });
    },
    [],
  );

  const handleClearSelection = useCallback((courseId: number) => {
    setCourseSelections((prev) => {
      const next = new Map(prev);
      next.delete(courseId);
      return next;
    });
  }, []);

  // Apply quick filter to each course's sections
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

  if (coursesWithSections.length === 0) {
    return <EmptyState variant="no-sections" />;
  }

  if (totalFilteredSections === 0) {
    return <EmptyState variant="filtered-empty" onClearFilters={onClearFilters} />;
  }

  return (
    <div className="space-y-3">
      {/* College header */}
      <div className="flex items-center gap-2 px-1">
        <h2 className="text-base font-semibold text-foreground">{collegeName}</h2>
        <Badge variant="outline" className="text-xs">
          {totalFilteredSections} section
          {totalFilteredSections !== 1 ? "s" : ""}
        </Badge>
      </div>

      {/* Course groups */}
      <div className="space-y-2">
        {filteredCourses.map(({ course, filteredSections }) => (
          <CourseGroup
            key={course.id}
            course={course}
            filteredSections={filteredSections}
            isExpanded={expandedCourses.has(course.id)}
            onToggle={() => toggleCourse(course.id)}
            selectedSectionIds={courseSelections.get(course.id) ?? new Set()}
            onSelectionChange={handleSelectionChange}
            onSelectAll={handleSelectAll}
            onClearSelection={handleClearSelection}
            onOpenSection={onOpenSection}
            onCancelSection={onCancelSection}
            onViewDetails={onViewDetails}
            onChangeAdviser={onChangeAdviser}
            onViewConflicts={onViewConflicts}
            onBulkOpen={onBulkOpen}
            onBulkCancel={onBulkCancel}
            onBulkAssignAdviser={onBulkAssignAdviser}
            openSectionPendingId={openSectionPendingId}
            cancelSectionPendingId={cancelSectionPendingId}
          />
        ))}
      </div>
    </div>
  );
}
