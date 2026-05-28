import type { ClassSection } from "@/api/models/class-section";
import type { Course } from "@/api/models/course";
import type { Teacher } from "@/api/models/teacher";
import { FormField } from "@/components/form/form-field";
import { FormSelectField } from "@/components/form/form-select-field";
import { FormSection } from "@/components/form/form-section";
import { FormDrawerFooter } from "@/components/form/form-drawer-footer";
import { Unauthorized } from "@/components/unauthorized";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { SearchTeachersDialog } from "@/components/shared/search-teachers-dialog";
import {
  Drawer,
  DrawerContent,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from "@/components/ui/drawer";
import { type FormMeta, defaultFormMeta } from "@/lib/form-meta";
import { useForm } from "@tanstack/react-form";
import { Plus, UserRound } from "lucide-react";
import { useState } from "react";
import { z } from "zod";
import { useEnrollmentContext } from "@/contexts/enrollment-context/enrollment-context";
import { createNewClassSectionsOptions } from "@/api/collections/class-section-collection";
import { useMutation } from "@tanstack/react-query";

const sectionFormSchema = z.object({
  yearLevel: z.number().int().min(1).max(6),
  courseId: z.number().min(1, "Course is required"),
  academicTermId: z.number().min(1, "Academic term is required"),
  adviserId: z.number().min(1, "Adviser is required"),
  studentCapacity: z.number().int().min(1, "Capacity must be at least 1"),
});

type SectionFormData = z.infer<typeof sectionFormSchema>;

interface SectionFormDrawerProps {
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  sectionToUpdate?: ClassSection | null;
  onOpenChange: (isOpen: boolean) => void;
  courses: Course[];
}

export function SectionFormDrawer({
  isOpen,
  setIsOpen,
  sectionToUpdate,
  onOpenChange,
  courses,
}: SectionFormDrawerProps) {
  const isUpdating = !!sectionToUpdate;
  const {
    selectedAcademicYear,
    academicCoreSettings: { yearLevelOptions },
  } = useEnrollmentContext();

  const [isAdviserDialogOpen, setIsAdviserDialogOpen] = useState(false);
  const [selectedAdviser, setSelectedAdviser] = useState<Pick<
    Teacher,
    "id" | "firstName" | "lastName" | "teacherIdentifier"
  > | null>(
    sectionToUpdate?.adviser
      ? {
          id: sectionToUpdate.adviser.id,
          firstName: sectionToUpdate.adviser.firstName,
          lastName: sectionToUpdate.adviser.lastName,
          teacherIdentifier: "",
        }
      : null,
  );

  const { mutateAsync: createSectionAsync } = useMutation(
    createNewClassSectionsOptions(),
  );
  // const { mutateAsync: updateSection } = useMutation(
  //   updateSectionOptions(sectionToUpdate?.id ?? 0),
  // );

  const courseOptions = courses.map((c) => ({
    value: c.id.toString(),
    label: `${c.code} — ${c.name}`,
  }));

  const termOptions = selectedAcademicYear?.academicTerms
    ? selectedAcademicYear?.academicTerms?.map((t) => ({
        value: t.id.toString(),
        label: t.termName ?? "<Invalid term name...>",
      }))
    : [];

  const defaultValues: SectionFormData = {
    yearLevel: sectionToUpdate?.intendedYearLevel ?? 1,
    studentCapacity: 40,
    courseId: sectionToUpdate?.course?.id ?? 0,
    academicTermId: sectionToUpdate?.academicTerm?.id ?? 0,
    adviserId: sectionToUpdate?.adviser?.id ?? 0,
  };

  const form = useForm({
    defaultValues,
    validators: {
      onBlur: sectionFormSchema,
      onSubmit: sectionFormSchema,
    },
    onSubmitMeta: defaultFormMeta as FormMeta,
    onSubmit: async ({ value, meta }) => {
      if (meta.submitAction === "create") {
        await createSectionAsync(value);
      } else if (meta.submitAction === "update") {
        // await updateSection(value);
        console.log(value);
      }
      if (meta.formAction === "close") {
        setIsOpen(false);
      }
      form.reset();
    },
  });

  return (
    <Drawer
      open={isOpen}
      onOpenChange={onOpenChange}
      direction="right"
      dismissible={false}
    >
      <DrawerTrigger asChild>
        <Button
          className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90 cursor-pointer"
          size="sm"
        >
          <Plus className="size-4" />
          New Section
        </Button>
      </DrawerTrigger>

      <AuthorizeView
        policy="canCreateClassSection"
        unauthorized={
          <Unauthorized
            buttonLabel="Go Home"
            message="You don't have permission to create class sections."
          />
        }
      >
        <DrawerContent className="data-[vaul-drawer-direction=right]:w-[480px] data-[vaul-drawer-direction=right]:sm:max-w-none h-full overflow-y-auto overflow-x-hidden">
          <DrawerHeader className="border-b pb-4">
            <DrawerTitle>
              {isUpdating
                ? `Edit "${sectionToUpdate.name}"`
                : "Create New Class Section"}
            </DrawerTitle>
          </DrawerHeader>

          <form
            className="flex flex-col gap-6 p-4"
            onSubmit={(e) => e.preventDefault()}
          >
            <FormSection title="Section Details">
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
                  />
                )}
              </form.Field>

              <form.Field name="studentCapacity">
                {(field) => (
                  <FormField
                    field={field}
                    label="Student Capacity"
                    type="number"
                    placeholder="40"
                    required
                    hint="Maximum number of students"
                  />
                )}
              </form.Field>
            </FormSection>

            <FormSection title="Assignment">
              <form.Field name="courseId">
                {(field) => (
                  <FormSelectField
                    field={field}
                    label="Course / Program"
                    options={courseOptions}
                    placeholder="Select course"
                    searchPlaceholder="Search courses..."
                    required
                  />
                )}
              </form.Field>

              <form.Field name="academicTermId">
                {(field) => (
                  <FormSelectField
                    field={field}
                    label="Academic Term"
                    options={termOptions}
                    placeholder="Select term"
                    searchPlaceholder="Search terms..."
                    required
                  />
                )}
              </form.Field>

              <form.Field name="adviserId">
                {(field) => {
                  const errorMessage = field.state.meta.errors
                    // eslint-disable-next-line @typescript-eslint/no-explicit-any
                    .map((e: any) => (typeof e === "string" ? e : e?.message))
                    .filter(Boolean)
                    .join(", ");
                  const hasError = !field.state.meta.isValid && errorMessage;
                  return (
                    <div className="grid w-full items-center gap-1.5">
                      <Label htmlFor={field.name}>
                        Adviser
                        <span className="text-destructive ml-0.5">*</span>
                      </Label>
                      <Button
                        id={field.name}
                        type="button"
                        variant="outline"
                        className="justify-start font-normal"
                        onClick={() => setIsAdviserDialogOpen(true)}
                      >
                        <UserRound className="size-4 text-muted-foreground" />
                        {selectedAdviser ? (
                          `${selectedAdviser.firstName} ${selectedAdviser.lastName}`
                        ) : (
                          <span className="text-muted-foreground">
                            Select adviser
                          </span>
                        )}
                      </Button>
                      {hasError && (
                        <p className="text-sm text-destructive">
                          {errorMessage}
                        </p>
                      )}
                    </div>
                  );
                }}
              </form.Field>

              <SearchTeachersDialog
                isOpen={isAdviserDialogOpen}
                onOpenChange={setIsAdviserDialogOpen}
                title="Select Adviser"
                maxSelections={1}
                excludeTeacherIds={selectedAdviser ? [selectedAdviser.id] : []}
                onSubmit={async (teachers) => {
                  const teacher = teachers[0];
                  setSelectedAdviser(teacher);
                  form.setFieldValue("adviserId", teacher.id);
                }}
              />
            </FormSection>
          </form>

          <FormDrawerFooter
            form={form}
            isUpdate={isUpdating}
            onCancel={() => setIsOpen(false)}
            entityLabel="Section"
            showSaveAndAddAnother
          />
        </DrawerContent>
      </AuthorizeView>
    </Drawer>
  );
}
