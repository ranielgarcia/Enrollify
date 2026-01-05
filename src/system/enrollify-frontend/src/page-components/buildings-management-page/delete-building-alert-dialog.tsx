import { deleteBuildingOptions } from "@/api/collections/building-collection";
import type { Building } from "@/api/models/building";
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

interface DeleteBuildingAlertDialogProps {
  buildingToDelete?: Building;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeleteBuildingAlertDialog({
  buildingToDelete,
  isOpen,
  onOpenChange,
}: DeleteBuildingAlertDialogProps) {
  const { mutateAsync: deleteBuildingAsync, isPending: isDeletingInProgress } =
    useMutation(deleteBuildingOptions(buildingToDelete?.id ?? 0));

  const handleContinueDelete = async () => {
    await deleteBuildingAsync(undefined);
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
            Deleting building <strong>{buildingToDelete?.name}</strong> <br />
            This action cannot be undone. This will permanently delete your
            building and remove your data from our servers.
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
