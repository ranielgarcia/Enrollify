import type { ClassSectionMinimal } from "@/api/models/class-scheduling/class-section";
import type { Course } from "@/api/models/course";
import type { Teacher } from "@/api/models/teacher";
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
} from "@/components/ui/drawer";
import { type FormMeta, defaultFormMeta } from "@/lib/form-meta";
import { useForm } from "@tanstack/react-form";
import { TriangleAlert, UserRound } from "lucide-react";
import { useState } from "react";
import { z } from "zod";
import { useEnrollmentContext } from "@/contexts/enrollment-context/enrollment-context";
import {
  createNewClassSectionsOptions,
  updateClassSectionOptions,
} from "@/api/collections/class-section-collection";
import { useMutation } from "@tanstack/react-query";
import { Alert, AlertDescription } from "@/components/ui/alert";

const editSectionFormSchema = z.object({
  yearLevel: z.number().int().min(1).max(6),
  courseId: z.number().min(1, "Course is required"),
  academicTermId: z.number().min(1, "Academic term is required"),
  adviserId: z.number().min(1, "Adviser is required"),
});

type EditSectionFormData = z.infer<typeof editSectionFormSchema>;

interface EditSectionFormDrawerProps {
  isOpen: boolean;
  sectionToUpdate?: ClassSectionMinimal | null;
  onOpenChange: (isOpen: boolean) => void;
  courses: Course[];
}

export function EditSectionFormDrawer({
  isOpen,
  sectionToUpdate,
  onOpenChange,
  courses,
}: EditSectionFormDrawerProps) {
  console.log(courses);

  const isUpdating = !!sectionToUpdate;
  const {
    selectedAcademicTerm,
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
  const { mutateAsync: updateSectionAsync } = useMutation(
    updateClassSectionOptions(sectionToUpdate?.id ?? 0),
  );

  const courseOptions = courses.map((c) => ({
    value: c.id.toString(),
    label: `${c.code} — ${c.name}`,
  }));

  const defaultValues: EditSectionFormData = {
    yearLevel: sectionToUpdate?.intendedYearLevel ?? 1,
    courseId: sectionToUpdate?.courseId ?? 0,
    academicTermId: selectedAcademicTerm?.id ?? 0,
    adviserId: sectionToUpdate?.adviser?.id ?? 0,
  };

  const form = useForm({
    defaultValues,
    validators: {
      onBlur: editSectionFormSchema,
      onSubmit: editSectionFormSchema,
    },
    onSubmitMeta: defaultFormMeta as FormMeta,
    onSubmit: async ({ value, meta }) => {
      if (meta.submitAction === "create") {
        await createSectionAsync(value);
      } else if (meta.submitAction === "update") {
        await updateSectionAsync({ adviserId: value.adviserId });
      }
      if (meta.formAction === "close") {
        onOpenChange(false);
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
      <AuthorizeView
        policy={isUpdating ? "canUpdateClassSection" : "canCreateClassSection"}
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
                ? `Edit "${sectionToUpdate.fullName}"`
                : "Create New Class Section"}
            </DrawerTitle>
          </DrawerHeader>

          {!selectedAcademicTerm && (
            <div className="flex flex-wrap items-center gap-x-3 gap-y-2 py-3">
              <div className="flex min-w-0 flex-1 flex-wrap items-center gap-2">
                <Alert
                  variant="destructive"
                  className="flex h-9 items-center gap-2 py-0"
                >
                  <TriangleAlert className="size-4" />
                  <AlertDescription className="text-xs font-medium">
                    Select an academic term to create a class section.
                  </AlertDescription>
                </Alert>
              </div>
            </div>
          )}

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
                    disabled={isUpdating}
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
                    disabled={isUpdating}
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
            onCancel={() => onOpenChange(false)}
            entityLabel="Section"
            showSaveAndAddAnother
          />
        </DrawerContent>
      </AuthorizeView>
    </Drawer>
  );
}
