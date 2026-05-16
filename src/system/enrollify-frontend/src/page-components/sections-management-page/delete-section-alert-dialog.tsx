import type { ClassSection } from "@/api/models/class-section";
import { DeleteAlertDialog } from "@/components/delete-alert-dialog";
import { deleteSectionOptions } from "@/api/collections/class-section-collection";
import { useMutation } from "@tanstack/react-query";

interface DeleteSectionAlertDialogProps {
  sectionToDelete?: ClassSection;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeleteSectionAlertDialog({
  sectionToDelete,
  isOpen,
  onOpenChange,
}: DeleteSectionAlertDialogProps) {
  const { mutateAsync } = useMutation(
    deleteSectionOptions(sectionToDelete?.id ?? 0),
  );

  return (
    <DeleteAlertDialog
      entityToDelete={sectionToDelete}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      entityLabel="Class Section"
      getEntityName={(s) => s.name}
      deleteMutationOptions={
        {
          mutationFn: async () => mutateAsync(undefined as never),
        } as any
      }
    />
  );
}
