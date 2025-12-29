import { deleteDepartmentOptions } from "@/api/collections/department-collection";
import type { Department } from "@/api/models/department";
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

interface DeleteDepartmentAlertDialogProps {
  departmentToDelete?: Department;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeleteDepartmentAlertDialog({
  departmentToDelete,
  isOpen,
  onOpenChange,
}: DeleteDepartmentAlertDialogProps) {
  const {
    mutateAsync: deleteDepartmentAsync,
    isPending: isDeletingInProgress,
  } = useMutation(deleteDepartmentOptions(departmentToDelete?.id ?? 0));

  const handleContinueDelete = async () => {
    await deleteDepartmentAsync(undefined);
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
            Deleting department <strong>{departmentToDelete?.name}</strong>{" "}
            <br />
            This action cannot be undone. This will permanently delete your
            department and remove your data from our servers.
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
