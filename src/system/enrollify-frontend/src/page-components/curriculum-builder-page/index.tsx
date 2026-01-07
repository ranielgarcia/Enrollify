import { useState, Suspense } from "react";
import { CurriculumForm } from "./curriculum-form";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Plus, Trash2, BookOpen } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import { Check } from "lucide-react";
import { cn } from "@/lib/utils";
import {
  MultiSearchableSelectWithTrigger,
  type MultiSearchableSelectWithTriggerOption,
} from "@/components/multi-searchable-select-with-trigger";
import { useSuspenseQuery } from "@tanstack/react-query";
import { getAllSubjectsMinimalOptions } from "@/api/collections/subject-collection";
import { useSystemSettingsContext } from "@/infrastructure/system-settings/system-settings-context";

interface SubjectInCurriculum {
  id: number;
  code: string;
  title: string;
  units: number;
  prerequisites: string[];
}

const DEFAULT_NUMBER_OF_YEARS = 4;

type SemesterGrid = Record<number, SubjectInCurriculum[]>;
type YearGrid = Record<number, SemesterGrid>;

const createSemesterGrid = (numberOfSemesters: number): SemesterGrid =>
  Object.fromEntries(
    Array.from({ length: numberOfSemesters }, (_, i) => [i + 1, []])
  );

const createInitialGrid = (
  numberOfYears: number,
  numberOfSemesters: number
): YearGrid =>
  Object.fromEntries(
    Array.from({ length: numberOfYears }, (_, i) => [
      i + 1,
      createSemesterGrid(numberOfSemesters),
    ])
  );

function CurriculumContent() {
  const systemSettings = useSystemSettingsContext();
  const numberOfSemesters =
    systemSettings.curricularSettings.academicSystem.value;

  const [showForm, setShowForm] = useState(false);
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const [curriculum, setCurriculum] = useState<any>(null);

  const [activeYears, setActiveYears] = useState<number[]>(
    Array.from({ length: DEFAULT_NUMBER_OF_YEARS }, (_, i) => i + 1)
  );
  const [grid, setGrid] = useState<YearGrid>(() =>
    createInitialGrid(DEFAULT_NUMBER_OF_YEARS, numberOfSemesters)
  );

  const { data: availableSubjects } = useSuspenseQuery(
    getAllSubjectsMinimalOptions()
  );

  const addSubjectToGrid = (
    year: number,
    semester: number,
    subjectId: number
  ) => {
    const subject = availableSubjects.find((s) => s.id === subjectId);
    if (!subject) return;

    setGrid((prev) => {
      // Check if subject already exists in this year/semester
      const subjectExists = prev[year][semester].some(
        (s) => s.id === subjectId
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
    setGrid((prev) => ({
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
    setGrid((prev) => ({
      ...prev,
      [nextYear]: createSemesterGrid(numberOfSemesters),
    }));
  };

  const removeYear = (year: number) => {
    setActiveYears((prev) => prev.filter((y) => y !== year));
    setGrid((prev) => {
      const newGrid = { ...prev };
      delete newGrid[year];
      return newGrid;
    });
  };

  const togglePrerequisite = (
    year: number,
    semester: number,
    subjectId: number,
    prerequisiteCode: string
  ) => {
    setGrid((prev) => {
      const newGrid = { ...prev };
      const subjects = [...newGrid[year][semester]];
      const subjectIndex = subjects.findIndex((s) => s.id === subjectId);

      if (subjectIndex !== -1) {
        const subject = { ...subjects[subjectIndex] };
        const hasPrereq = subject.prerequisites.includes(prerequisiteCode);

        if (hasPrereq) {
          subject.prerequisites = subject.prerequisites.filter(
            (p) => p !== prerequisiteCode
          );
        } else {
          subject.prerequisites = [...subject.prerequisites, prerequisiteCode];
        }

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
            semesterSubjects.map((s) => s.id)
          )
        )
      );

      return availableSubjects
        .filter((s) => !allAddedSubjectIds.has(s.id))
        .map((s) => ({
          value: s.id.toString(),
          label: `${s.code} - ${s.title} (${s.units}u)`,
        }));
    };

  return (
    <main>
      <div className="p-4 md:p-8">
        <div className="mb-8 flex flex-col md:flex-row md:items-center md:justify-between gap-4">
          <div className="mb-8">
            <h1 className="text-3xl font-bold text-foreground mb-2">
              Curriculum Builder
            </h1>
            <p className="text-muted-foreground">
              Design and manage multi-year academic curricula
            </p>
          </div>
          {!curriculum && !showForm && (
            <Button onClick={() => setShowForm(true)} className="gap-2">
              <Plus className="size-4" />
              Create New Curriculum
            </Button>
          )}
        </div>

        {showForm && (
          <div className="mb-8">
            <CurriculumForm
              onClose={() => setShowForm(false)}
              onSave={(data) => {
                setCurriculum(data);
                setShowForm(false);
              }}
            />
          </div>
        )}

        {curriculum ? (
          <div className="space-y-8 animate-in fade-in duration-500">
            {/* Curriculum Header */}
            <Card className="border-accent bg-accent/5">
              <CardContent className="pt-6">
                <div className="flex flex-wrap gap-6 items-center">
                  <div>
                    <p className="text-xs uppercase tracking-wider text-muted-foreground mb-1 font-semibold">
                      Status
                    </p>
                    <Badge variant="outline" className="bg-background">
                      DRAFT
                    </Badge>
                  </div>
                  <div>
                    <p className="text-xs uppercase tracking-wider text-muted-foreground mb-1 font-semibold">
                      Version
                    </p>
                    <p className="font-bold">{curriculum.version}</p>
                  </div>
                  <div>
                    <p className="text-xs uppercase tracking-wider text-muted-foreground mb-1 font-semibold">
                      Effective Year
                    </p>
                    <p className="font-bold">{curriculum.effectiveYear}</p>
                  </div>
                  <div className="flex-1">
                    <p className="text-xs uppercase tracking-wider text-muted-foreground mb-1 font-semibold">
                      Description
                    </p>
                    <p className="text-sm italic">
                      {curriculum.description || "No description provided."}
                    </p>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => setCurriculum(null)}
                  >
                    Edit Details
                  </Button>
                </div>
              </CardContent>
            </Card>

            {/* Multi-Year Subject Grid */}
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
                    {Array.from({ length: numberOfSemesters }, (_, i) => i + 1).map((semester) => (
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
                                0
                              )}{" "}
                              Units Total
                            </Badge>
                          </CardTitle>
                        </CardHeader>
                        <CardContent className="flex-1 pt-4 space-y-4">
                          <div className="space-y-2">
                            {grid[year][semester].map((s) => (
                              <div
                                key={s.id}
                                className="group p-3 border rounded-lg bg-background hover:shadow-sm transition-all flex items-start justify-between gap-2"
                              >
                                <div className="flex-1 min-w-0">
                                  <div className="flex items-center gap-2 mb-1">
                                    <span className="text-[10px] font-bold text-accent uppercase">
                                      {s.code}
                                    </span>
                                    <span className="text-[10px] bg-accent/10 text-accent px-1.5 py-0.5 rounded-full font-medium">
                                      {s.units} Units
                                    </span>
                                  </div>
                                  <p className="text-sm font-semibold truncate leading-tight">
                                    {s.title}
                                  </p>

                                  {/* Prerequisites Logic */}
                                  <div className="mt-2 flex flex-wrap gap-1">
                                    <Popover>
                                      <PopoverTrigger asChild>
                                        <Button
                                          variant="ghost"
                                          size="sm"
                                          className="h-auto p-0 text-[9px] text-muted-foreground italic hover:text-accent flex flex-wrap gap-1 justify-start"
                                        >
                                          {s.prerequisites.length > 0 ? (
                                            s.prerequisites.map((pre) => (
                                              <Badge
                                                key={pre}
                                                variant="outline"
                                                className="text-[9px] px-1 h-4 border-dashed bg-accent/5"
                                              >
                                                Pre: {pre}
                                              </Badge>
                                            ))
                                          ) : (
                                            <span>+ Add Prerequisite</span>
                                          )}
                                        </Button>
                                      </PopoverTrigger>
                                      <PopoverContent
                                        className="w-[200px] p-0"
                                        align="start"
                                      >
                                        <Command>
                                          <CommandInput
                                            placeholder="Search subjects..."
                                            className="h-8 text-xs"
                                          />
                                          <CommandList>
                                            <CommandEmpty className="text-xs p-2">
                                              No subjects found.
                                            </CommandEmpty>
                                            <CommandGroup
                                              heading="Available Subjects"
                                              className="p-1"
                                            >
                                              {availableSubjects
                                                .filter(
                                                  (sub) => sub.code !== s.code
                                                ) // Can't be a prereq of itself
                                                .map((sub) => (
                                                  <CommandItem
                                                    key={sub.id}
                                                    onSelect={() =>
                                                      togglePrerequisite(
                                                        year,
                                                        semester,
                                                        s.id,
                                                        sub.code
                                                      )
                                                    }
                                                    className="text-xs py-1"
                                                  >
                                                    <Check
                                                      className={cn(
                                                        "mr-2 h-3 w-3",
                                                        s.prerequisites.includes(
                                                          sub.code
                                                        )
                                                          ? "opacity-100"
                                                          : "opacity-0"
                                                      )}
                                                    />
                                                    {sub.code}
                                                  </CommandItem>
                                                ))}
                                            </CommandGroup>
                                          </CommandList>
                                        </Command>
                                      </PopoverContent>
                                    </Popover>
                                  </div>
                                </div>
                                <Button
                                  variant="ghost"
                                  size="sm"
                                  className="size-7 p-0 opacity-0 group-hover:opacity-100 text-destructive transition-opacity"
                                  onClick={() =>
                                    removeSubject(year, semester, s.id)
                                  }
                                >
                                  <Trash2 className="size-3.5" />
                                </Button>
                              </div>
                            ))}
                          </div>
                          <MultiSearchableSelectWithTrigger
                            options={getSubjectsOptionsForYearAndSemester()}
                            value={[]}
                            onValueChange={(values: string[]) => {
                              values.forEach((val) =>
                                addSubjectToGrid(year, semester, Number(val))
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
                    ))}
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

            <div className="flex justify-end gap-3 pt-8 border-t">
              <Button variant="outline">Save as Draft</Button>
              <Button className="bg-primary text-primary-foreground">
                Finalize Curriculum
              </Button>
            </div>
          </div>
        ) : (
          <div className="flex flex-col items-center justify-center py-32 bg-muted/20 rounded-xl border-2 border-dashed">
            <BookOpen className="size-16 text-muted-foreground/20 mb-4" />
            <h2 className="text-xl font-semibold text-foreground">
              No Curriculum Started
            </h2>
            <p className="text-muted-foreground max-w-sm text-center mt-2">
              Click the button above to start designing a new academic
              curriculum.
            </p>
          </div>
        )}
      </div>
    </main>
  );
}

export default function CurriculumPage() {
  return (
    <Suspense fallback={null}>
      <CurriculumContent />
    </Suspense>
  );
}
