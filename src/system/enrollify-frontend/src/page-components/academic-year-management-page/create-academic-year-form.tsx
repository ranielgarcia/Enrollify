import {
  createAcademicYearOptions,
  updateAcademicYearOptions,
} from "@/api/collections/academic-year-collection";
import type { AcademicYear } from "@/api/models/academic-year";
import { FormSection } from "@/components/form/form-section";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { Unauthorized } from "@/components/unauthorized";
import { type FormMeta, defaultFormMeta } from "@/lib/form-meta";
import { useForm } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import { Plus, Trash2, Loader2 } from "lucide-react";
import { useCallback } from "react";
import { format, parseISO } from "date-fns";
import z from "zod";

// ---------- Zod schemas ----------

const termFormSchema = z.object({
  termNumber: z.number().min(1, "Term number must be at least 1"),
  startDate: z.string().min(1, "Start date is required"),
  endDate: z.string().min(1, "End date is required"),
});

const academicYearFormSchema = z.object({
  startDate: z.string().min(1, "Start date is required"),
  endDate: z.string().min(1, "End date is required"),
  terms: z
    .array(termFormSchema)
    .min(1, "At least one term is required"),
});

type AcademicYearFormData = z.infer<typeof academicYearFormSchema>;

// ---------- Helpers ----------

function toInputDate(isoStr?: string | null): string {
  if (!isoStr) return "";
  try {
    return format(parseISO(isoStr), "yyyy-MM-dd");
  } catch {
    return "";
  }
}

// ---------- Field-level error helper ----------

function FieldError({ errors }: { errors: unknown[] }) {
  const msg = errors
    .map((e) => (typeof e === "string" ? e : (e as { message?: string })?.message))
    .filter(Boolean)
    .join(", ");
  if (!msg) return null;
  return <p className="text-xs text-destructive mt-1">{msg}</p>;
}

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
  const isUpdating = !!yearToEdit;

  const defaultTerms = yearToEdit?.academicTerms?.length
    ? yearToEdit.academicTerms
        .slice()
        .sort((a, b) => (a.termNumber ?? 0) - (b.termNumber ?? 0))
        .map((t) => ({
          termNumber: t.termNumber ?? 1,
          startDate: toInputDate(t.startDate),
          endDate: toInputDate(t.endDate),
        }))
    : [
        { termNumber: 1, startDate: "", endDate: "" },
        { termNumber: 2, startDate: "", endDate: "" },
        { termNumber: 3, startDate: "", endDate: "" },
      ];

  const defaultFormValues: AcademicYearFormData = {
    startDate: toInputDate(yearToEdit?.startDate),
    endDate: toInputDate(yearToEdit?.endDate),
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

  const handleAddTerm = useCallback(() => {
    const current = form.getFieldValue("terms");
    form.setFieldValue("terms", [
      ...current,
      {
        termNumber: current.length + 1,
        startDate: "",
        endDate: "",
      },
    ]);
  }, [form]);

  const handleRemoveTerm = useCallback(
    (index: number) => {
      const current = form.getFieldValue("terms");
      const updated = current
        .filter((_, i) => i !== index)
        .map((t, i) => ({ ...t, termNumber: i + 1 }));
      form.setFieldValue("terms", updated);
    },
    [form],
  );

  return (
    <AuthorizeView
      policy={isUpdating ? "canUpdateAcademicYear" : "canCreateAcademicYear"}
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
                <div className="grid gap-1.5">
                  <Label htmlFor="startDate">
                    Start Date <span className="text-destructive">*</span>
                  </Label>
                  <Input
                    id="startDate"
                    type="date"
                    value={field.state.value}
                    onChange={(e) => field.handleChange(e.target.value)}
                    onBlur={field.handleBlur}
                  />
                  <FieldError errors={field.state.meta.errors} />
                </div>
              )}
            </form.Field>

            <form.Field name="endDate">
              {(field) => (
                <div className="grid gap-1.5">
                  <Label htmlFor="endDate">
                    End Date <span className="text-destructive">*</span>
                  </Label>
                  <Input
                    id="endDate"
                    type="date"
                    value={field.state.value}
                    onChange={(e) => field.handleChange(e.target.value)}
                    onBlur={field.handleBlur}
                  />
                  <FieldError errors={field.state.meta.errors} />
                </div>
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
                  <Card key={termIndex} className="border-l-4 border-l-primary/40">
                    <CardHeader className="pb-2 pt-4 px-4">
                      <div className="flex items-center justify-between">
                        <CardTitle className="text-sm font-medium flex items-center gap-2">
                          <Badge variant="secondary" className="font-mono text-xs">
                            Term {termIndex + 1}
                          </Badge>
                        </CardTitle>
                        {termsField.state.value.length > 1 && (
                          <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            className="h-7 w-7 p-0 hover:bg-destructive/10 hover:text-destructive"
                            onClick={() => handleRemoveTerm(termIndex)}
                          >
                            <Trash2 className="size-3.5" />
                          </Button>
                        )}
                      </div>
                    </CardHeader>
                    <CardContent className="px-4 pb-4">
                      <div className="grid grid-cols-3 gap-3">
                        <form.Field name={`terms[${termIndex}].termNumber`}>
                          {(field) => (
                            <div className="grid gap-1.5">
                              <Label>
                                Term Number{" "}
                                <span className="text-destructive">*</span>
                              </Label>
                              <Input
                                type="number"
                                min={1}
                                value={field.state.value}
                                onChange={(e) =>
                                  field.handleChange(Number(e.target.value))
                                }
                                onBlur={field.handleBlur}
                              />
                              <FieldError errors={field.state.meta.errors} />
                            </div>
                          )}
                        </form.Field>

                        <form.Field name={`terms[${termIndex}].startDate`}>
                          {(field) => (
                            <div className="grid gap-1.5">
                              <Label>
                                Start Date{" "}
                                <span className="text-destructive">*</span>
                              </Label>
                              <Input
                                type="date"
                                value={field.state.value}
                                onChange={(e) =>
                                  field.handleChange(e.target.value)
                                }
                                onBlur={field.handleBlur}
                              />
                              <FieldError errors={field.state.meta.errors} />
                            </div>
                          )}
                        </form.Field>

                        <form.Field name={`terms[${termIndex}].endDate`}>
                          {(field) => (
                            <div className="grid gap-1.5">
                              <Label>
                                End Date{" "}
                                <span className="text-destructive">*</span>
                              </Label>
                              <Input
                                type="date"
                                value={field.state.value}
                                onChange={(e) =>
                                  field.handleChange(e.target.value)
                                }
                                onBlur={field.handleBlur}
                              />
                              <FieldError errors={field.state.meta.errors} />
                            </div>
                          )}
                        </form.Field>
                      </div>
                    </CardContent>
                  </Card>
                ))}

                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="gap-2 border-dashed"
                  onClick={handleAddTerm}
                >
                  <Plus className="size-4" />
                  Add Term
                </Button>
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
