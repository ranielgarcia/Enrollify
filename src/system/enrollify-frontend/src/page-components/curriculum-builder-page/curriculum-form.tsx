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
import { useSuspenseQuery } from "@tanstack/react-query";
import { getAllCoursesOptions } from "@/api/collections/course-collection";
import { useForm } from "@tanstack/react-form";
import {
  curriculaFormSchema,
  type CurriculaFormData,
} from "./models/curricula-form-schema";

interface CurriculumFormProps {
  onClose: () => void;
  onSave: (curriculum: CurriculaFormData) => void;
}

export function CurriculumForm({ onClose, onSave }: CurriculumFormProps) {
  const defaultFormValues: CurriculaFormData = {
    courseId: 0,
    effectiveYear: new Date().getFullYear(),
    version: "",
    description: "",
  };

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onChange: curriculaFormSchema,
    },
    onSubmit: async ({ value }) => {
      const formValues = curriculaFormSchema.parse(value);
      onSave(formValues);
      form.reset();
    },
  });

  const { data: courses } = useSuspenseQuery(getAllCoursesOptions());

  const coursesOptions: SearchableSelectOption[] = courses.map((c) => ({
    value: c.id.toString(),
    label: c.name,
  }));

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
                    value={field.state.value}
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
                  onClick={() => form.handleSubmit()}
                >
                  {isSubmitting ? <Loader2 /> : "Create Curriculum"}
                </Button>
              )}
            />
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
