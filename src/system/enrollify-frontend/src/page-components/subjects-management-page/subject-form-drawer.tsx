import {
  createSubjectOptions,
  updateSubjectOptions,
} from "@/api/collections/subject-collection";
import type { RoomType } from "@/api/models/room-type";
import type { Subject } from "@/api/models/subject";
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
    createSubjectOptions(),
  );
  const { mutateAsync: updateSubjectAsync } = useMutation(
    updateSubjectOptions(subjectToUpdate?.id ?? 0),
  );

  const form = useForm({
    defaultValues: defaultFormValues,
    validators: {
      onBlur: subjectFormSchema,
      onSubmit: subjectFormSchema,
    },
    onSubmitMeta: defaultFormMeta as FormMeta,
    onSubmit: async ({ value, meta }) => {
      const formValues = subjectFormSchema.parse(value);

      if (meta.submitAction === "create") {
        await createNewSubjectAsync(formValues);
        toast.success("Subject created successfully");
      } else if (meta.submitAction === "update") {
        await updateSubjectAsync(formValues);
        toast.success("Subject updated successfully");
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
      <DrawerContent>
        <AuthorizeView
          policy={isUpdatingSubject ? "canUpdateSubject" : "canCreateSubject"}
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
            className="flex flex-col overflow-hidden h-full"
            onSubmit={(e) => {
              e.preventDefault();
              e.stopPropagation();
            }}
          >
            <DrawerHeader className="border-b pb-4">
              <DrawerTitle>
                {isUpdatingSubject ? "Update" : "New"} Subject
              </DrawerTitle>
              <DrawerDescription>
                Fill in the details below to{" "}
                {isUpdatingSubject ? "update this" : "create a new"} subject.
              </DrawerDescription>
            </DrawerHeader>
            <div className="flex-1 overflow-y-auto px-4 py-5 space-y-6">
              <FormSection title="Identification">
                <div className="grid grid-cols-2 gap-4">
                  <form.Field
                    name="code"
                    children={(field) => (
                      <FormField
                        field={field}
                        label="Code"
                        required
                        hint="e.g. CS101, MATH201"
                        autoFocus
                      />
                    )}
                  />
                  <form.Field
                    name="units"
                    children={(field) => (
                      <FormField
                        field={field}
                        label="Units"
                        type="number"
                        required
                      />
                    )}
                  />
                </div>
                <form.Field
                  name="title"
                  children={(field) => (
                    <FormField
                      field={field}
                      label="Title"
                      required
                      hint="Full subject title"
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

              <FormSection title="Classification">
                <form.Field
                  name="preferRoomTypeId"
                  children={(field) => (
                    <FormSelectField
                      field={field}
                      label="Preferred Room Type"
                      required
                      options={roomTypeOptions}
                      placeholder="Select a room type"
                      searchPlaceholder="Search room types..."
                      emptyMessage="No room type found"
                    />
                  )}
                />
              </FormSection>
            </div>

            <FormDrawerFooter
              form={form}
              isUpdate={isUpdatingSubject}
              onCancel={() => setIsOpen(false)}
              entityLabel="Subject"
              showSaveAndAddAnother
            />
          </form>
        </AuthorizeView>
      </DrawerContent>
    </Drawer>
  );
}
