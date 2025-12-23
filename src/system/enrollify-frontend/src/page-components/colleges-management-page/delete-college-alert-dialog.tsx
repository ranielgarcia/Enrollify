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
import { OverlayLoader } from "@/components/app-loading-overlay";
import type { College } from "@/api/models/college";
import { useDeleteCollege } from "@/api/collections/college-collection";

interface DeleteCollegeAlertDialogProps {
  collegeToDelete?: College;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeleteCollegeAlertDialog({
  collegeToDelete,
  isOpen,
  onOpenChange,
}: DeleteCollegeAlertDialogProps) {
  const { mutateAsync: deleteCollegeAsync, isPending: isDeletingInProgress } =
    useDeleteCollege(collegeToDelete?.id ?? 0);

  const handleContinueDelete = async () => {
    await deleteCollegeAsync(undefined);
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
            Deleting college <strong>{collegeToDelete?.name}</strong> <br />
            This action cannot be undone. This will permanently delete your
            college and remove your data from our servers.
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
