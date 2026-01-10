import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Loader2, X } from "lucide-react";
import {
  SearchableSelect,
  type SearchableSelectOption,
} from "@/components/searchable-select";
import { Label } from "@/components/ui/label";
import { useMutation, useSuspenseQuery } from "@tanstack/react-query";
import { getAllCoursesOptions } from "@/api/collections/course-collection";
import { useForm } from "@tanstack/react-form";
import z from "zod";
import {
  createDraftCurriculumOptions,
  updateCurriculumOptions,
} from "@/api/collections/curriculum-collection";
import type { Curriculum } from "@/api/models/curriculum";

type FormMeta = {
  submitAction: "create" | "update" | null;
  formAction: "close" | "stayopen" | null;
};
// Metadata is not required to call form.handleSubmit().
// Specify what values to use as default if no meta is passed
const defaultMeta: FormMeta = {
  submitAction: null,
  formAction: null,
};

const curriculaFormSchema = z.object({
  courseId: z.number().min(1, "Course is required"),
  effectiveYear: z
    .number()
    .min(2020, "Effective Year must be a valid year")
    .max(
      new Date().getFullYear() + 1,
      "Effective Year cannot be in the distant future"
    ),
  version: z.string().min(1, "Version Identifier is required"),
  description: z.string().nullable(),
});

type CurriculaFormData = z.infer<typeof curriculaFormSchema>;

interface CurriculumFormProps {
  onClose: () => void;
  onSave: (curriculumId: number) => void;
  curriculumToUpdate?: Curriculum | null;
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
    createDraftCurriculumOptions()
  );

  const { mutateAsync: updateCurriculumAsync } = useMutation(
    updateCurriculumOptions(curriculumToUpdate?.id ?? 0)
  );

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onChange: curriculaFormSchema,
    },
    onSubmitMeta: defaultMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = curriculaFormSchema.parse(value);

      if (meta.submitAction === "create") {
        const curriculumId = await createDraftCurriculumAsync(formValues);
        onSave(curriculumId);
      }

      if (meta.submitAction === "update" && curriculumToUpdate) {
        await updateCurriculumAsync(formValues);
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
        <CardTitle>Create New Curriculum</CardTitle>
        <Button variant="ghost" size="sm" onClick={onClose}>
          <X className="size-4" />
        </Button>
      </CardHeader>
      <CardContent>
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
                <div>
                  <Label htmlFor={field.name}>Course:</Label>
                  <SearchableSelect
                    options={coursesOptions}
                    value={field.state.value.toString()}
                    onValueChange={(val) => field.handleChange(Number(val))}
                    name={field.name}
                    placeholder="Select Course"
                    searchPlaceholder="Search Course..."
                    emptyMessage="No Course found"
                  />
                  {!field.state.meta.isValid && (
                    <em role="alert" className="text-red-800">
                      {field.state.meta.errors
                        .map((e) => e?.message)
                        .join(", ")}
                    </em>
                  )}
                </div>
              )}
            />

            <form.Field
              name="effectiveYear"
              children={(field) => (
                <div>
                  <Label htmlFor={field.name}>Effective Year:</Label>
                  <Input
                    name="effectiveYear"
                    type="number"
                    value={field.state.value}
                    onChange={(e) => field.handleChange(Number(e.target.value))}
                    required
                    id={field.name}
                  />
                  {!field.state.meta.isValid && (
                    <em role="alert" className="text-red-800">
                      {field.state.meta.errors
                        .map((e) => e?.message)
                        .join(", ")}
                    </em>
                  )}
                </div>
              )}
            />

            <form.Field
              name="version"
              children={(field) => (
                <div>
                  <Label
                    className="text-sm font-medium block mb-1"
                    htmlFor={field.name}
                  >
                    Version Identifier:
                  </Label>
                  <Input
                    placeholder="e.g., 2024-A"
                    value={field.state.value}
                    onChange={(e) => field.handleChange(e.target.value)}
                    required
                    id={field.name}
                  />
                </div>
              )}
            />

            <form.Field
              name="description"
              children={(field) => (
                <div className="md:col-span-2">
                  <Label
                    className="text-sm font-medium block mb-1"
                    htmlFor="description"
                  >
                    Description
                  </Label>
                  <Textarea
                    placeholder="Curriculum details..."
                    value={field.state.value ?? undefined}
                    onChange={(e) => field.handleChange(e.target.value)}
                    rows={3}
                    id={field.name}
                  />
                </div>
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
                    <Loader2 />
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
      </CardContent>
    </Card>
  );
}
