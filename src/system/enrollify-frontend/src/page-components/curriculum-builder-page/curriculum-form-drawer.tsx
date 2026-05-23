import type { CurriculumWithSubjects } from "@/api/models/curriculum";
import { toast } from "sonner";
import z from "zod";
import { useForm } from "@tanstack/react-form";
import {
  createDraftCurriculumOptions,
  updateCurriculumOptions,
} from "@/api/collections/curriculum-collection";
import { useMutation } from "@tanstack/react-query";
import { defaultFormMeta, type FormMeta } from "@/lib/form-meta";
import {
  Drawer,
  DrawerContent,
  DrawerDescription,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from "@/components/ui/drawer";
import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { Unauthorized } from "@/components/unauthorized";
import { FormDrawerFooter } from "@/components/form/form-drawer-footer";
import { FormSelectField } from "@/components/form/form-select-field";
import { FormSection } from "@/components/form/form-section";
import { Separator } from "@/components/ui/separator";
import type { Course } from "@/api/models/course";
import type { SearchableSelectOption } from "@/components/form/searchable-select";
import { FormField } from "@/components/form/form-field";

const curriculaFormSchema = z.object({
  courseId: z.number().min(1, "Course is required"),
  effectiveYear: z
    .number()
    .min(2020, "Effective Year must be a valid year")
    .max(
      new Date().getFullYear() + 1,
      "Effective Year cannot be in the distant future",
    ),
  version: z.string().min(1, "Version Identifier is required"),
  description: z.string().nullable(),
});

type CurriculaFormData = z.infer<typeof curriculaFormSchema>;

interface CurriculumFormDrawerProps {
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  curriculumToUpdate?: CurriculumWithSubjects | null;
  onOpenChange: (isOpen: boolean) => void;
  courses: Course[];
  disabled?: boolean;
}

export function CurriculumFormDrawer({
  isOpen,
  setIsOpen,
  curriculumToUpdate,
  onOpenChange,
  courses,
  disabled = false,
}: CurriculumFormDrawerProps) {
  const defaultFormValues: CurriculaFormData = {
    courseId: curriculumToUpdate?.course.id ?? 0,
    effectiveYear:
      curriculumToUpdate?.effectiveYear ?? new Date().getFullYear(),
    version: curriculumToUpdate?.version ?? "",
    description: curriculumToUpdate?.description ?? "",
  };

  const { mutateAsync: createDraftCurriculumAsync } = useMutation(
    createDraftCurriculumOptions(),
  );

  const { mutateAsync: updateCurriculumAsync } = useMutation(
    updateCurriculumOptions(curriculumToUpdate?.id ?? 0),
  );

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onBlur: curriculaFormSchema,
      onSubmit: curriculaFormSchema,
    },
    onSubmitMeta: defaultFormMeta as FormMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = curriculaFormSchema.parse(value);

      if (meta.submitAction === "create") {
        await createDraftCurriculumAsync(formValues);
        toast.success("Curriculum created successfully");
      }

      if (meta.submitAction === "update" && curriculumToUpdate) {
        await updateCurriculumAsync(formValues);
        toast.success("Curriculum updated successfully");
      }

      if (meta.formAction === "close") {
        setIsOpen(false);
      }
      form.reset();
    },
  });

  const coursesOptions: SearchableSelectOption[] = courses.map((c) => ({
    value: c.id.toString(),
    label: c.name,
  }));

  const isUpdating = !!curriculumToUpdate;

  return (
    <Drawer
      direction="right"
      dismissible={false}
      open={isOpen}
      onOpenChange={onOpenChange}
    >
      <DrawerTrigger asChild>
        <Button
          className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90 cursor-pointer"
          size="sm"
          disabled={disabled}
        >
          <Plus className="size-4" />
          Create New Curriculum
        </Button>
      </DrawerTrigger>
      <DrawerContent className="data-[vaul-drawer-direction=right]:w-[480px] data-[vaul-drawer-direction=right]:sm:max-w-none">
        <AuthorizeView
          policy={isUpdating ? "canUpdateCurriculum" : "canCreateCurriculum"}
          unauthorized={
            <Unauthorized
              message="Your current role does not have the necessary permissions to create/update curriculum."
              buttonLabel="Back to Home"
              backCallback={() => setIsOpen(false)}
              redirectOptions={{
                to: "/portal",
              }}
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
                {isUpdating ? "Update" : "New"} Curriculum
              </DrawerTitle>
              <DrawerDescription>
                Fill in the details below to{" "}
                {isUpdating ? "update this" : "create a new"} curriculum.
              </DrawerDescription>
            </DrawerHeader>
            <div className="flex-1 overflow-y-auto px-4 py-5 space-y-6">
              <FormSection title="Course & Year">
                <form.Field
                  name="courseId"
                  children={(field) => (
                    <FormSelectField
                      field={field}
                      label="Course"
                      required
                      options={coursesOptions}
                      placeholder="Select Course"
                      searchPlaceholder="Search Course..."
                      emptyMessage="No Course found"
                    />
                  )}
                />

                <form.Field
                  name="effectiveYear"
                  children={(field) => (
                    <FormField
                      field={field}
                      label="Effective Year"
                      type="number"
                      required
                    />
                  )}
                />
              </FormSection>

              <Separator />

              <FormSection title="Identification">
                <form.Field
                  name="version"
                  children={(field) => (
                    <FormField
                      field={field}
                      label="Version Identifier"
                      required
                      placeholder="e.g., 2024-A"
                    />
                  )}
                />
              </FormSection>

              <Separator />

              <FormSection title="Details">
                <form.Field
                  name="description"
                  children={(field) => (
                    <FormField
                      field={field}
                      label="Description"
                      type="textarea"
                      placeholder="Curriculum details..."
                    />
                  )}
                />
              </FormSection>
            </div>

            <FormDrawerFooter
              form={form}
              isUpdate={isUpdating}
              onCancel={() => setIsOpen(false)}
              entityLabel="Curriculum"
              showSaveAndAddAnother
            />
          </form>
        </AuthorizeView>
      </DrawerContent>
    </Drawer>
  );
}
