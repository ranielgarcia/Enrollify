import type { Teacher } from "@/api/models/teacher";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

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
  const handleContinueDelete = () => {
    if (teacherToDelete) {
      onConfirmDelete(teacherToDelete);
      onOpenChange(false);
    }
  };

  return (
    <AlertDialog onOpenChange={onOpenChange} open={isOpen}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
          <AlertDialogDescription>
            Deleting teacher{" "}
            <strong>
              {teacherToDelete?.firstName} {teacherToDelete?.lastName}
            </strong>
            <br />
            This action cannot be undone. This will permanently delete the
            teacher record and remove their data from our servers.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel onClick={() => onOpenChange(false)}>
            Cancel
          </AlertDialogCancel>
          <AlertDialogAction onClick={handleContinueDelete}>
            Continue
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
