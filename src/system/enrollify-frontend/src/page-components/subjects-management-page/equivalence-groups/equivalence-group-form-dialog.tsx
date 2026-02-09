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
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useForm } from "@tanstack/react-form";
import { Loader2 } from "lucide-react";
import { useEffect, useState } from "react";
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
  onSubmit: (data: GroupFormData) => Promise<void>;
}

export function EquivalenceGroupFormDialog({
  isOpen,
  onOpenChange,
  groupToEdit,
  onSubmit,
}: EquivalenceGroupFormDialogProps) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const isEditing = !!groupToEdit;

  const form = useForm({
    defaultValues: {
      name: groupToEdit?.name ?? "",
    },
    validators: {
      onSubmit: groupFormSchema,
    },
    onSubmit: async ({ value }) => {
      setIsSubmitting(true);
      try {
        await onSubmit(value);
        onOpenChange(false);
      } finally {
        setIsSubmitting(false);
      }
    },
  });

  // Reset form when dialog opens/closes or when editing different group
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

          <div className="grid gap-4 py-4">
            <form.Field
              name="name"
              children={(field) => (
                <div className="grid gap-2">
                  <Label htmlFor={field.name}>Group Name</Label>
                  <Input
                    id={field.name}
                    placeholder="e.g., Programming Fundamentals"
                    value={field.state.value}
                    onChange={(e) => field.handleChange(e.target.value)}
                    onBlur={field.handleBlur}
                  />
                  {field.state.meta.errors.length > 0 && (
                    <p className="text-sm text-destructive">
                      {field.state.meta.errors.join(", ")}
                    </p>
                  )}
                </div>
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
              {isSubmitting && <Loader2 className="mr-2 size-4 animate-spin" />}
              {isEditing ? "Save Changes" : "Create Group"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
