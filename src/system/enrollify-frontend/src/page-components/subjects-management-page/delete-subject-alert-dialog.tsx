import { deleteSubjectOptions } from "@/api/collections/subject-collection";
import type { Subject } from "@/api/models/subject";
import { OverlayLoader } from "@/components/app-loading-overlay";
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
import { useMutation } from "@tanstack/react-query";

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
  const { mutateAsync: deleteSubjectAsync, isPending: isDeletingInProgress } =
    useMutation(deleteSubjectOptions(subjectToDelete?.id ?? 0));

  const handleContinueDelete = async () => {
    await deleteSubjectAsync(undefined);
  };

  return (
    <AlertDialog onOpenChange={onOpenChange} open={isOpen}>
      <AlertDialogContent>
        <OverlayLoader
          isLoading={isDeletingInProgress}
          text={isDeletingInProgress ? "Deleting..." : "Processing.."}
          size="sm"
        />
        <AlertDialogHeader>
          <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>

          <AlertDialogDescription>
            Deleting subject <strong>{subjectToDelete?.title}</strong> <br />
            This action cannot be undone. This will permanently delete your
            subject and remove your data from our servers.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel onClick={() => onOpenChange(false)}>
            Cancel
          </AlertDialogCancel>
          <AlertDialogAction onClick={async () => await handleContinueDelete()}>
            Continue
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
