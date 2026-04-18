import z from "zod";
import { useForm } from "@tanstack/react-form";
import {
  Drawer,
  DrawerContent,
  DrawerDescription,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from "@/components/ui/drawer";
import { Separator } from "@/components/ui/separator";
import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { Unauthorized } from "@/components/unauthorized";
import {
  createCollegeOptions,
  updateCollegeOptions,
} from "@/api/collections/college-collection";
import type { College } from "@/api/models/college";
import { useMutation } from "@tanstack/react-query";
import { FormField } from "@/components/form-field";
import { FormSection } from "@/components/form-section";
import { FormDrawerFooter } from "@/components/form-drawer-footer";
import { type FormMeta, defaultFormMeta } from "@/lib/form-meta";
import { toast } from "sonner";

const collegeFormSchema = z.object({
  code: z.string().min(3, "Code is required"),
  name: z.string().min(3, "Name is required"),
  description: z.string().min(3, "Description is required"),
  dean: z.string().min(3, "Dean is required"),
});
type CollegeForm = z.infer<typeof collegeFormSchema>;

interface CollegeFormDrawerProps {
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  collegeToUpdate?: College | null;
  onOpenChange: (isOpen: boolean) => void;
}

export function CollegeFormDrawer({
  collegeToUpdate,
  onOpenChange,
  isOpen = false,
  setIsOpen,
}: CollegeFormDrawerProps) {
  const defaultFormValues: CollegeForm = {
    code: collegeToUpdate?.code ?? "",
    name: collegeToUpdate?.name ?? "",
    description: collegeToUpdate?.description ?? "",
    dean: collegeToUpdate?.dean ?? "",
  };

  const { mutateAsync: createNewCollegeAsync } = useMutation(
    createCollegeOptions(),
  );

  const { mutateAsync: updateCollegeAsync } = useMutation(
    updateCollegeOptions(collegeToUpdate?.id ?? 0),
  );

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onBlur: collegeFormSchema,
      onSubmit: collegeFormSchema,
    },
    onSubmitMeta: defaultFormMeta as FormMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = collegeFormSchema.parse(value);
      if (meta.submitAction === "create") {
        await createNewCollegeAsync(formValues);
        toast.success("College created successfully");
      } else if (meta.submitAction === "update" && collegeToUpdate) {
        await updateCollegeAsync(formValues);
        toast.success("College updated successfully");
      }

      if (meta.formAction === "close") {
        setIsOpen(false);
      }
      form.reset();
    },
  });

  const isUpdateCollege = !!collegeToUpdate;
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
          Add College
        </Button>
      </DrawerTrigger>
      <DrawerContent>
        <AuthorizeView
          policy={isUpdateCollege ? "canUpdateCollege" : "canCreateCollege"}
          unauthorized={
            <Unauthorized
              message="Your current role does not have the necessary permissions to create college."
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
                  {isUpdateCollege ? "Update" : "New"} College
                </DrawerTitle>
                <DrawerDescription>
                  Fill in the details below to{" "}
                  {isUpdateCollege ? "update this" : "create a new"} college.
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
                        hint="e.g. COE, COS"
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
                        hint="Full college name"
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
                    name="dean"
                    children={(field) => (
                      <FormField field={field} label="Dean" required />
                    )}
                  />
                </FormSection>
              </div>

              <FormDrawerFooter
                form={form}
                isUpdate={isUpdateCollege}
                onCancel={() => setIsOpen(false)}
                entityLabel="College"
                showSaveAndAddAnother
              />
            </div>
          </form>
        </AuthorizeView>
      </DrawerContent>
    </Drawer>
  );
}
