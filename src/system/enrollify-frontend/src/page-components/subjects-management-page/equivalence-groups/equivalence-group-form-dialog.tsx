import {
  createSubjectEquivalenceGroupOptions,
  updateSubjectEquivalenceGroupOptions,
} from "@/api/collections/subject-equivalence-group-collection";
import type { SubjectEquivalenceGroup } from "@/api/models/subject-equivalence";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { FormField } from "@/components/form/form-field";
import { Unauthorized } from "@/components/unauthorized";
import { AuthorizeView } from "@/infrastructure/authorization/components/AuthorizeView";
import { useForm } from "@tanstack/react-form";
import { useMutation } from "@tanstack/react-query";
import { Loader2 } from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import z from "zod";

const groupFormSchema = z.object({
  name: z
    .string()
    .min(3, "Name must be at least 3 characters")
    .max(255, "Name must not exceed 255 characters"),
});

type GroupFormData = z.infer<typeof groupFormSchema>;

interface EquivalenceGroupFormDialogProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  groupToEdit?: SubjectEquivalenceGroup | null;
}

export function EquivalenceGroupFormDialog({
  isOpen,
  onOpenChange,
  groupToEdit,
}: EquivalenceGroupFormDialogProps) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const isEditing = !!groupToEdit;

  const { mutateAsync: createSubjectEquivalenceGroupAsync } = useMutation(
    createSubjectEquivalenceGroupOptions(),
  );
  const { mutateAsync: updateSubjectEquivalenceGroupAsync } = useMutation(
    updateSubjectEquivalenceGroupOptions(groupToEdit?.id ?? 0),
  );

  const handleFormSubmit = async (data: GroupFormData) => {
    if (groupToEdit) {
      await updateSubjectEquivalenceGroupAsync({ name: data.name });
      toast.success("Equivalence group updated successfully");
    } else {
      await createSubjectEquivalenceGroupAsync({ name: data.name });
      toast.success("Equivalence group created successfully");
    }
  };

  const form = useForm({
    defaultValues: {
      name: groupToEdit?.name ?? "",
    },
    validators: {
      onBlur: groupFormSchema,
      onSubmit: groupFormSchema,
    },
    onSubmit: async ({ value }) => {
      setIsSubmitting(true);
      try {
        await handleFormSubmit(value);
        onOpenChange(false);
      } finally {
        setIsSubmitting(false);
      }
    },
  });

  useEffect(() => {
    if (isOpen) {
      form.reset({
        name: groupToEdit?.name ?? "",
      });
    }
  }, [isOpen, groupToEdit, form]);

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <form
          onSubmit={(e) => {
            e.preventDefault();
            e.stopPropagation();
            form.handleSubmit();
          }}
        >
          <DialogHeader>
            <DialogTitle>
              {isEditing
                ? "Edit Equivalence Group"
                : "Create Equivalence Group"}
            </DialogTitle>
            <DialogDescription>
              {isEditing
                ? "Update the equivalence group name."
                : "Create a new equivalence group to link related subjects across colleges."}
            </DialogDescription>
          </DialogHeader>

          <AuthorizeView
            policy={
              isEditing
                ? "canUpdateSubjectEquivalenceGroup"
                : "canCreateSubjectEquivalenceGroup"
            }
            unauthorized={
              <Unauthorized
                message="Your current role does not have the necessary permissions to create subject equivalence group."
                buttonLabel="Back to Home"
                backCallback={() => onOpenChange(false)}
                redirectOptions={{
                  to: "/portal",
                }}
              />
            }
          >
            <div className="grid gap-4 py-4">
              <form.Field
                name="name"
                children={(field) => (
                  <FormField
                    field={field}
                    label="Group Name"
                    required
                    placeholder="e.g., Programming Fundamentals"
                    autoFocus
                  />
                )}
              />
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
                disabled={isSubmitting}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting && (
                  <Loader2 className="mr-2 size-4 animate-spin" />
                )}
                {isEditing ? "Save Changes" : "Create Group"}
              </Button>
            </DialogFooter>
          </AuthorizeView>
        </form>
      </DialogContent>
    </Dialog>
  );
}
