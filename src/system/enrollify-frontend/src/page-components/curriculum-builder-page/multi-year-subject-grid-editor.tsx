import { saveCurriculumContentOptions } from "@/api/collections/curriculum-collection";
import { getAllSubjectsMinimalOptions } from "@/api/collections/subject-collection";
import type { CurriculumWithSubjects } from "@/api/models/curriculum";
import {
  SearchableSelectWithCustomTrigger,
  type MultiSearchableSelectWithTriggerOption,
} from "@/components/searchable-select-with-custom-trigger";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useSystemSettingsContext } from "@/infrastructure/system-settings/system-settings-context";
import { useMutation, useSuspenseQuery } from "@tanstack/react-query";
import { Check, Cloud, CloudOff, Loader2, Plus, Trash2 } from "lucide-react";
import { useEffect, useRef, useState } from "react";

interface SubjectInCurriculum {
  id: number;
  code: string;
  title: string;
  units: number;
  prerequisites: string[];
}

type SemesterGrid = Record<number, SubjectInCurriculum[]>;
type YearGrid = Record<number, SemesterGrid>;

type SaveStatus = "idle" | "saving" | "saved" | "error";

const createSemesterGrid = (numberOfSemesters: number): SemesterGrid =>
  Object.fromEntries(
    Array.from({ length: numberOfSemesters }, (_, i) => [i + 1, []]),
  );

const createInitialGrid = (
  numberOfYears: number,
  numberOfSemesters: number,
): YearGrid =>
  Object.fromEntries(
    Array.from({ length: numberOfYears }, (_, i) => [
      i + 1,
      createSemesterGrid(numberOfSemesters),
    ]),
  );

const DEFAULT_NUMBER_OF_YEARS = 2;

/**
 * Populates the grid state from the curriculum's existing subjects.
 * Maps curriculum subjects to the YearGrid structure organized by year and semester.
 */
const populateGridFromCurriculum = (
  curriculum: CurriculumWithSubjects,
  numberOfSemesters: number,
): { grid: YearGrid; activeYears: number[] } => {
  const { curriculumSubjects } = curriculum;

  // If no subjects, return default empty grid
  if (!curriculumSubjects || curriculumSubjects.length === 0) {
    return {
      grid: createInitialGrid(DEFAULT_NUMBER_OF_YEARS, numberOfSemesters),
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

  // Initialize grid with empty semesters for all years
  const grid: YearGrid = Object.fromEntries(
    allYears.map((year) => [year, createSemesterGrid(numberOfSemesters)]),
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
      prerequisites: prerequisiteCodes,
    };

    // Add subject to the appropriate year and semester
    if (grid[yearLevel] && grid[yearLevel][termNumber]) {
      grid[yearLevel][termNumber].push(subjectInCurriculum);
    }
  }

  return { grid, activeYears: allYears };
};

interface MultiYearSubjectGridEditorProps {
  curriculum: CurriculumWithSubjects;
}

export default function MultiYearSubjectGridEditor({
  curriculum,
}: MultiYearSubjectGridEditorProps) {
  const systemSettings = useSystemSettingsContext();
  const numberOfSemesters = systemSettings.academicSettings.academicSystem;

  const { mutateAsync: saveCurriculumContentAsync } = useMutation(
    saveCurriculumContentOptions(curriculum.id),
  );

  const [isDirty, setIsDirty] = useState(false);
  const [saveStatus, setSaveStatus] = useState<SaveStatus>("idle");
  const saveTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const [activeYears, setActiveYears] = useState<number[]>(() => {
    const { activeYears: years } = populateGridFromCurriculum(
      curriculum,
      numberOfSemesters,
    );
    return years;
  });
  const [grid, setGrid] = useState<YearGrid>(() => {
    const { grid: populatedGrid } = populateGridFromCurriculum(
      curriculum,
      numberOfSemesters,
    );
    return populatedGrid;
  });

  const { data: availableSubjects } = useSuspenseQuery(
    getAllSubjectsMinimalOptions(),
  );

  // Auto-save effect with debounce
  useEffect(() => {
    if (!isDirty) return;

    // Clear any existing timeout
    if (saveTimeoutRef.current) {
      clearTimeout(saveTimeoutRef.current);
    }

    // Set up debounced save
    saveTimeoutRef.current = setTimeout(async () => {
      setSaveStatus("saving");
      try {
        await saveCurriculumContentAsync({ grid });
        setSaveStatus("saved");
        setIsDirty(false);

        // Reset to idle after showing "saved" for 2 seconds
        setTimeout(() => setSaveStatus("idle"), 2000);
      } catch {
        setSaveStatus("error");
      }
    }, 1500); // 1.5 second debounce

    return () => {
      if (saveTimeoutRef.current) {
        clearTimeout(saveTimeoutRef.current);
      }
    };
  }, [grid, isDirty, saveCurriculumContentAsync]);

  // Helper to mark grid as dirty when updating
  const updateGridWithDirty = (updater: (prev: YearGrid) => YearGrid) => {
    setGrid((prev) => {
      const newGrid = updater(prev);
      // Only mark dirty if the grid actually changed
      if (newGrid !== prev) {
        setIsDirty(true);
      }
      return newGrid;
    });
  };

  const addSubjectToGrid = (
    year: number,
    semester: number,
    subjectId: number,
  ) => {
    const subject = availableSubjects.find((s) => s.id === subjectId);
    if (!subject) return;

    updateGridWithDirty((prev) => {
      // Check if subject already exists in this year/semester
      const subjectExists = prev[year][semester].some(
        (s) => s.id === subjectId,
      );

      if (subjectExists) return prev;

      return {
        ...prev,
        [year]: {
          ...prev[year],
          [semester]: [
            ...prev[year][semester],
            { ...subject, subjectId: subject.id, prerequisites: [] },
          ],
        },
      };
    });
  };

  const removeSubject = (year: number, semester: number, subjectId: number) => {
    updateGridWithDirty((prev) => ({
      ...prev,
      [year]: {
        ...prev[year],
        [semester]: prev[year][semester].filter((s) => s.id !== subjectId),
      },
    }));
  };

  const addYear = () => {
    const nextYear = activeYears.length > 0 ? Math.max(...activeYears) + 1 : 1;
    setActiveYears((prev) => [...prev, nextYear]);
    updateGridWithDirty((prev) => ({
      ...prev,
      [nextYear]: createSemesterGrid(numberOfSemesters),
    }));
  };

  const removeYear = (year: number) => {
    setActiveYears((prev) => prev.filter((y) => y !== year));
    updateGridWithDirty((prev) => {
      const newGrid = { ...prev };
      delete newGrid[year];
      return newGrid;
    });
  };

  const addPrerequisites = (
    year: number,
    semester: number,
    subjectId: number,
    prerequisiteCodes: string[],
  ) => {
    updateGridWithDirty((prev) => {
      const newGrid = { ...prev };
      const subjects = [...newGrid[year][semester]];
      const subjectIndex = subjects.findIndex((s) => s.id === subjectId);

      if (subjectIndex !== -1) {
        const subject = { ...subjects[subjectIndex] };
        // Filter out duplicates and add new prerequisites
        const newPrereqs = prerequisiteCodes.filter(
          (code) => !subject.prerequisites.includes(code),
        );
        subject.prerequisites = [...subject.prerequisites, ...newPrereqs];
        subjects[subjectIndex] = subject;
        newGrid[year][semester] = subjects;
      }

      return newGrid;
    });
  };

  const removePrerequisite = (
    year: number,
    semester: number,
    subjectId: number,
    prerequisiteCode: string,
  ) => {
    updateGridWithDirty((prev) => {
      const newGrid = { ...prev };
      const subjects = [...newGrid[year][semester]];
      const subjectIndex = subjects.findIndex((s) => s.id === subjectId);

      if (subjectIndex !== -1) {
        const subject = { ...subjects[subjectIndex] };
        subject.prerequisites = subject.prerequisites.filter(
          (code) => code !== prerequisiteCode,
        );
        subjects[subjectIndex] = subject;
        newGrid[year][semester] = subjects;
      }

      return newGrid;
    });
  };

  const getSubjectsOptionsForYearAndSemester =
    (): MultiSearchableSelectWithTriggerOption[] => {
      // Get all subject IDs already added across all years and semesters
      const allAddedSubjectIds = new Set(
        Object.values(grid).flatMap((yearData) =>
          Object.values(yearData).flatMap((semesterSubjects) =>
            semesterSubjects.map((s) => s.id),
          ),
        ),
      );

      return availableSubjects
        .filter((s) => !allAddedSubjectIds.has(s.id))
        .map((s) => ({
          value: s.id.toString(),
          label: `${s.code} - ${s.title} (${s.units}u)`,
        }));
    };

  /**
   * Returns all subjects added across all years and semesters that can be
   * used as prerequisites for the specified subject.
   * Excludes:
   * - The subject itself
   * - Subjects already added as prerequisites
   * - Subjects from future years
   * - Subjects from the same year but same or future semesters
   */
  const getAvailablePrerequisites = (
    year: number,
    semester: number,
    subjectCode: string,
  ) => {
    // Find the current subject to get its existing prerequisites
    const currentSubject = grid[year][semester].find(
      (s) => s.code === subjectCode,
    );
    const existingPrerequisites = new Set(currentSubject?.prerequisites ?? []);

    return Object.entries(grid).flatMap(([yearKey, yearData]) =>
      Object.entries(yearData).flatMap(([semesterKey, subjects]) => {
        const subjectYear = Number(yearKey);
        const subjectSemester = Number(semesterKey);

        // Exclude subjects from future years
        if (subjectYear > year) return [];

        // Exclude subjects from the same year and same or future semesters
        if (subjectYear === year && subjectSemester >= semester) return [];

        // Filter out the subject itself and already added prerequisites
        return subjects.filter(
          (s) => s.code !== subjectCode && !existingPrerequisites.has(s.code),
        );
      }),
    );
  };

  const getAvailableSubjectForPrerequisitesOptions = (
    year: number,
    semester: number,
    subjectCode: string,
  ): MultiSearchableSelectWithTriggerOption[] => {
    const availablePrerequisites = getAvailablePrerequisites(
      year,
      semester,
      subjectCode,
    );
    return availablePrerequisites.map((s) => ({
      value: s.code,
      label: `${s.code} - ${s.title}`,
    }));
  };

  return (
    <>
      <div className="space-y-12">
        {activeYears.map((year) => (
          <div key={year} className="space-y-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <div className="size-8 rounded-full bg-primary flex items-center justify-center text-primary-foreground font-bold">
                  {year}
                </div>
                <h2 className="text-2xl font-bold tracking-tight">
                  Year {year}
                </h2>
              </div>
              <Button
                variant="ghost"
                size="sm"
                className="text-muted-foreground hover:text-destructive gap-2"
                onClick={() => removeYear(year)}
              >
                <Trash2 className="size-4" />
                Remove Year {year}
              </Button>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
              {Array.from({ length: numberOfSemesters }, (_, i) => i + 1).map(
                (semester) => (
                  <Card
                    key={semester}
                    className="flex flex-col border-muted hover:border-accent/50 transition-colors"
                  >
                    <CardHeader className="bg-muted/30 pb-4">
                      <CardTitle className="text-lg flex items-center justify-between">
                        Semester {semester}
                        <Badge
                          variant="secondary"
                          className="font-normal text-[10px] uppercase"
                        >
                          {grid[year][semester].reduce(
                            (acc, s) => acc + s.units,
                            0,
                          )}{" "}
                          Units Total
                        </Badge>
                      </CardTitle>
                    </CardHeader>
                    <CardContent className="flex-1 pt-4 space-y-4">
                      <div className="space-y-2">
                        {grid[year][semester].map((subject) => (
                          <div
                            key={subject.id}
                            className="group p-3 border rounded-lg bg-background hover:shadow-sm transition-all flex items-start justify-between gap-2"
                          >
                            <div className="flex-1 min-w-0">
                              <div className="flex items-center gap-2 mb-1">
                                <span className="text-[10px] font-bold text-accent uppercase">
                                  {subject.code}
                                </span>
                                <span className="text-[10px] bg-accent/10 text-accent px-1.5 py-0.5 rounded-full font-medium">
                                  {subject.units} Units
                                </span>
                              </div>
                              <p className="text-sm font-semibold truncate leading-tight">
                                {subject.title}
                              </p>

                              {subject.prerequisites.length > 0 && (
                                <div className="mt-2 flex flex-wrap gap-1">
                                  {subject.prerequisites.map((pre) => (
                                    <Badge
                                      key={pre}
                                      variant="outline"
                                      className="text-[9px] px-1 h-5 border-dashed bg-accent/5 gap-1 group/badge"
                                    >
                                      Pre: {pre}
                                      <button
                                        type="button"
                                        className="ml-0.5 rounded-full hover:bg-destructive/20 p-0.5 transition-colors"
                                        onClick={(e) => {
                                          e.stopPropagation();
                                          removePrerequisite(
                                            year,
                                            semester,
                                            subject.id,
                                            pre,
                                          );
                                        }}
                                      >
                                        <Trash2 className="size-2.5 text-destructive" />
                                      </button>
                                    </Badge>
                                  ))}
                                </div>
                              )}

                              {/* Prerequisites Logic */}
                              <SearchableSelectWithCustomTrigger
                                options={getAvailableSubjectForPrerequisitesOptions(
                                  year,
                                  semester,
                                  subject.code,
                                )}
                                value={[]}
                                onValueChange={(subjectCodes: string[]) => {
                                  addPrerequisites(
                                    year,
                                    semester,
                                    subject.id,
                                    subjectCodes,
                                  );
                                }}
                                searchPlaceholder="Search subjects..."
                                emptyMessage="No subjects found"
                                trigger={
                                  <Button
                                    variant="ghost"
                                    size="sm"
                                    className="h-auto p-0 text-[9px] text-muted-foreground italic hover:text-accent flex flex-wrap gap-1 justify-start"
                                  >
                                    <span>+ Add Prerequisite</span>
                                  </Button>
                                }
                              />
                            </div>
                            <Button
                              variant="ghost"
                              size="sm"
                              className="size-7 p-0 opacity-0 group-hover:opacity-100 text-destructive transition-opacity"
                              onClick={() =>
                                removeSubject(year, semester, subject.id)
                              }
                            >
                              <Trash2 className="size-3.5" />
                            </Button>
                          </div>
                        ))}
                      </div>
                      <SearchableSelectWithCustomTrigger
                        options={getSubjectsOptionsForYearAndSemester()}
                        value={[]}
                        onValueChange={(values: string[]) => {
                          values.forEach((val) =>
                            addSubjectToGrid(year, semester, Number(val)),
                          );
                        }}
                        searchPlaceholder="Search subjects..."
                        emptyMessage="No subjects found"
                        trigger={
                          <Button
                            variant="outline"
                            className="w-full h-9 border-dashed text-xs text-muted-foreground hover:bg-transparent hover:border-accent hover:text-accent transition-colors"
                          >
                            <Plus className="size-3 mr-2" />
                            Add Subject to Sem {semester}
                          </Button>
                        }
                      />
                    </CardContent>
                  </Card>
                ),
              )}
            </div>
          </div>
        ))}

        <Button
          variant="outline"
          className="w-full py-8 border-dashed border-2 hover:border-primary hover:bg-primary/5 transition-all gap-2 bg-transparent"
          onClick={addYear}
        >
          <Plus className="size-5" />
          Add Academic Year {activeYears.length + 1}
        </Button>
      </div>

      <div className="flex items-center justify-between gap-3 pt-8 border-t">
        {/* Auto-save status indicator */}
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          {saveStatus === "idle" && !isDirty && (
            <>
              <Cloud className="size-4" />
              <span>All changes saved</span>
            </>
          )}
          {saveStatus === "idle" && isDirty && (
            <>
              <Cloud className="size-4 animate-pulse" />
              <span>Unsaved changes</span>
            </>
          )}
          {saveStatus === "saving" && (
            <>
              <Loader2 className="size-4 animate-spin" />
              <span>Saving...</span>
            </>
          )}
          {saveStatus === "saved" && (
            <>
              <Check className="size-4 text-green-600" />
              <span className="text-green-600">Saved</span>
            </>
          )}
          {saveStatus === "error" && (
            <>
              <CloudOff className="size-4 text-destructive" />
              <span className="text-destructive">Save failed</span>
            </>
          )}
        </div>

        <div className="flex gap-3">
          <Button
            variant="outline"
            onClick={async () => {
              setSaveStatus("saving");
              try {
                await saveCurriculumContentAsync({ grid });
                setSaveStatus("saved");
                setIsDirty(false);
                setTimeout(() => setSaveStatus("idle"), 2000);
              } catch {
                setSaveStatus("error");
              }
            }}
            disabled={saveStatus === "saving"}
          >
            {saveStatus === "saving" ? (
              <Loader2 className="size-4 mr-2 animate-spin" />
            ) : null}
            Save as Draft
          </Button>
          <Button className="bg-primary text-primary-foreground">
            Finalize Curriculum
          </Button>
        </div>
      </div>
    </>
  );
}
