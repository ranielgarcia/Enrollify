import { bulkInitializeClassSectionsOptions } from "@/api/collections/class-section-collection";
import type { Course } from "@/api/models/course";
import { FormSelectField } from "@/components/form/form-select-field";
import { SearchableSelectWithCustomTrigger } from "@/components/form/searchable-select-with-custom-trigger";
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
import { type FormMeta, defaultFormMeta } from "@/lib/form-meta";
import { useForm } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import { BookOpen, ChevronDown, ListPlus, Minus, Plus, X } from "lucide-react";
import { z } from "zod";
import { useState, useEffect } from "react";
import { useEnrollmentContext } from "@/contexts/enrollment-context/enrollment-context";

const bulkInitializePayload = z.object({
  courseId: z.number(),
  numberOfSections: z.number().min(1).max(26),
});

const bulkInitializeFormSchema = z.object({
  academicTermId: z.number().min(1, "Academic term is required"),
  yearLevel: z
    .number()
    .int()
    .min(1)
    .max(6, "Year level must be between 1 and 6"),
  requestPayload: z
    .array(bulkInitializePayload)
    .min(1, "Add at least one course"),
});

type BulkInitializeFormData = z.infer<typeof bulkInitializeFormSchema>;

interface CoursePayloadRow {
  course: Course;
  numberOfSections: number;
}

interface BulkInitializeSectionsDrawerProps {
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  onOpenChange: (isOpen: boolean) => void;
  courses: Course[];
}

export function BulkInitializeSectionsDrawer({
  isOpen,
  setIsOpen,
  onOpenChange,
  courses,
}: BulkInitializeSectionsDrawerProps) {
  const [selectedCourseRows, setSelectedCourseRows] = useState<
    CoursePayloadRow[]
  >([]);

  const {
    selectedAcademicYear,
    academicCoreSettings: { yearLevelOptions },
  } = useEnrollmentContext();

  const { mutateAsync: bulkInitialize } = useMutation(
    bulkInitializeClassSectionsOptions(),
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
    requestPayload: [],
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
        await bulkInitialize({
          academicTermId: value.academicTermId,
          yearLevel: value.yearLevel,
          requestPayload: value.requestPayload,
        });
      }

      if (meta.formAction === "close") {
        setIsOpen(false);
        setSelectedCourseRows([]);
      }
      form.reset();
    },
  });

  useEffect(() => {
    form.setFieldValue(
      "requestPayload",
      selectedCourseRows.map((row) => ({
        courseId: row.course.id,
        numberOfSections: row.numberOfSections,
      })),
    );
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedCourseRows]);

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
                      options={yearLevelOptions.map((o) => ({
                        value: o.value.toString(),
                        label: o.label,
                      }))}
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
                  <div>
                    <label className="text-sm font-medium mb-1.5 block">
                      Add Course
                    </label>
                    <SearchableSelectWithCustomTrigger
                      options={courseOptions}
                      searchPlaceholder="Search courses..."
                      emptyMessage="No courses found."
                      value={[]}
                      onValueChange={(vals) => {
                        if (vals.length === 0) return;
                        const courseId = Number(vals[vals.length - 1]);
                        const course = courses.find((c) => c.id === courseId);
                        if (!course) return;
                        setSelectedCourseRows((prev) => [
                          ...prev,
                          { course, numberOfSections: 1 },
                        ]);
                      }}
                      trigger={
                        <div className="w-full border border-input rounded-md px-3 py-2 text-sm flex items-center justify-between cursor-pointer bg-background hover:bg-accent">
                          <span className="text-muted-foreground">
                            Select a course...
                          </span>
                          <ChevronDown className="size-4 opacity-50" />
                        </div>
                      }
                    />
                  </div>

                  {/* Selected Courses List */}
                  {selectedCourseRows.length === 0 ? (
                    <div className="flex flex-col items-center justify-center gap-3 rounded-xl border border-dashed bg-muted/20 py-8 text-center">
                      <div className="flex h-12 w-12 items-center justify-center rounded-xl border border-dashed bg-muted/60">
                        <BookOpen className="h-5 w-5 text-muted-foreground/50" />
                      </div>
                      <div className="space-y-1">
                        <p className="text-sm font-medium text-muted-foreground">
                          No courses added yet
                        </p>
                        <p className="text-xs text-muted-foreground/70">
                          Select a course above to get started
                        </p>
                      </div>
                    </div>
                  ) : (
                    <div className="space-y-2">
                      <p className="px-0.5 text-xs font-semibold uppercase tracking-widest text-muted-foreground">
                        {selectedCourseRows.length}{" "}
                        {selectedCourseRows.length === 1 ? "course" : "courses"}{" "}
                        added
                      </p>
                      <div className="grid gap-2">
                        {selectedCourseRows.map((row, idx) => (
                          <div
                            key={row.course.id}
                            className="group flex items-center justify-between rounded-lg border bg-card px-4 py-3 transition-colors hover:bg-accent/40"
                          >
                            <div className="flex items-center gap-3">
                              <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-md bg-primary/10 text-xs font-bold text-primary">
                                {idx + 1}
                              </span>
                              <div className="space-y-0.5">
                                <div className="flex items-center gap-1.5">
                                  <span className="rounded bg-primary/10 px-1.5 py-0.5 text-xs font-semibold text-primary">
                                    {row.course.code}
                                  </span>
                                  <span className="text-sm font-medium leading-none">
                                    {row.course.name}
                                  </span>
                                </div>
                                <p className="text-xs text-muted-foreground">
                                  {row.numberOfSections}{" "}
                                  {row.numberOfSections === 1
                                    ? "section"
                                    : "sections"}{" "}
                                  will be created
                                </p>
                              </div>
                            </div>

                            <div className="flex items-center gap-2">
                              {/* +/- stepper */}
                              <div className="flex items-center gap-1 rounded-lg border bg-muted/40 px-1 py-0.5">
                                <button
                                  type="button"
                                  onClick={() =>
                                    handleUpdateSectionCount(
                                      row.course.id,
                                      Math.max(1, row.numberOfSections - 1),
                                    )
                                  }
                                  disabled={row.numberOfSections <= 1}
                                  className="flex h-6 w-6 items-center justify-center rounded text-muted-foreground transition-colors hover:bg-background hover:text-foreground disabled:opacity-40"
                                >
                                  <Minus className="size-3" />
                                </button>
                                <span className="w-5 text-center text-sm font-semibold tabular-nums">
                                  {row.numberOfSections}
                                </span>
                                <button
                                  type="button"
                                  onClick={() =>
                                    handleUpdateSectionCount(
                                      row.course.id,
                                      Math.min(26, row.numberOfSections + 1),
                                    )
                                  }
                                  disabled={row.numberOfSections >= 26}
                                  className="flex h-6 w-6 items-center justify-center rounded text-muted-foreground transition-colors hover:bg-background hover:text-foreground disabled:opacity-40"
                                >
                                  <Plus className="size-3" />
                                </button>
                              </div>

                              <Button
                                type="button"
                                variant="ghost"
                                size="sm"
                                onClick={() =>
                                  handleRemoveCourse(row.course.id)
                                }
                                className="h-8 w-8 p-0 text-muted-foreground opacity-0 transition-all hover:bg-destructive/10 hover:text-destructive group-hover:opacity-100"
                              >
                                <X className="size-4" />
                              </Button>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}

                  <p className="text-xs text-muted-foreground">
                    Tip: The system will auto-generate section codes (A, B,
                    C...) and create subject offerings based on the curriculum.
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
