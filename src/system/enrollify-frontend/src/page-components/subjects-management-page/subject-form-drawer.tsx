import {
  createSubjectOptions,
  updateSubjectOptions,
} from "@/api/collections/subject-collection";
import type { RoomType } from "@/api/models/room-type";
import type { Subject } from "@/api/models/subject";
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

const subjectFormSchema = z.object({
  code: z
    .string()
    .min(3, "Subject code must be at least 3 characters")
    .max(20, "Subject code must not exceed 20 characters"),
  title: z.string().min(3, "Title must be at least 3 characters"),
  description: z.string().min(10, "Description must be at least 10 characters"),
  units: z
    .number()
    .min(1, "Units must be greater than 0")
    .max(12, "Units must not exceed 12"),
  preferRoomTypeId: z
    .number()
    .nonnegative("Room Type is required")
    .min(1, "Room Type is required"),
});
type SubjectFormData = z.infer<typeof subjectFormSchema>;

interface SubjectFormDrawerProps {
  roomTypes?: RoomType[];
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  subjectToUpdate?: Subject | null;
  onOpenChange: (isOpen: boolean) => void;
}

export function SubjectFormDrawer({
  roomTypes,
  isOpen,
  setIsOpen,
  subjectToUpdate,
  onOpenChange,
}: SubjectFormDrawerProps) {
  const defaultFormValues: SubjectFormData = {
    code: subjectToUpdate?.code ?? "",
    title: subjectToUpdate?.title ?? "",
    description: subjectToUpdate?.description ?? "",
    units: subjectToUpdate?.units ?? 1,
    preferRoomTypeId: subjectToUpdate?.preferRoomType?.id ?? 0,
  };

  const { mutateAsync: createNewSubjectAsync } = useMutation(
    createSubjectOptions()
  );
  const { mutateAsync: updateSubjectAsync } = useMutation(
    updateSubjectOptions(subjectToUpdate?.id ?? 0)
  );

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onSubmit: subjectFormSchema,
    },
    onSubmitMeta: defaultMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = subjectFormSchema.parse(value);

      if (meta.submitAction === "create") {
        await createNewSubjectAsync(formValues);
      } else if (meta.submitAction === "update") {
        await updateSubjectAsync(formValues);
      }

      if (meta.formAction === "close") {
        setIsOpen(false);
      }
      form.reset();
    },
  });

  const roomTypeOptions: SearchableSelectOption[] =
    roomTypes?.map((roomType) => ({
      value: roomType.id.toString(),
      label: roomType.name,
    })) ?? [];

  const isUpdatingSubject = !!subjectToUpdate;
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
          Add Subject
        </Button>
      </DrawerTrigger>
      {/*  className="sm:max-w-[600px]!" */}
      <DrawerContent>
        <AuthorizeView
          policy="canCreateSubject"
          unauthorized={
            <Unauthorized
              message="Your current role does not have the necessary permissions to create subject."
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
            <div className="w-full">
              <DrawerHeader>
                <DrawerTitle>
                  {isUpdatingSubject ? "Update" : "Create"} Subject
                </DrawerTitle>
                <DrawerDescription>Set subject details.</DrawerDescription>
              </DrawerHeader>
              <div className="p-4 pb-0">
                <div className="pb-4">
                  <form.Field
                    name="code"
                    children={(field) => (
                      <div className="grid w-full items-center gap-3">
                        <Label htmlFor={field.name}>Code:</Label>
                        <Input
                          type="text"
                          placeholder="Code:"
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
                    name="title"
                    children={(field) => (
                      <div className="grid w-full max-w-sm items-center gap-3">
                        <Label htmlFor={field.name}>Title:</Label>
                        <Input
                          type="text"
                          value={field.state.value}
                          placeholder="Title:"
                          onChange={(e) => field.handleChange(e.target.value)}
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
                    name="description"
                    children={(field) => (
                      <div className="grid w-full max-w-sm items-center gap-3">
                        <Label htmlFor={field.name}>Description:</Label>
                        <Input
                          type="text"
                          value={field.state.value}
                          placeholder="Description:"
                          onChange={(e) => field.handleChange(e.target.value)}
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
                    name="units"
                    children={(field) => (
                      <div className="grid w-full max-w-sm items-center gap-3">
                        <Label htmlFor={field.name}>Units:</Label>
                        <Input
                          type="number"
                          step="0.5"
                          value={field.state.value}
                          placeholder="Units:"
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
                    name="preferRoomTypeId"
                    children={(field) => (
                      <div className="grid w-full max-w-sm items-center gap-3">
                        <Label htmlFor={field.name}>Prefer Room Type:</Label>
                        <SearchableSelect
                          options={roomTypeOptions}
                          value={field.state.value.toString()}
                          onValueChange={(val) =>
                            field.handleChange(Number(val))
                          }
                          name={field.name}
                          placeholder="Select a prefer room type"
                          searchPlaceholder="Search prefer room types..."
                          emptyMessage="No prefer room type found"
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
                          submitAction: isUpdatingSubject ? "update" : "create",
                          formAction: "close",
                        })
                      }
                    >
                      {isSubmitting ? (
                        <Loader2 />
                      ) : isUpdatingSubject ? (
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
