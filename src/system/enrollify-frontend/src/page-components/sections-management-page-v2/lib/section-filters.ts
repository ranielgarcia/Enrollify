import type {
  ClassSectionMinimal,
  CourseWithClassSections,
} from "@/api/models/class-scheduling/class-section";

/**
 * Single source of truth for the page-level "quick filter" enum.
 * Mirrors the values produced by `searchParams.ts`.
 */
export type QuickFilter =
  | "all"
  | "draft"
  | "open"
  | "cancelled"
  | "needs-attention";

/**
 * Pure quick-filter predicate. Removes the duplicate `applyQuickFilter` blocks
 * that previously lived in `sections-card-view.tsx` and `sections-table.tsx`.
 */
export function filterSectionsByQuickFilter(
  sections: readonly ClassSectionMinimal[],
  filter: QuickFilter,
): ClassSectionMinimal[] {
  if (filter === "all") return [...sections];

  return sections.filter((s) => {
    switch (filter) {
      case "draft":
        return s.status.name === "Draft";
      case "open":
        return s.status.name === "Open";
      case "cancelled":
        return s.status.name === "Cancelled";
      case "needs-attention":
        return (s.validationSummary?.OFFERING_WITH_ISSUE_COUNT ?? 0) > 0;
      default:
        return true;
    }
  });
}

/**
 * Apply quick-filter to a course's nested sections and drop courses with no
 * surviving sections. Returned shape mirrors the input.
 */
export function filterCourseGroupsByQuickFilter(
  courses: readonly CourseWithClassSections[],
  filter: QuickFilter,
): CourseWithClassSections[] {
  if (filter === "all") return [...courses];
  const out: CourseWithClassSections[] = [];
  for (const course of courses) {
    const filtered = filterSectionsByQuickFilter(
      course.classSections ?? [],
      filter,
    );
    if (filtered.length > 0) {
      out.push({ ...course, classSections: filtered });
    }
  }
  return out;
}

/**
 * Normalized scheduling-progress info for a single section.
 * `tone` is a semantic bucket the UI can map to colors.
 */
export type SchedulingProgress = {
  scheduled: number;
  total: number;
  pct: number;
  tone: "success" | "warn" | "danger" | "neutral";
};

/**
 * Treat an offering as "scheduled" iff it has no recorded issue.
 * Cleaner than subtracting overlapping counts (the old code subtracted
 * teacher+room+schedule which over-counted offerings missing multiple things).
 */
export function getSchedulingProgress(
  section: Pick<ClassSectionMinimal, "validationSummary">,
): SchedulingProgress {
  const vs = section.validationSummary;
  if (!vs || vs.OFFERINGS_COUNT === 0) {
    return { scheduled: 0, total: 0, pct: 0, tone: "neutral" };
  }
  const scheduled = Math.max(
    0,
    vs.OFFERINGS_COUNT - vs.OFFERING_WITH_ISSUE_COUNT,
  );
  const pct = Math.round((scheduled / vs.OFFERINGS_COUNT) * 100);
  const tone: SchedulingProgress["tone"] =
    pct >= 100 ? "success" : pct >= 60 ? "warn" : "danger";
  return { scheduled, total: vs.OFFERINGS_COUNT, pct, tone };
}

/**
 * Aggregate scheduling-progress across many sections (e.g. a course group).
 */
export function getAggregateSchedulingProgress(
  sections: readonly ClassSectionMinimal[],
): SchedulingProgress {
  let scheduled = 0;
  let total = 0;
  for (const s of sections) {
    const vs = s.validationSummary;
    if (!vs || vs.OFFERINGS_COUNT === 0) continue;
    total += vs.OFFERINGS_COUNT;
    scheduled += Math.max(0, vs.OFFERINGS_COUNT - vs.OFFERING_WITH_ISSUE_COUNT);
  }
  if (total === 0) return { scheduled: 0, total: 0, pct: 0, tone: "neutral" };
  const pct = Math.round((scheduled / total) * 100);
  const tone: SchedulingProgress["tone"] =
    pct >= 100 ? "success" : pct >= 60 ? "warn" : "danger";
  return { scheduled, total, pct, tone };
}

/**
 * Display-friendly view over `ValidationSummary`. Maps the raw schema keys
 * (which are uppercase snake-case for parity with the backend DTO) to the
 * concepts the UI cares about and adds the `hasIssues` rollup.
 */
export type SectionValidationView = {
  totalOfferings: number;
  withIssues: number;
  missingTeacher: number;
  missingSchedule: number;
  missingRoom: number;
  hasIssues: boolean;
  hasOfferings: boolean;
};

export function getValidationSummary(
  section: Pick<ClassSectionMinimal, "validationSummary">,
): SectionValidationView | null {
  const vs = section.validationSummary;
  if (!vs) return null;
  return {
    totalOfferings: vs.OFFERINGS_COUNT,
    withIssues: vs.OFFERING_WITH_ISSUE_COUNT,
    missingTeacher: vs.OFFERING_MISSING_TEACHER_COUNT,
    missingSchedule: vs.OFFERING_NO_SCHEDULE_COUNT,
    missingRoom: vs.OFFERING_MISSING_ROOM_COUNT,
    hasIssues: vs.OFFERING_WITH_ISSUE_COUNT > 0,
    hasOfferings: vs.OFFERINGS_COUNT > 0,
  };
}
