import type { Teacher } from "@/api/models/teacher";
import { DeleteAlertDialog } from "@/components/delete-alert-dialog";

interface DeleteTeacherAlertDialogProps {
  teacherToDelete?: Teacher;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirmDelete: (teacher: Teacher) => void;
}

export function DeleteTeacherAlertDialog({
  teacherToDelete,
  isOpen,
  onOpenChange,
  onConfirmDelete,
}: DeleteTeacherAlertDialogProps) {
  return (
    <DeleteAlertDialog
      entityToDelete={teacherToDelete}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      entityLabel="Teacher"
      getEntityName={(t) => `${t.firstName} ${t.lastName}`}
      onConfirmDelete={onConfirmDelete}
    />
  );
}
