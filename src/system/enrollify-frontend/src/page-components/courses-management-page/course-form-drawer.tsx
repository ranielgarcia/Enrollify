import {
  createCourseOptions,
  updateCourseOptions,
} from "@/api/collections/course-collection";
import type { College } from "@/api/models/college";
import type { Course } from "@/api/models/course";
import { type SearchableSelectOption } from "@/components/searchable-select";
import { Button } from "@/components/ui/button";
import {
  Drawer,
  DrawerContent,
  DrawerDescription,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from "@/components/ui/drawer";
import { Separator } from "@/components/ui/separator";
import { FormField } from "@/components/form-field";
import { FormSelectField } from "@/components/form-select-field";
import { FormSection } from "@/components/form-section";
import { FormDrawerFooter } from "@/components/form-drawer-footer";
import { Unauthorized } from "@/components/unauthorized";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { type FormMeta, defaultFormMeta } from "@/lib/form-meta";
import { useForm } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import { toast } from "sonner";
import z from "zod";

const courseFormSchema = z.object({
  code: z.string().min(3, "Code is required"),
  name: z.string().min(3, "Name is required"),
  description: z.string().min(3, "Description is required"),
  durationYears: z
    .number()
    .min(1, "Duration must be at least 1 year")
    .max(10, "Duration cannot exceed 10 years"),
  collegeId: z.number().min(1, "College is required"),
});

type CourseFormData = z.infer<typeof courseFormSchema>;

interface CourseFormDrawerProps {
  colleges: College[];
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  courseToUpdate?: Course | undefined;
  onOpenChange: (isOpen: boolean) => void;
}

export function CourseFormDrawer({
  colleges,
  isOpen,
  setIsOpen,
  courseToUpdate,
  onOpenChange,
}: CourseFormDrawerProps) {
  const defaultFormValues: CourseFormData = {
    code: courseToUpdate?.code ?? "",
    name: courseToUpdate?.name ?? "",
    description: courseToUpdate?.description ?? "",
    durationYears: courseToUpdate?.durationYears ?? 1,
    collegeId: courseToUpdate?.college?.id ?? 0,
  };

  const { mutateAsync: createNewCourseAsync } = useMutation(
    createCourseOptions(),
  );
  const { mutateAsync: updateCourseAsync } = useMutation(
    updateCourseOptions(courseToUpdate?.id ?? 0),
  );

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onBlur: courseFormSchema,
      onSubmit: courseFormSchema,
    },
    onSubmitMeta: defaultFormMeta as FormMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = courseFormSchema.parse(value);
      if (meta.submitAction === "create") {
        await createNewCourseAsync(formValues);
        toast.success("Course created successfully");
      } else if (meta.submitAction === "update") {
        await updateCourseAsync(formValues);
        toast.success("Course updated successfully");
      }

      if (meta.formAction === "close") {
        setIsOpen(false);
      }
      form.reset();
    },
  });

  const collegesOptions: SearchableSelectOption[] =
    colleges?.map((college) => ({
      value: college.id.toString(),
      label: college.name,
    })) ?? [];

  const isUpdateCourse = !!courseToUpdate;

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
        >
          <Plus className="size-4" />
          Add Course
        </Button>
      </DrawerTrigger>
      <DrawerContent>
        <AuthorizeView
          policy={isUpdateCourse ? "canUpdateCourse" : "canCreateCourse"}
          unauthorized={
            <Unauthorized
              message="Your current role does not have the necessary permissions to create course."
              buttonLabel="Back to Home"
              backCallback={() => setIsOpen(false)}
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
          >
            <div className="mx-auto w-full max-w-sm flex flex-col h-full">
              <DrawerHeader className="border-b pb-4">
                <DrawerTitle>
                  {isUpdateCourse ? "Update" : "New"} Course
                </DrawerTitle>
                <DrawerDescription>
                  Fill in the details below to{" "}
                  {isUpdateCourse ? "update this" : "create a new"} course.
                </DrawerDescription>
              </DrawerHeader>
              <div className="px-4 py-5 space-y-6 overflow-y-auto">
                <FormSection title="Identification">
                  <div className="grid grid-cols-2 gap-4">
                    <form.Field
                      name="code"
                      children={(field) => (
                        <FormField
                          field={field}
                          label="Code"
                          required
                          hint="e.g. BSCS, BSIT"
                          autoFocus
                        />
                      )}
                    />
                    <form.Field
                      name="durationYears"
                      children={(field) => (
                        <FormField
                          field={field}
                          label="Duration (Years)"
                          type="number"
                          required
                        />
                      )}
                    />
                  </div>
                  <form.Field
                    name="name"
                    children={(field) => (
                      <FormField
                        field={field}
                        label="Name"
                        required
                        hint="Full course name"
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
                        required
                        maxLength={500}
                      />
                    )}
                  />
                </FormSection>

                <Separator />

                <FormSection title="Classification">
                  <form.Field
                    name="collegeId"
                    children={(field) => (
                      <FormSelectField
                        field={field}
                        label="College"
                        required
                        options={collegesOptions}
                        placeholder="Select a college"
                        searchPlaceholder="Search colleges..."
                        emptyMessage="No college found"
                      />
                    )}
                  />
                </FormSection>
              </div>

              <FormDrawerFooter
                form={form}
                isUpdate={isUpdateCourse}
                onCancel={() => setIsOpen(false)}
                entityLabel="Course"
                showSaveAndAddAnother
              />
            </div>
          </form>
        </AuthorizeView>
      </DrawerContent>
    </Drawer>
  );
}
