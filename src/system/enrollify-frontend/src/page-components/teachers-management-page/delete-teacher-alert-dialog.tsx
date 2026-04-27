import type { Teacher } from "@/api/models/teacher";
import { DeleteAlertDialog } from "@/components/delete-alert-dialog";

interface DeleteTeacherAlertDialogProps {
  teacherToDelete?: Teacher;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeleteTeacherAlertDialog({
  teacherToDelete,
  isOpen,
  onOpenChange,
}: DeleteTeacherAlertDialogProps) {
  return (
    <DeleteAlertDialog
      entityToDelete={teacherToDelete}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      entityLabel="Teacher"
      getEntityName={(t) => `${t.firstName} ${t.lastName}`}
      onConfirmDelete={() => onOpenChange(false)}
      customDialogDescription={
        <span>
          Teacher deletion is not currently supported. Please contact your
          system administrator to remove teacher records.
        </span>
      }
    />
  );
}
