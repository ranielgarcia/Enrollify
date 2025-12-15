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
import { toast } from "sonner";
import useAppMutation from "@/hooks/use-app-mutation-v2";
import { AxiosError } from "axios";
import {
  formatValidationErrors,
  parseApiError,
  type ProblemDetails,
} from "@/lib/axios-utils";
import type { RoomType } from "./models/RoomType";

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
  onSuccessful?: () => void;
}

export function RoomTypeFormDrawer({
  roomTypeToUpdate,
  onOpenChange,
  isOpen = false,
  setIsOpen,
  onSuccessful,
}: RoomTypeFormDrawerProps) {
  const defaultFormValues: RoomTypeForm = {
    name: roomTypeToUpdate?.name ?? "",
    description: roomTypeToUpdate?.description ?? "",
  };

  const { mutateAsync: createNewRoomTypeAsync } = useAppMutation({
    httpVerb: "post",
    path: "/api/room-types",
    mutationKey: "create-new-room-type",
  });

  const { mutateAsync: updateRoomTypeAsync } = useAppMutation({
    httpVerb: "put",
    path: `/api/room-types`,
    params: roomTypeToUpdate
      ? {
          id: roomTypeToUpdate.id,
        }
      : undefined,
    mutationKey: `update-room-type-${roomTypeToUpdate?.id}`,
  });

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onChange: roomTypeSchema,
    },
    onSubmitMeta: defaultMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = roomTypeSchema.parse(value);

      try {
        if (meta.submitAction === "create") {
          await createNewRoomTypeAsync(formValues);
          toast.success("Room type created successfully");
        }

        if (meta.submitAction === "update" && roomTypeToUpdate) {
          await updateRoomTypeAsync({
            name: formValues.name,
            description: formValues.description,
          });
          toast.success("Room type updated successfully");
        }

        if (meta.formAction === "close") {
          setIsOpen(false);
          if (onSuccessful) onSuccessful();
          form.reset();
        } else {
          form.reset();
        }
      } catch (err) {
        const axiosError = err as AxiosError<ProblemDetails>;
        const parsed = parseApiError(axiosError);

        // Show error toast with title and detail
        toast.error(parsed.title, {
          description: parsed.validationErrors
            ? formatValidationErrors(parsed.validationErrors)
            : parsed.detail || "Please try again.",
        });
      }
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
        <form
          className="space-y-4"
          onSubmit={(e) => {
            e.preventDefault();
            e.stopPropagation();
          }}
        >
          <div className="mx-auto w-full max-w-sm">
            <DrawerHeader>
              <DrawerTitle>Create Room Type</DrawerTitle>
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
      </DrawerContent>
    </Drawer>
  );
}
