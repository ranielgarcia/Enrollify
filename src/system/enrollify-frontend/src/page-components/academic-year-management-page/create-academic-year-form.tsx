import {
  createAcademicYearOptions,
  updateAcademicYearOptions,
} from "@/api/collections/academic-year-collection";
import type { AcademicYear } from "@/api/models/academic-year";
import { FormDatePicker } from "@/components/form/form-date-picker";
import { FormSection } from "@/components/form/form-section";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { Unauthorized } from "@/components/unauthorized";
import { type FormMeta, defaultFormMeta } from "@/lib/form-meta";
import { useForm } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import { Loader2 } from "lucide-react";
import z from "zod";
import { useSystemSettingsContext } from "@/infrastructure/system-settings/system-settings-context";

// ---------- Zod schemas ----------

const termFormSchema = z.object({
  termNumber: z.number().min(1, "Term number must be at least 1"),
  startDate: z.string().min(1, "Start date is required"),
  endDate: z.string().min(1, "End date is required"),
});

const academicYearFormSchema = z.object({
  startDate: z.string().min(1, "Start date is required"),
  endDate: z.string().min(1, "End date is required"),
  terms: z.array(termFormSchema).min(1, "At least one term is required"),
});

type AcademicYearFormData = z.infer<typeof academicYearFormSchema>;

// ---------- Props ----------

interface CreateAcademicYearFormProps {
  yearToEdit?: AcademicYear | null;
  onSuccess?: () => void;
  onCancel?: () => void;
}

// ---------- Component ----------

export function CreateAcademicYearForm({
  yearToEdit,
  onSuccess,
  onCancel,
}: CreateAcademicYearFormProps) {
  const systemSettings = useSystemSettingsContext();
  const numberOfSemesters = systemSettings.academicSettings.academicSystem;

  const isUpdating = !!yearToEdit;

  const defaultTerms = yearToEdit?.academicTerms?.length
    ? yearToEdit.academicTerms
        .slice()
        .sort((a, b) => (a.termNumber ?? 0) - (b.termNumber ?? 0))
        .map((t) => ({
          termNumber: t.termNumber ?? 1,
          startDate: t.startDate ?? "",
          endDate: t.endDate ?? "",
        }))
    : Array.from({ length: numberOfSemesters }, (_, i) => ({
        termNumber: i + 1,
        startDate: "",
        endDate: "",
      }));

  const defaultFormValues: AcademicYearFormData = {
    startDate: yearToEdit?.startDate ?? "",
    endDate: yearToEdit?.endDate ?? "",
    terms: defaultTerms,
  };

  const { mutateAsync: createAsync, isPending: isCreating } = useMutation(
    createAcademicYearOptions(),
  );
  const { mutateAsync: updateAsync, isPending: isUpdatingPending } =
    useMutation(updateAcademicYearOptions(yearToEdit?.id ?? 0));

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onBlur: academicYearFormSchema,
      onSubmit: academicYearFormSchema,
    },
    onSubmitMeta: defaultFormMeta as FormMeta,
    onSubmit: async ({ value }) => {
      const parsed = academicYearFormSchema.parse(value);

      if (isUpdating) {
        await updateAsync({
          startDate: parsed.startDate,
          endDate: parsed.endDate,
          terms: parsed.terms,
        });
      } else {
        await createAsync({
          startDate: parsed.startDate,
          endDate: parsed.endDate,
          terms: parsed.terms,
        });
      }

      form.reset();
      onSuccess?.();
    },
  });

  const isPending = isCreating || isUpdatingPending;

  return (
    <AuthorizeView
      policy={
        isUpdating
          ? "canUpdateAcademicYearAndTerms"
          : "canCreateAcademicYearAndTerms"
      }
      unauthorized={
        <Unauthorized
          message="Your current role does not have the necessary permissions to manage academic years."
          buttonLabel="Back to Home"
          redirectOptions={{ to: "/portal" }}
        />
      }
    >
      <form
        className="space-y-6"
        onSubmit={(e) => {
          e.preventDefault();
          e.stopPropagation();
        }}
      >
        {/* Academic Year Dates */}
        <FormSection
          title="Academic Year"
          description="Set the overall start and end dates for this academic year."
        >
          <div className="grid grid-cols-2 gap-4">
            <form.Field name="startDate">
              {(field) => (
                <FormDatePicker field={field} label="Start Date" required />
              )}
            </form.Field>

            <form.Field name="endDate">
              {(field) => (
                <FormDatePicker field={field} label="End Date" required />
              )}
            </form.Field>
          </div>
        </FormSection>

        <Separator />

        {/* Academic Terms */}
        <FormSection
          title="Academic Terms"
          description="Define the terms (semesters) for this academic year. At least one term is required."
        >
          <form.Field name="terms" mode="array">
            {(termsField) => (
              <div className="space-y-4">
                {termsField.state.value.map((_, termIndex) => (
                  <Card
                    key={termIndex}
                    className="border-l-4 border-l-primary/40"
                  >
                    <CardHeader className="pb-2 pt-4 px-4">
                      <div className="flex items-center justify-between">
                        <CardTitle className="text-sm font-medium flex items-center gap-2">
                          <Badge
                            variant="secondary"
                            className="font-mono text-xs"
                          >
                            Term {termIndex + 1}
                          </Badge>
                        </CardTitle>
                      </div>
                    </CardHeader>
                    <CardContent className="px-4 pb-4">
                      <div className="grid grid-cols-2 gap-3">
                        <form.Field name={`terms[${termIndex}].startDate`}>
                          {(field) => (
                            <FormDatePicker
                              field={field}
                              label="Start Date"
                              required
                            />
                          )}
                        </form.Field>
                        <form.Field name={`terms[${termIndex}].endDate`}>
                          {(field) => (
                            <FormDatePicker
                              field={field}
                              label="End Date"
                              required
                            />
                          )}
                        </form.Field>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            )}
          </form.Field>
        </FormSection>

        <Separator />

        {/* Form Actions */}
        <div className="flex items-center gap-3">
          <form.Subscribe
            selector={(state) => [state.canSubmit, state.isSubmitting]}
          >
            {([canSubmit, isSubmitting]) => (
              <Button
                type="submit"
                disabled={!canSubmit || isSubmitting || isPending}
                onClick={() =>
                  form.handleSubmit({
                    submitAction: isUpdating ? "update" : "create",
                    formAction: "close",
                  })
                }
                className="gap-2"
              >
                {(isSubmitting || isPending) && (
                  <Loader2 className="size-4 animate-spin" />
                )}
                {isUpdating ? "Update Academic Year" : "Initiate Academic Year"}
              </Button>
            )}
          </form.Subscribe>

          {onCancel && (
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancel
            </Button>
          )}
        </div>
      </form>
    </AuthorizeView>
  );
}
