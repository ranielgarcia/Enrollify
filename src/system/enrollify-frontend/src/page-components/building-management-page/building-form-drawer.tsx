import {
  createBuildingOptions,
  updateBuildingOptions,
} from "@/api/collections/building-collection";
import type { Building } from "@/api/models/building";
import { Button } from "@/components/ui/button";
import {
  Drawer,
  DrawerClose,
  DrawerContent,
  DrawerDescription,
  DrawerFooter,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from "@/components/ui/drawer";
import { Unauthorized } from "@/components/unauthorized";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { useForm } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import { Loader2, Plus } from "lucide-react";
import z from "zod";

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

const buildingFormSchema = z.object({
  name: z.string().min(3, "Name is required"),
  description: z.string().min(3, "Description is required"),
  address: z.string().min(3, "Address is required"),
});
type BuildingForm = z.infer<typeof buildingFormSchema>;

interface BuildingFormDrawerProps {
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  buildingToUpdate?: Building | null;
  onOpenChange: (isOpen: boolean) => void;
}

export function BuildingFormDrawer({
  buildingToUpdate,
  onOpenChange,
  isOpen = false,
  setIsOpen,
}: BuildingFormDrawerProps) {
  const defaultFormValues: BuildingForm = {
    name: buildingToUpdate?.name ?? "",
    description: buildingToUpdate?.description ?? "",
    address: buildingToUpdate?.address ?? "",
  };

  const { mutateAsync: createNewBuildingAsync } = useMutation(
    createBuildingOptions()
  );

  const { mutateAsync: updateBuildingAsync } = useMutation(
    updateBuildingOptions(buildingToUpdate?.id ?? 0)
  );

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onChange: buildingFormSchema,
    },
    onSubmitMeta: defaultMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = buildingFormSchema.parse(value);
      if (meta.submitAction === "create") {
        await createNewBuildingAsync(formValues);
      } else if (meta.submitAction === "update" && buildingToUpdate) {
        await updateBuildingAsync(formValues);
      }

      if (meta.formAction === "close") {
        setIsOpen(false);
      }
      form.reset();
    },
  });

  const isUpdateBuilding = !!buildingToUpdate;

  return (
    <Drawer
      direction="right"
      dismissible={false}
      open={isOpen}
      onOpenChange={onOpenChange}
    >
      <DrawerTrigger asChild>
        <Button className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90 cursor-pointer">
          <Plus className="size-4" />
          Add Building
        </Button>
      </DrawerTrigger>
      <DrawerContent>
        <AuthorizeView
          policy="canCreateBuilding"
          unauthorized={
            <Unauthorized
              message="Your current role does not have the necessary permissions to create building."
              buttonLabel="Back to Home"
              backCallback={() => setIsOpen(false)}
              redirectOptions={{
                to: "/portal",
              }}
            />
          }
        >
          <form
            className="space-y-4"
            onSubmit={(e) => {
              e.preventDefault();
              e.stopPropagation();
            }}
          >
            <div className="mx-auto w-full max-w-sm">
              <DrawerHeader>
                <DrawerTitle>
                  {isUpdateBuilding ? "Update" : "Create"} Building
                </DrawerTitle>
                <DrawerDescription>Set building details.</DrawerDescription>
              </DrawerHeader>
              <div className="p-4 pb-0">
                <div>
                  <label className="text-sm font-medium text-foreground block mb-1">
                    Name
                  </label>
                  <form.Field
                    name="name"
                    children={(field) => (
                      <>
                        <input
                          className="w-full px-3 py-2 rounded-lg border border-border bg-background text-foreground"
                          placeholder="e.g., Main Building"
                          value={field.state.value}
                          onChange={(e) => field.handleChange(e.target.value)}
                        />
                        <em role="alert" className="text-red-800">
                          {field.state.meta.errors
                            .map((e) => e?.message)
                            .join(", ")}
                        </em>
                      </>
                    )}
                  />
                </div>
                <div>
                  <label className="text-sm font-medium text-foreground block mb-1">
                    Description
                  </label>
                  <form.Field
                    name="description"
                    children={(field) => (
                      <>
                        <input
                          className="w-full px-3 py-2 rounded-lg border border-border bg-background text-foreground"
                          placeholder="e.g., Main Building"
                          value={field.state.value}
                          onChange={(e) => field.handleChange(e.target.value)}
                        />
                        {!field.state.meta.isValid && (
                          <em role="alert" className="text-red-800">
                            {field.state.meta.errors
                              .map((e) => e?.message)
                              .join(", ")}
                          </em>
                        )}
                      </>
                    )}
                  />
                </div>
                <div>
                  <label className="text-sm font-medium text-foreground block mb-1">
                    Address
                  </label>
                  <form.Field
                    name="address"
                    children={(field) => (
                      <>
                        <input
                          className="w-full px-3 py-2 rounded-lg border border-border bg-background text-foreground"
                          placeholder="e.g., 123 Main St."
                          value={field.state.value}
                          onChange={(e) => field.handleChange(e.target.value)}
                        />
                        {!field.state.meta.isValid && (
                          <em role="alert" className="text-red-800">
                            {field.state.meta.errors
                              .map((e) => e?.message)
                              .join(", ")}
                          </em>
                        )}
                      </>
                    )}
                  />
                </div>
              </div>

              <DrawerFooter>
                <form.Subscribe
                  selector={(state) => [state.canSubmit, state.isSubmitting]}
                  children={([canSubmit, isSubmitting]) => (
                    <Button
                      className=" cursor-pointer"
                      disabled={!canSubmit}
                      type="submit"
                      onClick={() =>
                        form.handleSubmit({
                          submitAction: isUpdateBuilding ? "update" : "create",
                          formAction: "close",
                        })
                      }
                    >
                      {isSubmitting ? (
                        <Loader2 />
                      ) : isUpdateBuilding ? (
                        "Update"
                      ) : (
                        "Submit"
                      )}
                    </Button>
                  )}
                />
                <DrawerClose asChild>
                  <Button
                    className=" cursor-pointer"
                    variant="outline"
                    onClick={() => setIsOpen(false)}
                  >
                    Cancel
                  </Button>
                </DrawerClose>
              </DrawerFooter>
            </div>
          </form>
        </AuthorizeView>
      </DrawerContent>
    </Drawer>
  );
}
