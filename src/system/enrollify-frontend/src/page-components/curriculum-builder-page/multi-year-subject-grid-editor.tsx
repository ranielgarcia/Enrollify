import {
  approveCurriculumOptions,
  saveCurriculumContentOptions,
} from "@/api/collections/curriculum-collection";
import { getAllSubjectsMinimalOptions } from "@/api/collections/subject-collection";
import {
  CurriculumStatusEnum,
  type CurriculumWithSubjects,
} from "@/api/models/curriculum";
import type { MultiSearchableSelectWithTriggerOption } from "@/components/form/searchable-select-with-custom-trigger";
import { Badge } from "@/components/ui/badge";
import { AddSubjectToTermDialog } from "./add-subject-to-term-dialog";
import { SubjectCard, type SubjectInCurriculum } from "./subject-card";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Switch } from "@/components/ui/switch";
import { useEnrollmentContext } from "@/contexts/enrollment-context/enrollment-context";
import { useMutation, useSuspenseQuery } from "@tanstack/react-query";
import { Loader2, Lock, Plus, Trash2 } from "lucide-react";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";

type TermGrid = Record<number, SubjectInCurriculum[]>;
type YearGrid = Record<number, TermGrid>;

type SaveStatus = "idle" | "saving" | "saved" | "error";

const createTermGrid = (numberOfTerms: number): TermGrid =>
  Object.fromEntries(
    Array.from({ length: numberOfTerms }, (_, i) => [i + 1, []]),
  );

const createInitialGrid = (
  numberOfYears: number,
  numberOfTerms: number,
): YearGrid =>
  Object.fromEntries(
    Array.from({ length: numberOfYears }, (_, i) => [
      i + 1,
      createTermGrid(numberOfTerms),
    ]),
  );

const DEFAULT_NUMBER_OF_YEARS = 2;

/**
 * Populates the grid state from the curriculum's existing subjects.
 * Maps curriculum subjects to the YearGrid structure organized by year and term.
 */
const populateGridFromCurriculum = (
  curriculum: CurriculumWithSubjects,
  numberOfTerms: number,
): { grid: YearGrid; activeYears: number[] } => {
  const { curriculumSubjects } = curriculum;

  // If no subjects, return default empty grid
  if (!curriculumSubjects || curriculumSubjects.length === 0) {
    return {
      grid: createInitialGrid(DEFAULT_NUMBER_OF_YEARS, numberOfTerms),
      activeYears: Array.from(
        { length: DEFAULT_NUMBER_OF_YEARS },
        (_, i) => i + 1,
      ),
    };
  }

  // Build a lookup map: curriculumSubjectId -> subject code
  // This is needed to resolve prerequisite references to subject codes
  const curriculumSubjectIdToCode = new Map<number, string>(
    curriculumSubjects.map((cs) => [cs.id, cs.subject.code]),
  );

  // Determine the unique years from curriculum subjects
  const yearsSet = new Set(curriculumSubjects.map((cs) => cs.yearLevel));
  const activeYears = Array.from(yearsSet).sort((a, b) => a - b);

  // Ensure we have at least the default number of years
  const maxYear = Math.max(...activeYears, DEFAULT_NUMBER_OF_YEARS);
  const allYears = Array.from({ length: maxYear }, (_, i) => i + 1);

  // Initialize grid with empty terms for all years
  const grid: YearGrid = Object.fromEntries(
    allYears.map((year) => [year, createTermGrid(numberOfTerms)]),
  );

  // Populate the grid with curriculum subjects
  for (const curriculumSubject of curriculumSubjects) {
    const { yearLevel, termNumber, subject, prerequisites } = curriculumSubject;

    // Resolve prerequisite IDs to subject codes
    const prerequisiteCodes = prerequisites
      .map((prereq) =>
        curriculumSubjectIdToCode.get(prereq.prerequisiteCurriculumSubjectId),
      )
      .filter((code): code is string => code !== undefined);

    const subjectInCurriculum: SubjectInCurriculum = {
      id: subject.id,
      code: subject.code,
      title: subject.title,
      units: subject.units,
      unitsOverride: curriculumSubject.unitsOverride ?? null,
      prerequisites: prerequisiteCodes,
      daysPerWeek: curriculumSubject.daysPerWeek,
      hoursPerDay: curriculumSubject.hoursPerDay,
    };

    // Add subject to the appropriate year and term
    if (grid[yearLevel] && grid[yearLevel][termNumber]) {
      grid[yearLevel][termNumber].push(subjectInCurriculum);
    }
  }

  return { grid, activeYears: allYears };
};

interface MultiYearSubjectGridEditorProps {
  curriculum: CurriculumWithSubjects;
}

const SESSION_STORAGE_KEY = "curr-multi-year-subj-grid-editor-auto-save";

export default function MultiYearSubjectGridEditor({
  curriculum,
}: MultiYearSubjectGridEditorProps) {
  const { academicCoreSettings } = useEnrollmentContext();
  const numberOfTerms = academicCoreSettings.academicTermSystem;

  const isReadOnly = curriculum.status.value === CurriculumStatusEnum.Active;

  const [isDirty, setIsDirty] = useState(false);
  const [saveStatus, setSaveStatus] = useState<SaveStatus>("idle");
  const [isAutoSaveEnabled, setIsAutoSaveEnabled] = useState(() =>
    sessionStorage.getItem(SESSION_STORAGE_KEY) &&
    sessionStorage.getItem(SESSION_STORAGE_KEY) === "enabled"
      ? true
      : false,
  );
  const saveTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const handleAutoSaveToggle = (value: boolean) => {
    setIsAutoSaveEnabled(value);
    sessionStorage.setItem(SESSION_STORAGE_KEY, value ? "enabled" : "disabled");
  };

  // Track pending removals for rollback on save error
  const pendingRemovedSubjectsRef = useRef<
    Array<{
      year: number;
      term: number;
      subject: SubjectInCurriculum;
    }>
  >([]);
  const pendingRemovedPrerequisitesRef = useRef<
    Array<{
      year: number;
      term: number;
      subjectId: number;
      prerequisiteCode: string;
    }>
  >([]);

  const [initialState] = useState(() =>
    populateGridFromCurriculum(curriculum, numberOfTerms),
  );
  const [activeYears, setActiveYears] = useState<number[]>(
    initialState.activeYears,
  );
  const [grid, setGrid] = useState<YearGrid>(initialState.grid);
  const [isAddSubjectDialogOpen, setIsAddSubjectDialogOpen] = useState(false);
  const [addSubjectTargetYear, setAddSubjectTargetYear] = useState<number>(1);
  const [addSubjectTargetTerm, setAddSubjectTargetTerm] = useState<number>(1);

  const { mutateAsync: saveCurriculumContentAsync } = useMutation(
    saveCurriculumContentOptions(curriculum.id),
  );
  const { mutateAsync: approveCurriculumAsync } = useMutation(
    approveCurriculumOptions(curriculum.id),
  );

  const { data: availableSubjects } = useSuspenseQuery(
    getAllSubjectsMinimalOptions(),
  );

  // Rollback removed subjects and prerequisites on save error
  const rollbackRemovals = (
    removedSubjects: typeof pendingRemovedSubjectsRef.current,
    removedPrerequisites: typeof pendingRemovedPrerequisitesRef.current,
  ) => {
    // Don't clear isDirty here - the editor still has unsaved changes after a failed save.
    // The auto-save effect will skip retries when saveStatus === "error".

    setGrid((prev) => {
      let newGrid = { ...prev };

      // Restore removed subjects
      for (const removal of removedSubjects) {
        const { year, term, subject } = removal;
        if (newGrid[year] && newGrid[year][term]) {
          // Check if subject doesn't already exist (avoid duplicates)
          const exists = newGrid[year][term].some((s) => s.id === subject.id);
          if (!exists) {
            newGrid = {
              ...newGrid,
              [year]: {
                ...newGrid[year],
                [term]: [...newGrid[year][term], subject],
              },
            };
          }
        }
      }

      // Restore removed prerequisites
      for (const removal of removedPrerequisites) {
        const { year, term, subjectId, prerequisiteCode } = removal;
        if (newGrid[year] && newGrid[year][term]) {
          const subjects = [...newGrid[year][term]];
          const subjectIndex = subjects.findIndex((s) => s.id === subjectId);
          if (subjectIndex !== -1) {
            const subject = { ...subjects[subjectIndex] };
            if (!subject.prerequisites.includes(prerequisiteCode)) {
              subject.prerequisites = [
                ...subject.prerequisites,
                prerequisiteCode,
              ];
              subjects[subjectIndex] = subject;
              newGrid = {
                ...newGrid,
                [year]: {
                  ...newGrid[year],
                  [term]: subjects,
                },
              };
            }
          }
        }
      }

      return newGrid;
    });
  };

  // Clear pending removals on successful save
  const clearPendingRemovals = () => {
    pendingRemovedSubjectsRef.current = [];
    pendingRemovedPrerequisitesRef.current = [];
  };

  const performSave = useCallback(async () => {
    const capturedRemovedSubjects = [...pendingRemovedSubjectsRef.current];
    const capturedRemovedPrerequisites = [
      ...pendingRemovedPrerequisitesRef.current,
    ];

    setSaveStatus("saving");
    try {
      await saveCurriculumContentAsync({ grid });
      setSaveStatus("saved");
      setIsDirty(false);
      clearPendingRemovals();
      setTimeout(() => setSaveStatus("idle"), 2000);
    } catch {
      setSaveStatus("error");
      clearPendingRemovals();
      rollbackRemovals(capturedRemovedSubjects, capturedRemovedPrerequisites);
    }
  }, [grid, saveCurriculumContentAsync]);

  // Auto-save effect with debounce
  useEffect(() => {
    // Skip auto-save if disabled, not dirty, or in error state
    if (!isAutoSaveEnabled || !isDirty || saveStatus === "error") return;

    // Clear any existing timeout
    if (saveTimeoutRef.current) {
      clearTimeout(saveTimeoutRef.current);
    }

    saveTimeoutRef.current = setTimeout(() => performSave(), 1500);

    return () => {
      if (saveTimeoutRef.current) {
        clearTimeout(saveTimeoutRef.current);
      }
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps -- saveStatus is intentionally excluded to prevent re-triggering on status changes
  }, [
    grid,
    isDirty,
    isAutoSaveEnabled,
    performSave,
    saveCurriculumContentAsync,
  ]);

  // Helper to mark grid as dirty when updating
  const updateGridWithDirty = (updater: (prev: YearGrid) => YearGrid) => {
    setGrid((prev) => {
      const newGrid = updater(prev);
      // Only mark dirty if the grid actually changed
      if (newGrid !== prev) {
        setIsDirty(true);
        // Reset error state when user makes a new change, allowing auto-save to retry
        if (saveStatus === "error") {
          setSaveStatus("idle");
        }
      }
      return newGrid;
    });
  };

  const addSubjectToGrid = (year: number, term: number, subjectId: number) => {
    const subject = availableSubjects.find((s) => s.id === subjectId);
    if (!subject) return;

    updateGridWithDirty((prev) => {
      // Check if subject already exists in this year/term
      const subjectExists = prev[year][term].some((s) => s.id === subjectId);

      if (subjectExists) return prev;

      return {
        ...prev,
        [year]: {
          ...prev[year],
          [term]: [
            ...prev[year][term],
            {
              ...subject,
              subjectId: subject.id,
              unitsOverride: null,
              prerequisites: [],
              daysPerWeek: 1,
              hoursPerDay: 1,
            },
          ],
        },
      };
    });
  };

  const removeSubject = (year: number, term: number, subjectId: number) => {
    // Track the subject being removed for potential rollback
    const subjectToRemove = grid[year]?.[term]?.find((s) => s.id === subjectId);
    if (subjectToRemove) {
      pendingRemovedSubjectsRef.current.push({
        year,
        term,
        subject: { ...subjectToRemove },
      });
    }

    updateGridWithDirty((prev) => ({
      ...prev,
      [year]: {
        ...prev[year],
        [term]: prev[year][term].filter((s) => s.id !== subjectId),
      },
    }));
  };

  const addYear = () => {
    const nextYear = activeYears.length > 0 ? Math.max(...activeYears) + 1 : 1;
    setActiveYears((prev) => [...prev, nextYear]);
    updateGridWithDirty((prev) => ({
      ...prev,
      [nextYear]: createTermGrid(numberOfTerms),
    }));
  };

  const removeYear = (year: number) => {
    // Track all subjects in the year for potential rollback
    if (grid[year]) {
      for (const [termKey, subjects] of Object.entries(grid[year])) {
        for (const subject of subjects) {
          pendingRemovedSubjectsRef.current.push({
            year,
            term: Number(termKey),
            subject: { ...subject },
          });
        }
      }
    }

    setActiveYears((prev) => prev.filter((y) => y !== year));
    updateGridWithDirty((prev) => {
      const newGrid = { ...prev };
      delete newGrid[year];
      return newGrid;
    });
  };

  const addPrerequisites = (
    year: number,
    term: number,
    subjectId: number,
    prerequisiteCodes: string[],
  ) => {
    updateGridWithDirty((prev) => {
      const newGrid = { ...prev };
      const subjects = [...newGrid[year][term]];
      const subjectIndex = subjects.findIndex((s) => s.id === subjectId);

      if (subjectIndex !== -1) {
        const subject = { ...subjects[subjectIndex] };
        // Filter out duplicates and add new prerequisites
        const newPrereqs = prerequisiteCodes.filter(
          (code) => !subject.prerequisites.includes(code),
        );
        subject.prerequisites = [...subject.prerequisites, ...newPrereqs];
        subjects[subjectIndex] = subject;
        newGrid[year][term] = subjects;
      }

      return newGrid;
    });
  };

  const setUnitsOverride = (
    year: number,
    term: number,
    subjectId: number,
    value: number | null,
  ) => {
    updateGridWithDirty((prev) => {
      const subjects = [...prev[year][term]];
      const idx = subjects.findIndex((s) => s.id === subjectId);
      if (idx === -1) return prev;
      subjects[idx] = { ...subjects[idx], unitsOverride: value };
      return {
        ...prev,
        [year]: { ...prev[year], [term]: subjects },
      };
    });
  };

  const setDaysPerWeek = (
    year: number,
    term: number,
    subjectId: number,
    value: number,
  ) => {
    updateGridWithDirty((prev) => {
      const subjects = [...prev[year][term]];
      const idx = subjects.findIndex((s) => s.id === subjectId);
      if (idx === -1) return prev;
      subjects[idx] = { ...subjects[idx], daysPerWeek: value };
      return {
        ...prev,
        [year]: { ...prev[year], [term]: subjects },
      };
    });
  };

  const setHoursPerDay = (
    year: number,
    term: number,
    subjectId: number,
    value: number,
  ) => {
    updateGridWithDirty((prev) => {
      const subjects = [...prev[year][term]];
      const idx = subjects.findIndex((s) => s.id === subjectId);
      if (idx === -1) return prev;
      subjects[idx] = { ...subjects[idx], hoursPerDay: value };
      return {
        ...prev,
        [year]: { ...prev[year], [term]: subjects },
      };
    });
  };

  const removePrerequisite = (
    year: number,
    term: number,
    subjectId: number,
    prerequisiteCode: string,
  ) => {
    // Track the prerequisite being removed for potential rollback
    pendingRemovedPrerequisitesRef.current.push({
      year,
      term,
      subjectId,
      prerequisiteCode,
    });

    updateGridWithDirty((prev) => {
      const newGrid = { ...prev };
      const subjects = [...newGrid[year][term]];
      const subjectIndex = subjects.findIndex((s) => s.id === subjectId);

      if (subjectIndex !== -1) {
        const subject = { ...subjects[subjectIndex] };
        subject.prerequisites = subject.prerequisites.filter(
          (code) => code !== prerequisiteCode,
        );
        subjects[subjectIndex] = subject;
        newGrid[year][term] = subjects;
      }

      return newGrid;
    });
  };

  const allAddedSubjectCodes = useMemo(
    () =>
      new Set(
        Object.values(grid).flatMap((yearData) =>
          Object.values(yearData).flatMap((termSubjects) =>
            termSubjects.map((s) => s.code),
          ),
        ),
      ),
    [grid],
  );

  const handleAddSubjectsToTerm = async (subjectCodes: string[]) => {
    for (const code of subjectCodes) {
      const subject = availableSubjects.find((s) => s.code === code);
      if (subject) {
        addSubjectToGrid(
          addSubjectTargetYear,
          addSubjectTargetTerm,
          subject.id,
        );
      }
    }
  };

  /**
   * Returns all subjects added across all years and terms that can be
   * used as prerequisites for the specified subject.
   * Excludes:
   * - The subject itself
   * - Subjects already added as prerequisites
   * - Subjects from future years
   * - Subjects from the same year but same or future terms
   */
  const getAvailablePrerequisites = (
    year: number,
    term: number,
    subjectCode: string,
  ) => {
    // Find the current subject to get its existing prerequisites
    const currentSubject = grid[year][term].find((s) => s.code === subjectCode);
    const existingPrerequisites = new Set(currentSubject?.prerequisites ?? []);

    return Object.entries(grid).flatMap(([yearKey, yearData]) =>
      Object.entries(yearData).flatMap(([termKey, subjects]) => {
        const subjectYear = Number(yearKey);
        const subjectTerm = Number(termKey);

        // Exclude subjects from future years
        if (subjectYear > year) return [];

        // Exclude subjects from the same year and same or future terms
        if (subjectYear === year && subjectTerm >= term) return [];

        // Filter out the subject itself and already added prerequisites
        return subjects.filter(
          (s) => s.code !== subjectCode && !existingPrerequisites.has(s.code),
        );
      }),
    );
  };

  const getAvailableSubjectForPrerequisitesOptions = (
    year: number,
    term: number,
    subjectCode: string,
  ): MultiSearchableSelectWithTriggerOption[] => {
    const availablePrerequisites = getAvailablePrerequisites(
      year,
      term,
      subjectCode,
    );
    return availablePrerequisites.map((s) => ({
      value: s.code,
      label: `${s.code} - ${s.title}`,
    }));
  };

  return (
    <>
      {/* Read-only banner for Active curricula */}
      {isReadOnly && (
        <div className="flex items-center gap-2 rounded-lg border border-amber-300 bg-amber-50 px-4 py-3 text-sm text-amber-950 dark:border-amber-700 dark:bg-amber-950/40 dark:text-amber-100">
          <Lock className="size-4 shrink-0" />
          <span>
            This curriculum is <strong>Active</strong> and its content cannot be
            edited.
          </span>
        </div>
      )}

      {/* Auto-save status indicator */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          {!isAutoSaveEnabled ? (
            <span className="flex items-center gap-1.5">
              <span className="size-1.5 rounded-full bg-muted-foreground/40" />
              Auto-save off
            </span>
          ) : saveStatus === "idle" && !isDirty ? (
            <span className="flex items-center gap-1.5">
              <span className="size-1.5 rounded-full bg-emerald-500" />
              All changes saved
            </span>
          ) : saveStatus === "idle" && isDirty ? (
            <span className="flex items-center gap-1.5">
              <span className="size-1.5 rounded-full bg-amber-400 animate-pulse" />
              Unsaved changes
            </span>
          ) : saveStatus === "saving" ? (
            <span className="flex items-center gap-1.5">
              <Loader2 className="size-3 animate-spin" />
              Saving...
            </span>
          ) : saveStatus === "saved" ? (
            <span className="flex items-center gap-1.5 text-emerald-600 dark:text-emerald-400">
              <span className="size-1.5 rounded-full bg-emerald-500" />
              Saved
            </span>
          ) : saveStatus === "error" ? (
            <span className="flex items-center gap-1.5 text-destructive">
              <span className="size-1.5 rounded-full bg-destructive" />
              Save failed
            </span>
          ) : null}
        </div>
        <div className="flex items-center gap-2">
          {!isReadOnly && (
            <>
              <label
                htmlFor="auto-save-switch"
                className="text-sm text-muted-foreground cursor-pointer select-none"
              >
                Auto-save
              </label>
              <Switch
                id="auto-save-switch"
                checked={isAutoSaveEnabled}
                onCheckedChange={handleAutoSaveToggle}
                size="sm"
              />
            </>
          )}
        </div>
      </div>

      <div className="space-y-10">
        {activeYears.map((year) => {
          const yearAccentClasses = [
            "border-l-primary bg-primary/10 text-primary",
            "border-l-teal-500 bg-teal-500/10 text-teal-600 dark:text-teal-400",
            "border-l-amber-500 bg-amber-500/10 text-amber-600 dark:text-amber-400",
            "border-l-rose-500 bg-rose-500/10 text-rose-600 dark:text-rose-400",
          ];
          const yearCardTopClasses = [
            "border-t-primary/50",
            "border-t-teal-400/60",
            "border-t-amber-400/60",
            "border-t-rose-400/60",
          ];
          const accentIdx = Math.min(year - 1, yearAccentClasses.length - 1);
          const yearAccent = yearAccentClasses[accentIdx];
          const yearCardTop = yearCardTopClasses[accentIdx];
          return (
            <div key={year} className="space-y-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2.5">
                  <div
                    className={`flex items-center gap-1.5 rounded-md border-l-4 pl-2.5 pr-3 py-1 ${yearAccent}`}
                  >
                    <span className="text-[10px] font-bold uppercase tracking-widest">
                      Year
                    </span>
                    <span className="text-xl font-bold leading-none nums">
                      {year}
                    </span>
                  </div>
                </div>
                {!isReadOnly && (
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-muted-foreground hover:text-destructive gap-2"
                    onClick={() => removeYear(year)}
                  >
                    <Trash2 className="size-4" />
                    Remove Year {year}
                  </Button>
                )}
              </div>

              <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {Array.from({ length: numberOfTerms }, (_, i) => i + 1).map(
                  (term) => (
                    <Card
                      key={term}
                      className={`flex flex-col border-t-2 ${yearCardTop} transition-colors`}
                    >
                      <CardHeader className="pb-3 pt-4">
                        <CardTitle className="text-sm font-semibold uppercase tracking-wider text-muted-foreground flex items-center justify-between">
                          <span>Term {term}</span>
                          <Badge
                            variant="secondary"
                            className="font-semibold text-[10px] uppercase tracking-wide nums"
                          >
                            {grid[year][term].reduce(
                              (acc, s) => acc + (s.unitsOverride ?? s.units),
                              0,
                            )}{" "}
                            Units Total
                          </Badge>
                        </CardTitle>
                      </CardHeader>
                      <CardContent className="flex-1 pt-4 space-y-4">
                        <div className="space-y-2">
                          {grid[year][term].map((subject) => (
                            <SubjectCard
                              key={subject.id}
                              subject={subject}
                              isReadOnly={isReadOnly}
                              availablePrerequisiteOptions={getAvailableSubjectForPrerequisitesOptions(
                                year,
                                term,
                                subject.code,
                              )}
                              onRemove={() =>
                                removeSubject(year, term, subject.id)
                              }
                              onUnitsOverrideChange={(value) =>
                                setUnitsOverride(year, term, subject.id, value)
                              }
                              onRemovePrerequisite={(pre) =>
                                removePrerequisite(year, term, subject.id, pre)
                              }
                              onAddPrerequisites={(codes) =>
                                addPrerequisites(year, term, subject.id, codes)
                              }
                              onDaysPerWeekChange={(value) =>
                                setDaysPerWeek(year, term, subject.id, value)
                              }
                              onHoursPerDayChange={(value) =>
                                setHoursPerDay(year, term, subject.id, value)
                              }
                            />
                          ))}
                        </div>
                        {!isReadOnly && (
                          <Button
                            variant="outline"
                            className="w-full h-9 border-dashed text-xs text-muted-foreground hover:bg-transparent hover:border-accent hover:text-accent transition-colors"
                            onClick={() => {
                              setAddSubjectTargetYear(year);
                              setAddSubjectTargetTerm(term);
                              setIsAddSubjectDialogOpen(true);
                            }}
                          >
                            <Plus className="size-3 mr-2" />
                            Add Subject to Term {term}
                          </Button>
                        )}
                      </CardContent>
                    </Card>
                  ),
                )}
              </div>
            </div>
          );
        })}

        {!isReadOnly && (
          <Button
            variant="outline"
            className="w-full py-8 border-dashed border-2 hover:border-primary hover:bg-primary/5 transition-all gap-2 bg-transparent"
            onClick={addYear}
          >
            <Plus className="size-5" />
            Add Academic Year{" "}
            {activeYears.length > 0 ? Math.max(...activeYears) + 1 : 1}
          </Button>
        )}
      </div>

      {!isReadOnly && (
        <div className="flex items-center justify-between gap-3 pt-8 border-t">
          <div className="flex gap-3">
            <Button
              variant="outline"
              onClick={performSave}
              disabled={saveStatus === "saving"}
            >
              {saveStatus === "saving" ? (
                <Loader2 className="size-4 mr-2 animate-spin" />
              ) : null}
              Save as Draft
            </Button>
            <Button
              className="bg-primary text-primary-foreground"
              onClick={async () => await approveCurriculumAsync({})}
            >
              Finalize Curriculum
            </Button>
          </div>
        </div>
      )}

      <AddSubjectToTermDialog
        isOpen={isAddSubjectDialogOpen}
        onOpenChange={setIsAddSubjectDialogOpen}
        year={addSubjectTargetYear}
        term={addSubjectTargetTerm}
        excludeSubjectCodes={Array.from(allAddedSubjectCodes)}
        onSubmit={handleAddSubjectsToTerm}
      />
    </>
  );
}
