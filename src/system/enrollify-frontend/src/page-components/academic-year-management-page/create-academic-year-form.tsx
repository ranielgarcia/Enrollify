import {
  createAcademicYearOptions,
  updateAcademicYearOptions,
} from "@/api/collections/academic-year-collection";
import type { AcademicYear } from "@/api/models/academic-year";
import { FormDatePicker } from "@/components/form/form-date-picker";
import { Button } from "@/components/ui/button";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { Unauthorized } from "@/components/unauthorized";
import { type FormMeta, defaultFormMeta } from "@/lib/form-meta";
import { useForm } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import { CalendarRange, Loader2 } from "lucide-react";
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
        className="space-y-6 max-w-2xl"
        onSubmit={(e) => {
          e.preventDefault();
          e.stopPropagation();
        }}
      >
        {/* Academic Year Dates */}
        <section className="space-y-4">
          <div className="flex items-center gap-2">
            <div className="flex h-7 w-7 items-center justify-center rounded-md bg-primary/10">
              <CalendarRange className="h-3.5 w-3.5 text-primary" />
            </div>
            <div>
              <p className="text-sm font-semibold leading-none">
                Academic Year
              </p>
              <p className="text-xs text-muted-foreground mt-0.5">
                Overall start and end dates for this academic year
              </p>
            </div>
          </div>

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
        </section>

        {/* Divider */}
        <div className="relative">
          <div className="absolute inset-0 flex items-center">
            <div className="w-full border-t border-dashed" />
          </div>
        </div>

        {/* Academic Terms */}
        <section className="space-y-4">
          <div>
            <p className="text-sm font-semibold">Academic Terms</p>
            <p className="text-xs text-muted-foreground mt-0.5">
              Define the date range for each term (semester) in this academic
              year
            </p>
          </div>

          <form.Field name="terms" mode="array">
            {(termsField) => (
              <div className="space-y-3">
                {termsField.state.value.map((_, termIndex) => (
                  <div
                    key={termIndex}
                    className="rounded-lg border bg-muted/30 p-4 space-y-3"
                  >
                    <div className="flex items-center gap-2">
                      <span className="flex h-6 w-6 items-center justify-center rounded-md bg-primary/10 text-xs font-bold text-primary">
                        {termIndex + 1}
                      </span>
                      <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wide">
                        Term {termIndex + 1}
                      </p>
                    </div>
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
                  </div>
                ))}
              </div>
            )}
          </form.Field>
        </section>

        {/* Actions */}
        <div className="flex items-center gap-3 pt-1">
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
