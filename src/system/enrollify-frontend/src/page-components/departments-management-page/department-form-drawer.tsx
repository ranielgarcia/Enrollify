import {
  createDepartmentOptions,
  updateDepartmentOptions,
} from "@/api/collections/department-collection";
import type { College } from "@/api/models/college";
import type { Department } from "@/api/models/department";
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

const departmentFormSchema = z.object({
  code: z.string().min(3, "Code is required"),
  name: z.string().min(3, "Name is required"),
  chairperson: z.string().min(3, "Chairperson is required"),
  description: z.string().min(3, "Description is required"),
  collegeId: z.number().min(1, "College is required"),
});
type DepartmentFormData = z.infer<typeof departmentFormSchema>;

interface DepartmentFormDrawerProps {
  colleges: College[];
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  departmentToUpdate?: Department | null;
  onOpenChange: (isOpen: boolean) => void;
}

export function DepartmentFormDrawer({
  colleges,
  isOpen,
  setIsOpen,
  departmentToUpdate,
  onOpenChange,
}: DepartmentFormDrawerProps) {
  const defaultFormValues: DepartmentFormData = {
    code: departmentToUpdate?.code ?? "",
    name: departmentToUpdate?.name ?? "",
    chairperson: departmentToUpdate?.chairperson ?? "",
    description: departmentToUpdate?.description ?? "",
    collegeId: departmentToUpdate?.college.id ?? 0,
  };

  const { mutateAsync: createNewDepartmentAsync } = useMutation(
    createDepartmentOptions(),
  );
  const { mutateAsync: updateDepartmentAsync } = useMutation(
    updateDepartmentOptions(departmentToUpdate?.id ?? 0),
  );

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onBlur: departmentFormSchema,
      onSubmit: departmentFormSchema,
    },
    onSubmitMeta: defaultFormMeta as FormMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = departmentFormSchema.parse(value);
      if (meta.submitAction === "create") {
        await createNewDepartmentAsync(formValues);
        toast.success("Department created successfully");
      } else if (meta.submitAction === "update") {
        await updateDepartmentAsync(formValues);
        toast.success("Department updated successfully");
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

  const isUpdateDepartment = !!departmentToUpdate;

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
          Add Department
        </Button>
      </DrawerTrigger>
      <DrawerContent>
        <AuthorizeView
          policy={
            isUpdateDepartment ? "canUpdateDepartment" : "canCreateDepartment"
          }
          unauthorized={
            <Unauthorized
              message="Your current role does not have the necessary permissions to create department."
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
                  {isUpdateDepartment ? "Update" : "New"} Department
                </DrawerTitle>
                <DrawerDescription>
                  Fill in the details below to{" "}
                  {isUpdateDepartment ? "update this" : "create a new"}{" "}
                  department.
                </DrawerDescription>
              </DrawerHeader>
              <div className="px-4 py-5 space-y-6 overflow-y-auto">
                <FormSection title="Identification">
                  <form.Field
                    name="code"
                    children={(field) => (
                      <FormField
                        field={field}
                        label="Code"
                        required
                        hint="e.g. CS, IT, EE"
                        autoFocus
                      />
                    )}
                  />
                  <form.Field
                    name="name"
                    children={(field) => (
                      <FormField
                        field={field}
                        label="Name"
                        required
                        hint="Full department name"
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

                <FormSection title="Leadership">
                  <form.Field
                    name="chairperson"
                    children={(field) => (
                      <FormField
                        field={field}
                        label="Chairperson"
                        required
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
                isUpdate={isUpdateDepartment}
                onCancel={() => setIsOpen(false)}
                entityLabel="Department"
                showSaveAndAddAnother
              />
            </div>
          </form>
        </AuthorizeView>
      </DrawerContent>
    </Drawer>
  );
}
