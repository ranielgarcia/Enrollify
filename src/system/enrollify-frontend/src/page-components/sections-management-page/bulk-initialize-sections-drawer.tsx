import {
  bulkInitializeSectionsOptions,
  type BulkInitializeClassSectionsRequest,
  type BulkInitializePayload,
} from "@/api/collections/class-section-collection";
import type { Course } from "@/api/models/course";
import type { Curriculum } from "@/api/models/curriculum";
import { FormSelectField } from "@/components/form/form-select-field";
import { FormSection } from "@/components/form/form-section";
import { FormDrawerFooter } from "@/components/form/form-drawer-footer";
import { Unauthorized } from "@/components/unauthorized";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { Button } from "@/components/ui/button";
import {
  Drawer,
  DrawerContent,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from "@/components/ui/drawer";
import { Separator } from "@/components/ui/separator";
import { Badge } from "@/components/ui/badge";
import { type FormMeta, defaultFormMeta } from "@/lib/form-meta";
import { useForm } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import { ListPlus, X } from "lucide-react";
import { z } from "zod";
import { useState } from "react";
import { Card } from "@/components/ui/card";
import { useEnrollmentContext } from "@/contexts/enrollment-context/enrollment-context";

const YEAR_LEVEL_OPTIONS = [
  { value: "1", label: "1st Year" },
  { value: "2", label: "2nd Year" },
  { value: "3", label: "3rd Year" },
  { value: "4", label: "4th Year" },
  { value: "5", label: "5th Year" },
  { value: "6", label: "6th Year" },
];

const bulkInitializeFormSchema = z.object({
  academicTermId: z.number().min(1, "Academic term is required"),
  yearLevel: z
    .number()
    .int()
    .min(1)
    .max(6, "Year level must be between 1 and 6"),
});

type BulkInitializeFormData = z.infer<typeof bulkInitializeFormSchema>;

interface CoursePayloadRow {
  course: Course;
  curriculum: Curriculum | null;
  numberOfSections: number;
}

interface BulkInitializeSectionsDrawerProps {
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  onOpenChange: (isOpen: boolean) => void;
  courses: Course[];
  curricula: Curriculum[];
}

export function BulkInitializeSectionsDrawer({
  isOpen,
  setIsOpen,
  onOpenChange,
  courses,
  curricula,
}: BulkInitializeSectionsDrawerProps) {
  const [selectedCourseRows, setSelectedCourseRows] = useState<
    CoursePayloadRow[]
  >([]);
  const [selectedCourseId, setSelectedCourseId] = useState<number>(0);

  const { selectedAcademicYear } = useEnrollmentContext();

  const { mutateAsync: bulkInitialize } = useMutation(
    bulkInitializeSectionsOptions(),
  );

  const termOptions =
    selectedAcademicYear?.academicTerms?.map((t) => ({
      value: t.id.toString(),
      label: t.termName ?? `Term ${t.termNumber}`,
    })) ?? [];

  const availableCourses = courses.filter(
    (c) => !selectedCourseRows.some((r) => r.course.id === c.id),
  );

  const courseOptions = availableCourses.map((c) => ({
    value: c.id.toString(),
    label: `${c.code} — ${c.name}`,
  }));

  const defaultValues: BulkInitializeFormData = {
    academicTermId: 0,
    yearLevel: 1,
  };

  const form = useForm({
    defaultValues,
    validators: {
      onBlur: bulkInitializeFormSchema,
      onSubmit: bulkInitializeFormSchema,
    },
    onSubmitMeta: defaultFormMeta as FormMeta,
    onSubmit: async ({ value, meta }) => {
      if (meta.submitAction === "create") {
        const payloads: BulkInitializePayload[] = selectedCourseRows.map(
          (row) => ({
            courseId: row.course.id,
            curriculumId: row.curriculum?.id ?? 0,
            numberOfSections: row.numberOfSections,
          }),
        );

        const request: BulkInitializeClassSectionsRequest = {
          academicTermId: value.academicTermId,
          yearLevel: value.yearLevel,
          requestPayload: payloads,
        };

        await bulkInitialize(request);
      }

      if (meta.formAction === "close") {
        setIsOpen(false);
        setSelectedCourseRows([]);
        setSelectedCourseId(0);
      }
      form.reset();
    },
  });

  const handleAddCourse = () => {
    if (selectedCourseId === 0) return;

    const course = courses.find((c) => c.id === selectedCourseId);
    if (!course) return;

    // Find latest active curriculum for this course
    const latestCurriculum = curricula
      .filter((cur) => cur.courseId === course.id && cur.statusId === 2) // statusId 2 = Active
      .sort((a, b) => (b.id ?? 0) - (a.id ?? 0))[0];

    setSelectedCourseRows([
      ...selectedCourseRows,
      {
        course,
        curriculum: latestCurriculum ?? null,
        numberOfSections: 1,
      },
    ]);
    setSelectedCourseId(0);
  };

  const handleRemoveCourse = (courseId: number) => {
    setSelectedCourseRows(
      selectedCourseRows.filter((r) => r.course.id !== courseId),
    );
  };

  const handleUpdateSectionCount = (courseId: number, count: number) => {
    setSelectedCourseRows(
      selectedCourseRows.map((r) =>
        r.course.id === courseId ? { ...r, numberOfSections: count } : r,
      ),
    );
  };

  return (
    <Drawer
      direction="right"
      dismissible={false}
      open={isOpen}
      onOpenChange={onOpenChange}
    >
      <DrawerTrigger asChild>
        <Button
          className="gap-2 bg-indigo-600 text-white hover:bg-indigo-700 cursor-pointer"
          size="sm"
        >
          <ListPlus className="size-4" />
          Bulk Initialize Sections
        </Button>
      </DrawerTrigger>

      <DrawerContent className="data-[vaul-drawer-direction=right]:w-[480px] data-[vaul-drawer-direction=right]:sm:max-w-none">
        <AuthorizeView
          policy="canCreateClassSection"
          unauthorized={
            <Unauthorized
              buttonLabel="Go Home"
              message="You don't have permission to bulk initialize class sections."
            />
          }
        >
          <form
            className="flex flex-col overflow-hidden h-full"
            onSubmit={(e) => {
              e.preventDefault();
              e.stopPropagation();
            }}
          >
            <DrawerHeader className="border-b pb-4">
              <DrawerTitle>
                Bulk Initialize Class Sections for{" "}
                {selectedAcademicYear?.academicYearTitle ??
                  "<invalid academic year...>"}
              </DrawerTitle>
              <p className="text-sm text-muted-foreground">
                Create multiple class sections for one academic term and year
                level
              </p>
            </DrawerHeader>

            <div className="flex-1 overflow-y-auto px-4 py-5 space-y-6">
              <FormSection title="Term & Year Level">
                <form.Field name="academicTermId">
                  {(field) => (
                    <FormSelectField
                      field={field}
                      label="Academic Term"
                      options={termOptions}
                      placeholder="Select academic term"
                      required
                      hint="Select the term when these sections will be active"
                    />
                  )}
                </form.Field>

                <form.Field name="yearLevel">
                  {(field) => (
                    <FormSelectField
                      field={field}
                      label="Year Level"
                      options={YEAR_LEVEL_OPTIONS}
                      placeholder="Select year level"
                      required
                      hint="All sections will be created for this year level"
                    />
                  )}
                </form.Field>
              </FormSection>

              <Separator />

              <FormSection title="Courses & Sections">
                <div className="space-y-4">
                  {/* Course Selector */}
                  <div className="flex gap-2">
                    <div className="flex-1">
                      <label className="text-sm font-medium mb-1.5 block">
                        Add Course
                      </label>
                      <select
                        className="w-full border border-gray-300 rounded-md px-3 py-2 text-sm"
                        value={selectedCourseId}
                        onChange={(e) =>
                          setSelectedCourseId(Number(e.target.value))
                        }
                      >
                        <option value="0">Select a course...</option>
                        {courseOptions.map((opt) => (
                          <option key={opt.value} value={opt.value}>
                            {opt.label}
                          </option>
                        ))}
                      </select>
                    </div>
                    <Button
                      type="button"
                      onClick={handleAddCourse}
                      disabled={selectedCourseId === 0}
                      className="self-end"
                      size="sm"
                    >
                      Add
                    </Button>
                  </div>

                  {/* Selected Courses List */}
                  {selectedCourseRows.length === 0 ? (
                    <Card className="p-6 text-center text-sm text-muted-foreground border-dashed">
                      No courses selected. Add at least one course to continue.
                    </Card>
                  ) : (
                    <div className="space-y-3">
                      {selectedCourseRows.map((row) => (
                        <Card key={row.course.id} className="p-4">
                          <div className="flex items-start justify-between gap-4">
                            <div className="flex-1 space-y-2">
                              <div className="flex items-center gap-2">
                                <span className="font-semibold text-sm">
                                  {row.course.code}
                                </span>
                                <span className="text-sm text-muted-foreground">
                                  — {row.course.name}
                                </span>
                              </div>

                              {row.curriculum ? (
                                <div className="flex items-center gap-2">
                                  <Badge variant="outline" className="text-xs">
                                    Curriculum: {row.curriculum.version}
                                  </Badge>
                                  <span className="text-xs text-muted-foreground">
                                    (Auto-selected latest active)
                                  </span>
                                </div>
                              ) : (
                                <Badge variant="destructive" className="text-xs">
                                  No active curriculum found
                                </Badge>
                              )}

                              <div className="flex items-center gap-2">
                                <label className="text-xs font-medium">
                                  Number of Sections:
                                </label>
                                <input
                                  type="number"
                                  min="1"
                                  max="26"
                                  value={row.numberOfSections}
                                  onChange={(e) =>
                                    handleUpdateSectionCount(
                                      row.course.id,
                                      Number(e.target.value),
                                    )
                                  }
                                  className="w-20 border border-gray-300 rounded px-2 py-1 text-xs"
                                />
                                <span className="text-xs text-muted-foreground">
                                  (Max 26: A-Z)
                                </span>
                              </div>
                            </div>

                            <Button
                              type="button"
                              variant="ghost"
                              size="sm"
                              onClick={() => handleRemoveCourse(row.course.id)}
                              className="text-destructive hover:text-destructive"
                            >
                              <X className="size-4" />
                            </Button>
                          </div>
                        </Card>
                      ))}
                    </div>
                  )}

                  <p className="text-xs text-muted-foreground">
                    Tip: The system will auto-generate section codes (A, B, C...)
                    and create subject offerings based on the curriculum.
                  </p>
                </div>
              </FormSection>
            </div>

            <FormDrawerFooter
              form={form}
              isUpdate={false}
              onCancel={() => setIsOpen(false)}
              entityLabel="Class Sections"
            />
          </form>
        </AuthorizeView>
      </DrawerContent>
    </Drawer>
  );
}
