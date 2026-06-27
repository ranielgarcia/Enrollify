import { useCallback, useMemo, useState } from "react";
import type {
  ClassSectionMinimal,
  CourseWithClassSections,
} from "@/api/models/class-scheduling/class-section";

/**
 * Cross-view, cross-course selection store. Lives at the page root so toggling
 * card<->table or applying a quick filter does not drop selection.
 *
 * Only Draft sections are selectable (bulk transitions only operate on draft);
 * the hook enforces that invariant on every mutation.
 */
export type UseSectionSelectionReturn = {
  selectedIds: ReadonlySet<number>;
  selectedCount: number;
  isSelected: (sectionId: number) => boolean;
  toggle: (section: Pick<ClassSectionMinimal, "id" | "status">) => void;
  toggleMany: (
    sections: ReadonlyArray<Pick<ClassSectionMinimal, "id" | "status">>,
  ) => void;
  selectAllInCourse: (course: CourseWithClassSections) => void;
  clearCourseSelection: (course: CourseWithClassSections) => void;
  clearAll: () => void;
  getCourseSelectionState: (course: CourseWithClassSections) => {
    selectableCount: number;
    selectedCount: number;
    allSelected: boolean;
    indeterminate: boolean;
  };
  getSelectedSections: (
    sections: ReadonlyArray<ClassSectionMinimal>,
  ) => ClassSectionMinimal[];
};

function isSelectable(section: Pick<ClassSectionMinimal, "status">): boolean {
  return section.status.name === "Draft";
}

export function useSectionSelection(): UseSectionSelectionReturn {
  const [selectedIds, setSelectedIds] = useState<ReadonlySet<number>>(
    () => new Set<number>(),
  );

  const isSelected = useCallback(
    (sectionId: number) => selectedIds.has(sectionId),
    [selectedIds],
  );

  const toggle = useCallback(
    (section: Pick<ClassSectionMinimal, "id" | "status">) => {
      if (!isSelectable(section)) return;
      setSelectedIds((prev) => {
        const next = new Set(prev);
        if (next.has(section.id)) next.delete(section.id);
        else next.add(section.id);
        return next;
      });
    },
    [],
  );

  const toggleMany = useCallback(
    (sections: ReadonlyArray<Pick<ClassSectionMinimal, "id" | "status">>) => {
      setSelectedIds((prev) => {
        const next = new Set(prev);
        for (const s of sections) {
          if (!isSelectable(s)) continue;
          if (next.has(s.id)) next.delete(s.id);
          else next.add(s.id);
        }
        return next;
      });
    },
    [],
  );

  const selectAllInCourse = useCallback((course: CourseWithClassSections) => {
    const draftIds = (course.classSections ?? [])
      .filter(isSelectable)
      .map((s) => s.id);
    if (draftIds.length === 0) return;
    setSelectedIds((prev) => {
      const next = new Set(prev);
      for (const id of draftIds) next.add(id);
      return next;
    });
  }, []);

  const clearCourseSelection = useCallback(
    (course: CourseWithClassSections) => {
      const ids = (course.classSections ?? []).map((s) => s.id);
      if (ids.length === 0) return;
      setSelectedIds((prev) => {
        const next = new Set(prev);
        for (const id of ids) next.delete(id);
        return next;
      });
    },
    [],
  );

  const clearAll = useCallback(() => {
    setSelectedIds((prev) => (prev.size === 0 ? prev : new Set<number>()));
  }, []);

  const getCourseSelectionState = useCallback(
    (course: CourseWithClassSections) => {
      const sections = course.classSections ?? [];
      const selectable = sections.filter(isSelectable);
      const selectedInCourse = selectable.filter((s) =>
        selectedIds.has(s.id),
      ).length;
      const allSelected =
        selectable.length > 0 && selectedInCourse === selectable.length;
      const indeterminate = selectedInCourse > 0 && !allSelected;
      return {
        selectableCount: selectable.length,
        selectedCount: selectedInCourse,
        allSelected,
        indeterminate,
      };
    },
    [selectedIds],
  );

  const getSelectedSections = useCallback(
    (sections: ReadonlyArray<ClassSectionMinimal>) =>
      sections.filter((s) => selectedIds.has(s.id)),
    [selectedIds],
  );

  return useMemo<UseSectionSelectionReturn>(
    () => ({
      selectedIds,
      selectedCount: selectedIds.size,
      isSelected,
      toggle,
      toggleMany,
      selectAllInCourse,
      clearCourseSelection,
      clearAll,
      getCourseSelectionState,
      getSelectedSections,
    }),
    [
      selectedIds,
      isSelected,
      toggle,
      toggleMany,
      selectAllInCourse,
      clearCourseSelection,
      clearAll,
      getCourseSelectionState,
      getSelectedSections,
    ],
  );
}
