import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Loader2, X } from "lucide-react";
import {
  type SearchableSelectOption,
} from "@/components/searchable-select";
import { useMutation, useSuspenseQuery } from "@tanstack/react-query";
import { getAllCoursesOptions } from "@/api/collections/course-collection";
import { useForm } from "@tanstack/react-form";
import z from "zod";
import {
  createDraftCurriculumOptions,
  updateCurriculumOptions,
} from "@/api/collections/curriculum-collection";
import type { CurriculumWithSubjects } from "@/api/models/curriculum";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { Unauthorized } from "@/components/unauthorized";
import { FormField } from "@/components/form-field";
import { FormSelectField } from "@/components/form-select-field";
import { type FormMeta, defaultFormMeta } from "@/lib/form-meta";
import { toast } from "sonner";

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

interface CurriculumFormProps {
  onClose: () => void;
  onSave: (curriculumId: number) => void;
  curriculumToUpdate?: CurriculumWithSubjects | null;
}

export function CurriculumForm({
  onClose,
  onSave,
  curriculumToUpdate,
}: CurriculumFormProps) {
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
        const curriculumId = await createDraftCurriculumAsync(formValues);
        toast.success("Curriculum created successfully");
        onSave(curriculumId);
      }

      if (meta.submitAction === "update" && curriculumToUpdate) {
        await updateCurriculumAsync(formValues);
        toast.success("Curriculum updated successfully");
      }

      form.reset();
    },
  });

  const { data: courses } = useSuspenseQuery(getAllCoursesOptions());

  const coursesOptions: SearchableSelectOption[] = courses.map((c) => ({
    value: c.id.toString(),
    label: c.name,
  }));

  const isUpdatingCurriculumBasicDetails = !!curriculumToUpdate;

  return (
    <Card className="border-2 border-primary">
      <CardHeader className="flex flex-row items-center justify-between space-y-0">
        <CardTitle>
          {isUpdatingCurriculumBasicDetails ? "Update" : "Create New"}{" "}
          Curriculum
        </CardTitle>
        <Button variant="ghost" size="sm" onClick={onClose}>
          <X className="size-4" />
        </Button>
      </CardHeader>
      <CardContent>
        <AuthorizeView
          policy={
            isUpdatingCurriculumBasicDetails
              ? "canUpdateCurriculum"
              : "canCreateCurriculum"
          }
          unauthorized={
            <Unauthorized
              message="Your current role does not have the necessary permissions to create/update curriculum."
              buttonLabel="Back to Home"
              backCallback={onClose}
              redirectOptions={{
                to: "/portal",
              }}
            />
          }
        >
          <form
            onSubmit={(e) => {
              e.preventDefault();
              e.stopPropagation();
            }}
            className="space-y-4"
          >
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
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

              <form.Field
                name="description"
                children={(field) => (
                  <FormField
                    field={field}
                    label="Description"
                    type="textarea"
                    placeholder="Curriculum details..."
                    className="md:col-span-2"
                  />
                )}
              />
            </div>

            <div className="flex gap-2 justify-end pt-4">
              <Button variant="outline" type="button" onClick={onClose}>
                Cancel
              </Button>
              <form.Subscribe
                selector={(state) => [state.canSubmit, state.isSubmitting]}
                children={([canSubmit, isSubmitting]) => (
                  <Button
                    type="submit"
                    disabled={!canSubmit}
                    onClick={() =>
                      form.handleSubmit({
                        submitAction: isUpdatingCurriculumBasicDetails
                          ? "update"
                          : "create",
                        formAction: "close",
                      })
                    }
                  >
                    {isSubmitting ? (
                      <Loader2 className="size-4 animate-spin" />
                    ) : isUpdatingCurriculumBasicDetails ? (
                      "Update Curriculum"
                    ) : (
                      "Create Curriculum"
                    )}
                  </Button>
                )}
              />
            </div>
          </form>
        </AuthorizeView>
      </CardContent>
    </Card>
  );
}
