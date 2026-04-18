import { Button } from "@/components/ui/button";
import {
  Drawer,
  DrawerContent,
  DrawerDescription,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from "@/components/ui/drawer";
import { Plus } from "lucide-react";
import { useForm } from "@tanstack/react-form";
import { z } from "zod";
import type { RoomType } from "../../../api/models/room-type";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { Unauthorized } from "@/components/unauthorized";
import {
  createRoomTypeOptions,
  updateRoomTypeOptions,
} from "@/api/collections/room-type-collection";
import { useMutation } from "@tanstack/react-query";
import { FormField } from "@/components/form-field";
import { FormSection } from "@/components/form-section";
import { FormDrawerFooter } from "@/components/form-drawer-footer";
import { type FormMeta, defaultFormMeta } from "@/lib/form-meta";
import { toast } from "sonner";

const roomTypeSchema = z.object({
  name: z.string().min(3, "Name is required"),
  description: z.string().min(3, "Description is required"),
});
type RoomTypeForm = z.infer<typeof roomTypeSchema>;

interface RoomTypeFormDrawerProps {
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  roomTypeToUpdate?: RoomType | null;
  onOpenChange: (isOpen: boolean) => void;
}

export function RoomTypeFormDrawer({
  roomTypeToUpdate,
  onOpenChange,
  isOpen = false,
  setIsOpen,
}: RoomTypeFormDrawerProps) {
  const defaultFormValues: RoomTypeForm = {
    name: roomTypeToUpdate?.name ?? "",
    description: roomTypeToUpdate?.description ?? "",
  };

  const { mutateAsync: createNewRoomTypeAsync } = useMutation(
    createRoomTypeOptions(),
  );
  const { mutateAsync: updateRoomTypeAsync } = useMutation(
    updateRoomTypeOptions(roomTypeToUpdate?.id ?? 0),
  );

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onBlur: roomTypeSchema,
      onSubmit: roomTypeSchema,
    },
    onSubmitMeta: defaultFormMeta as FormMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = roomTypeSchema.parse(value);
      if (meta.submitAction === "create") {
        await createNewRoomTypeAsync(formValues);
        toast.success("Room type created successfully");
      }

      if (meta.submitAction === "update" && roomTypeToUpdate) {
        await updateRoomTypeAsync(formValues);
        toast.success("Room type updated successfully");
      }

      if (meta.formAction === "close") {
        setIsOpen(false);
      }
      form.reset();
    },
  });
  const isUpdateRoomType = !!roomTypeToUpdate;

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
          Add Room Type
        </Button>
      </DrawerTrigger>
      <DrawerContent>
        <AuthorizeView
          policy="canCreateRoomTypes"
          unauthorized={
            <Unauthorized
              message="Your current role does not have the necessary permissions to create room type."
              buttonLabel="Back to Home"
              backOptions={{
                to: "/portal/master-data/rooms/room-types",
              }}
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
                  {isUpdateRoomType ? "Update" : "New"} Room Type
                </DrawerTitle>
                <DrawerDescription>
                  Fill in the details below to{" "}
                  {isUpdateRoomType ? "update this" : "create a new"} room type.
                </DrawerDescription>
              </DrawerHeader>
              <div className="px-4 py-5 space-y-6 overflow-y-auto">
                <FormSection>
                  <form.Field
                    name="name"
                    children={(field) => (
                      <FormField
                        field={field}
                        label="Type Name"
                        required
                        autoFocus
                      />
                    )}
                  />
                  <form.Field
                    name="description"
                    children={(field) => (
                      <FormField
                        field={field}
                        label="Description"
                        type="textarea"
                        required
                        placeholder="e.g., Large classroom for lectures"
                        maxLength={500}
                      />
                    )}
                  />
                </FormSection>
              </div>

              <FormDrawerFooter
                form={form}
                isUpdate={isUpdateRoomType}
                onCancel={() => setIsOpen(false)}
                entityLabel="Room Type"
                showSaveAndAddAnother
              />
            </div>
          </form>
        </AuthorizeView>
      </DrawerContent>
    </Drawer>
  );
}
