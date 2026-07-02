import { useCallback, useReducer } from "react";
import type { ClassSectionMinimal } from "@/api/models/class-scheduling/class-section";

/**
 * Discriminated union covering every modal/drawer the sections page can open.
 * Only one is ever open at a time, so a single reducer is sufficient and
 * removes the 5+ useState calls the legacy index.tsx accumulated.
 */
export type SectionsDialogState =
  | { type: "closed" }
  | { type: "edit-adviser"; section: ClassSectionMinimal }
  | { type: "cancel-single"; section: ClassSectionMinimal }
  | { type: "bulk-open"; sectionIds: readonly number[] }
  | { type: "bulk-cancel"; sectionIds: readonly number[] }
  | { type: "bulk-assign-adviser"; sectionIds: readonly number[] }
  | { type: "view-conflicts"; sectionId: number; sectionName: string }
  | { type: "view-section-details"; sectionId: number };

type Action =
  | { kind: "open"; state: Exclude<SectionsDialogState, { type: "closed" }> }
  | { kind: "close" };

function reducer(
  state: SectionsDialogState,
  action: Action,
): SectionsDialogState {
  switch (action.kind) {
    case "open":
      return action.state;
    case "close":
      return { type: "closed" };
    default:
      return state;
  }
}

export type UseSectionsDialogsReturn = {
  state: SectionsDialogState;
  close: () => void;
  openEditAdviser: (section: ClassSectionMinimal) => void;
  openCancelSingle: (section: ClassSectionMinimal) => void;
  openBulkOpen: (sectionIds: readonly number[]) => void;
  openBulkCancel: (sectionIds: readonly number[]) => void;
  openBulkAssignAdviser: (sectionIds: readonly number[]) => void;
  openViewConflicts: (sectionId: number, sectionName: string) => void;
  openViewSectionDetails: (sectionId: number) => void;
};

export function useSectionsDialogs(): UseSectionsDialogsReturn {
  const [state, dispatch] = useReducer(reducer, { type: "closed" });

  const close = useCallback(() => dispatch({ kind: "close" }), []);

  const openEditAdviser = useCallback(
    (section: ClassSectionMinimal) =>
      dispatch({ kind: "open", state: { type: "edit-adviser", section } }),
    [],
  );
  const openCancelSingle = useCallback(
    (section: ClassSectionMinimal) =>
      dispatch({ kind: "open", state: { type: "cancel-single", section } }),
    [],
  );
  const openBulkOpen = useCallback(
    (sectionIds: readonly number[]) =>
      dispatch({ kind: "open", state: { type: "bulk-open", sectionIds } }),
    [],
  );
  const openBulkCancel = useCallback(
    (sectionIds: readonly number[]) =>
      dispatch({ kind: "open", state: { type: "bulk-cancel", sectionIds } }),
    [],
  );
  const openBulkAssignAdviser = useCallback(
    (sectionIds: readonly number[]) =>
      dispatch({
        kind: "open",
        state: { type: "bulk-assign-adviser", sectionIds },
      }),
    [],
  );
  const openViewConflicts = useCallback(
    (sectionId: number, sectionName: string) =>
      dispatch({
        kind: "open",
        state: { type: "view-conflicts", sectionId, sectionName },
      }),
    [],
  );
  const openViewSectionDetails = useCallback(
    (sectionId: number) =>
      dispatch({
        kind: "open",
        state: { type: "view-section-details", sectionId },
      }),
    [],
  );

  return {
    state,
    close,
    openEditAdviser,
    openCancelSingle,
    openBulkOpen,
    openBulkCancel,
    openBulkAssignAdviser,
    openViewConflicts,
    openViewSectionDetails,
  };
}
