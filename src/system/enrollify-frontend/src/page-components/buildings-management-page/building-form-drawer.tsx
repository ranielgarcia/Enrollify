import {
  createBuildingOptions,
  updateBuildingOptions,
} from "@/api/collections/building-collection";
import type { Building } from "@/api/models/building";
import type { College } from "@/api/models/college";
import { type SearchableSelectOption } from "@/components/form/searchable-select";
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
import { FormField } from "@/components/form/form-field";
import { FormSelectField } from "@/components/form/form-select-field";
import { FormSection } from "@/components/form/form-section";
import { FormDrawerFooter } from "@/components/form/form-drawer-footer";
import { Unauthorized } from "@/components/unauthorized";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { type FormMeta, defaultFormMeta } from "@/lib/form-meta";
import { useForm } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import { toast } from "sonner";
import z from "zod";

const buildingFormSchema = z.object({
  name: z.string().min(3, "Name is required"),
  description: z.string().min(3, "Description is required"),
  address: z.string().min(3, "Address is required"),
  collegeId: z.number().nonnegative(),
});
type BuildingForm = z.infer<typeof buildingFormSchema>;

interface BuildingFormDrawerProps {
  colleges: College[];
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  buildingToUpdate?: Building | null;
  onOpenChange: (isOpen: boolean) => void;
}

export function BuildingFormDrawer({
  colleges,
  buildingToUpdate,
  onOpenChange,
  isOpen = false,
  setIsOpen,
}: BuildingFormDrawerProps) {
  const defaultFormValues: BuildingForm = {
    name: buildingToUpdate?.name ?? "",
    description: buildingToUpdate?.description ?? "",
    address: buildingToUpdate?.address ?? "",
    collegeId: buildingToUpdate?.college.id ?? 0,
  };

  const { mutateAsync: createNewBuildingAsync } = useMutation(
    createBuildingOptions(),
  );

  const { mutateAsync: updateBuildingAsync } = useMutation(
    updateBuildingOptions(buildingToUpdate?.id ?? 0),
  );

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onBlur: buildingFormSchema,
      onSubmit: buildingFormSchema,
    },
    onSubmitMeta: defaultFormMeta as FormMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = buildingFormSchema.parse(value);
      if (meta.submitAction === "create") {
        await createNewBuildingAsync(formValues);
        toast.success("Building created successfully");
      } else if (meta.submitAction === "update" && buildingToUpdate) {
        await updateBuildingAsync(formValues);
        toast.success("Building updated successfully");
      }

      if (meta.formAction === "close") {
        setIsOpen(false);
      }
      form.reset();
    },
  });

  const collegesOptions: SearchableSelectOption[] = colleges.map((college) => ({
    value: college.id.toString(),
    label: college.name,
  }));

  const isUpdateBuilding = !!buildingToUpdate;

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
          Add Building
        </Button>
      </DrawerTrigger>
      <DrawerContent>
        <AuthorizeView
          policy={isUpdateBuilding ? "canUpdateBuilding" : "canCreateBuilding"}
          unauthorized={
            <Unauthorized
              message="Your current role does not have the necessary permissions to create/update building."
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
                {isUpdateBuilding ? "Update" : "New"} Building
              </DrawerTitle>
              <DrawerDescription>
                Fill in the details below to{" "}
                {isUpdateBuilding ? "update this" : "create a new"} building.
              </DrawerDescription>
            </DrawerHeader>
            <div className="flex-1 overflow-y-auto px-4 py-5 space-y-6">
              <FormSection title="Identification">
                <form.Field
                  name="name"
                  children={(field) => (
                    <FormField field={field} label="Name" required autoFocus />
                  )}
                />
                <form.Field
                  name="address"
                  children={(field) => (
                    <FormField field={field} label="Address" required />
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
                      options={collegesOptions}
                      placeholder="Select a college"
                      searchPlaceholder="Search college"
                      emptyMessage="No college found"
                    />
                  )}
                />
              </FormSection>
            </div>

            <FormDrawerFooter
              form={form}
              isUpdate={isUpdateBuilding}
              onCancel={() => setIsOpen(false)}
              entityLabel="Building"
              showSaveAndAddAnother
            />
          </form>
        </AuthorizeView>
      </DrawerContent>
    </Drawer>
  );
}
