import {
  createRoomOptions,
  UpdateRoomOptions,
} from "@/api/collections/room-collection";
import type { Building } from "@/api/models/building";
import type { Room } from "@/api/models/room";
import type { RoomType } from "@/api/models/room-type";
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

const roomFormSchema = z.object({
  roomNumber: z.string().min(1, "Room number is required"),
  capacity: z.number().min(1, "Capacity must be at least 1"),
  buildingId: z
    .number()
    .nonnegative("Building is required")
    .min(1, "Building is required"),
  roomTypeId: z
    .number()
    .nonnegative("Room Type is required")
    .min(1, "Room Type is required"),
});
type RoomFormData = z.infer<typeof roomFormSchema>;

interface RoomFormDrawerProps {
  buildings?: Building[];
  roomTypes?: RoomType[];
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  roomToUpdate?: Room | null;
  onOpenChange: (isOpen: boolean) => void;
}

export function RoomFormDrawer({
  buildings,
  roomTypes,
  isOpen,
  setIsOpen,
  roomToUpdate,
  onOpenChange,
}: RoomFormDrawerProps) {
  const defaultFormValues: RoomFormData = {
    roomNumber: roomToUpdate?.roomNumber ?? "",
    capacity: roomToUpdate?.capacity ?? 1,
    buildingId: roomToUpdate?.building?.id ?? 0,
    roomTypeId: roomToUpdate?.roomType?.id ?? 0,
  };

  const { mutateAsync: createNewRoomAsync } = useMutation(createRoomOptions());
  const { mutateAsync: updateRoomAsync } = useMutation(
    UpdateRoomOptions(roomToUpdate?.id ?? 0),
  );

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onBlur: roomFormSchema,
      onSubmit: roomFormSchema,
    },
    onSubmitMeta: defaultFormMeta as FormMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = roomFormSchema.parse(value);

      if (meta.submitAction === "create") {
        await createNewRoomAsync(formValues);
        toast.success("Room created successfully");
      } else if (meta.submitAction === "update") {
        await updateRoomAsync(formValues);
        toast.success("Room updated successfully");
      }

      if (meta.formAction === "close") {
        setIsOpen(false);
      }
      form.reset();
    },
  });

  const buildingOptions: SearchableSelectOption[] =
    buildings?.map((building) => ({
      value: building.id.toString(),
      label: building.name,
    })) ?? [];

  const roomTypeOptions: SearchableSelectOption[] =
    roomTypes?.map((roomType) => ({
      value: roomType.id.toString(),
      label: roomType.name,
    })) ?? [];

  const isUpdatingRoom = !!roomToUpdate;
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
          Add Room
        </Button>
      </DrawerTrigger>
      <DrawerContent>
        <AuthorizeView
          policy="canCreateRooms"
          unauthorized={
            <Unauthorized
              message="Your current role does not have the necessary permissions to create room."
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
                {isUpdatingRoom ? "Update" : "New"} Room
              </DrawerTitle>
              <DrawerDescription>
                Fill in the details below to{" "}
                {isUpdatingRoom ? "update this" : "create a new"} room.
              </DrawerDescription>
            </DrawerHeader>
            <div className="flex-1 overflow-y-auto px-4 py-5 space-y-6">
                <FormSection title="Room Info">
                  <div className="grid grid-cols-2 gap-4">
                    <form.Field
                      name="roomNumber"
                      children={(field) => (
                        <FormField
                          field={field}
                          label="Room Number"
                          required
                          hint="e.g. 101, A-201"
                          autoFocus
                        />
                      )}
                    />
                    <form.Field
                      name="capacity"
                      children={(field) => (
                        <FormField
                          field={field}
                          label="Capacity"
                          type="number"
                          required
                        />
                      )}
                    />
                  </div>
                </FormSection>

                <Separator />

                <FormSection title="Classification">
                  <form.Field
                    name="roomTypeId"
                    children={(field) => (
                      <FormSelectField
                        field={field}
                        label="Room Type"
                        required
                        options={roomTypeOptions}
                        placeholder="Select a room type"
                        searchPlaceholder="Search room types..."
                        emptyMessage="No room type found"
                      />
                    )}
                  />
                  <form.Field
                    name="buildingId"
                    children={(field) => (
                      <FormSelectField
                        field={field}
                        label="Building"
                        required
                        options={buildingOptions}
                        placeholder="Select a building"
                        searchPlaceholder="Search buildings..."
                        emptyMessage="No building found"
                      />
                    )}
                  />
                </FormSection>
              </div>

              <FormDrawerFooter
                form={form}
                isUpdate={isUpdatingRoom}
                onCancel={() => setIsOpen(false)}
                entityLabel="Room"
                showSaveAndAddAnother
              />
          </form>
        </AuthorizeView>
      </DrawerContent>
    </Drawer>
  );
}
