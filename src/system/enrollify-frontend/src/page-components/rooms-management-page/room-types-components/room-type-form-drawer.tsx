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
import { Plus } from "lucide-react";
import { useForm } from "@tanstack/react-form";
import { z } from "zod";
import { Loader2 } from "lucide-react";
import type { RoomType } from "../../../api/models/room-type";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { Unauthorized } from "@/components/unauthorized";
import {
  createRoomTypeOptions,
  updateRoomTypeOptions,
} from "@/api/collections/room-type-collection";
import { useMutation } from "@tanstack/react-query";

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
    createRoomTypeOptions()
  );
  const { mutateAsync: updateRoomTypeAsync } = useMutation(
    updateRoomTypeOptions(roomTypeToUpdate?.id ?? 0)
  );

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onChange: roomTypeSchema,
    },
    onSubmitMeta: defaultMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = roomTypeSchema.parse(value);
      if (meta.submitAction === "create") {
        await createNewRoomTypeAsync(formValues);
      }

      if (meta.submitAction === "update" && roomTypeToUpdate) {
        await updateRoomTypeAsync(formValues);
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
        <Button className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90 cursor-pointer">
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
            className="space-y-4"
            onSubmit={(e) => {
              e.preventDefault();
              e.stopPropagation();
            }}
          >
            <div className="mx-auto w-full max-w-sm">
              <DrawerHeader>
                <DrawerTitle>
                  {isUpdateRoomType ? "Update" : "Create"} Room Type
                </DrawerTitle>
                <DrawerDescription>Set room type details.</DrawerDescription>
              </DrawerHeader>
              <div className="p-4 pb-0">
                <div>
                  <label className="text-sm font-medium text-foreground block mb-1">
                    Type Name
                  </label>
                  <form.Field
                    name="name"
                    children={(field) => (
                      <>
                        <input
                          className="w-full px-3 py-2 rounded-lg border border-border bg-background text-foreground"
                          placeholder="e.g., Lecture Hall"
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
                          placeholder="e.g., Large classroom for lectures"
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
                          submitAction: isUpdateRoomType ? "update" : "create",
                          formAction: "close",
                        })
                      }
                    >
                      {isSubmitting ? (
                        <Loader2 />
                      ) : isUpdateRoomType ? (
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
