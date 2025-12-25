import { DeleteRoomOptions } from "@/api/collections/room-collection";
import type { Room } from "@/api/models/room";
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

interface DeleteRoomAlertDialogProps {
  roomToDelete?: Room;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeleteRoomAlertDialog({
  roomToDelete,
  isOpen,
  onOpenChange,
}: DeleteRoomAlertDialogProps) {
  const { mutateAsync: deleteRoomAsync } = useMutation(
    DeleteRoomOptions(roomToDelete?.id ?? 0)
  );
  const handleContinueDelete = async () => await deleteRoomAsync(undefined);

  return (
    <AlertDialog onOpenChange={onOpenChange} open={isOpen}>
      <AlertDialogContent>
        {/* <OverlayLoader
          isLoading={isLoadingAssociatedRooms || isDeletingInProgress}
          text={isDeletingInProgress ? "Deleting..." : "Processing.."}
          size="sm"
        /> */}
        <AlertDialogHeader>
          <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
          <AlertDialogDescription>
            Deleting room type <strong>{roomToDelete?.roomNumber}</strong>{" "}
            <br />
            This action cannot be undone. This will permanently delete your room
            type and remove your data from our servers.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <>
            <AlertDialogCancel onClick={() => onOpenChange(false)}>
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={async () => await handleContinueDelete()}
            >
              Continue
            </AlertDialogAction>
          </>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
