import {
  createRoomOptions,
  UpdateRoomOptions,
} from "@/api/collections/room-collection";
import type { Building } from "@/api/models/building";
import type { Room } from "@/api/models/room";
import type { RoomType } from "@/api/models/room-type";
import {
  SearchableSelect,
  type SearchableSelectOption,
} from "@/components/searchable-select";
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
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Unauthorized } from "@/components/unauthorized";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { useForm } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import { Plus, Loader2 } from "lucide-react";
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
    UpdateRoomOptions(roomToUpdate?.id ?? 0)
  );

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onChange: roomFormSchema,
    },
    onSubmitMeta: defaultMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = roomFormSchema.parse(value);

      if (meta.submitAction === "create") {
        await createNewRoomAsync(formValues);
      } else if (meta.submitAction === "update") {
        await updateRoomAsync(formValues);
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
        <Button className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90 cursor-pointer">
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
            className="space-y-4"
            onSubmit={(e) => {
              e.preventDefault();
              e.stopPropagation();
            }}
          >
            <div className="mx-auto w-full max-w-sm">
              <DrawerHeader>
                <DrawerTitle>
                  {isUpdatingRoom ? "Update" : "Create"} Room
                </DrawerTitle>
                <DrawerDescription>Set building details.</DrawerDescription>
              </DrawerHeader>
              <div className="p-4 pb-0">
                <div className="pb-4">
                  <form.Field
                    name="roomNumber"
                    children={(field) => (
                      <div className="grid w-full max-w-sm items-center gap-3">
                        <Label htmlFor={field.name}>Room Number:</Label>
                        <Input
                          type="text"
                          placeholder="Room Number:"
                          value={field.state.value}
                          onChange={(e) => field.handleChange(e.target.value)}
                          id={field.name}
                        />
                        {!field.state.meta.isValid && (
                          <em role="alert" className="text-red-800">
                            {field.state.meta.errors
                              .map((e) => e?.message)
                              .join(", ")}
                          </em>
                        )}
                      </div>
                    )}
                  />
                </div>
                <div className="pb-4">
                  <form.Field
                    name="capacity"
                    children={(field) => (
                      <div className="grid w-full max-w-sm items-center gap-3">
                        <Label htmlFor={field.name}>Capacity:</Label>
                        <Input
                          type="number"
                          value={field.state.value}
                          placeholder="Capacity:"
                          onChange={(e) =>
                            field.handleChange(Number(e.target.value))
                          }
                        />
                        {!field.state.meta.isValid && (
                          <em role="alert" className="text-red-800">
                            {field.state.meta.errors
                              .map((e) => e?.message)
                              .join(", ")}
                          </em>
                        )}
                      </div>
                    )}
                  />
                </div>
                <div className="pb-4">
                  <form.Field
                    name="roomTypeId"
                    children={(field) => (
                      <div className="grid w-full max-w-sm items-center gap-3">
                        <Label htmlFor={field.name}>Room Type:</Label>
                        <SearchableSelect
                          options={roomTypeOptions}
                          value={field.state.value.toString()}
                          onValueChange={(val) =>
                            field.handleChange(Number(val))
                          }
                          name={field.name}
                          placeholder="Select a room type"
                          searchPlaceholder="Search room types..."
                          emptyMessage="No room type found"
                        />

                        {!field.state.meta.isValid && (
                          <em role="alert" className="text-red-800">
                            {field.state.meta.errors
                              .map((e) => e?.message)
                              .join(", ")}
                          </em>
                        )}
                      </div>
                    )}
                  />
                </div>
                <div className="pb-4">
                  <form.Field
                    name="buildingId"
                    children={(field) => (
                      <div className="grid w-full max-w-sm items-center gap-3">
                        <Label htmlFor={field.name}>Building:</Label>
                        <SearchableSelect
                          options={buildingOptions}
                          value={field.state.value.toString()}
                          onValueChange={(val) =>
                            field.handleChange(Number(val))
                          }
                          name={field.name}
                          placeholder="Select a building"
                          searchPlaceholder="Search buildings..."
                          emptyMessage="No building found"
                        />

                        {!field.state.meta.isValid && (
                          <em role="alert" className="text-red-800">
                            {field.state.meta.errors
                              .map((e) => e?.message)
                              .join(", ")}
                          </em>
                        )}
                      </div>
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
                          submitAction: isUpdatingRoom ? "update" : "create",
                          formAction: "close",
                        })
                      }
                    >
                      {isSubmitting ? (
                        <Loader2 />
                      ) : isUpdatingRoom ? (
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
