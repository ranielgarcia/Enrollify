import { deleteSubjectOptions } from "@/api/collections/subject-collection";
import type { Subject } from "@/api/models/subject";
import { DeleteAlertDialog } from "@/components/delete-alert-dialog";

interface DeleteSubjeectAlertDialogProps {
  subjectToDelete?: Subject;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeleteSubjectAlertDialog({
  subjectToDelete,
  isOpen,
  onOpenChange,
}: DeleteSubjeectAlertDialogProps) {
  return (
    <DeleteAlertDialog
      entityToDelete={subjectToDelete}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      entityLabel="Subject"
      getEntityName={(s) => s.title}
      deleteMutationOptions={deleteSubjectOptions(subjectToDelete?.id ?? 0)}
    />
  );
}
