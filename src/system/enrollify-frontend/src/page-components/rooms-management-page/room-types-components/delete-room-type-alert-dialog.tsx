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
import type { RoomType } from "../../../api/models/room-type";
import { OverlayLoader } from "@/components/app-loading-overlay";
import {
  countRoomsByRoomTypeOptions,
  deleteRoomTypeOptions,
} from "@/api/collections/room-type-collection";
import { useMutation, useQuery } from "@tanstack/react-query";

interface DeleteRoomTypeAlertDialogProps {
  roomTypeToDelete?: RoomType;
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
}

export function DeleteRoomTypeAlertDialog({
  roomTypeToDelete,
  isOpen,
  onOpenChange,
}: DeleteRoomTypeAlertDialogProps) {
  const { data: associatedRoomsCount, isPending: isLoadingAssociatedRooms } =
    useQuery(countRoomsByRoomTypeOptions(roomTypeToDelete?.id ?? 0));

  const { mutateAsync: deleteRoomType, isPending: isDeletingInProgress } =
    useMutation(deleteRoomTypeOptions(roomTypeToDelete?.id ?? 0));

  const handleContinueDelete = async () => await deleteRoomType(undefined);

  return (
    <AlertDialog onOpenChange={onOpenChange} open={isOpen}>
      <AlertDialogContent>
        <OverlayLoader
          isLoading={isLoadingAssociatedRooms || isDeletingInProgress}
          text={isDeletingInProgress ? "Deleting..." : "Processing.."}
          size="sm"
        />
        <AlertDialogHeader>
          {associatedRoomsCount && associatedRoomsCount > 0 ? (
            <AlertDialogTitle className="text-danger">
              Unable to Delete Room Type
            </AlertDialogTitle>
          ) : (
            <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
          )}

          {associatedRoomsCount && associatedRoomsCount > 0 ? (
            <AlertDialogDescription>
              This room type cannot be deleted because it has{" "}
              <strong>{associatedRoomsCount}</strong> room(s) associated with
              it. Please reassign or remove these rooms from this room type
              before deleting.
            </AlertDialogDescription>
          ) : (
            <AlertDialogDescription>
              Deleting room type <strong>{roomTypeToDelete?.name}</strong>{" "}
              <br />
              This action cannot be undone. This will permanently delete your
              room type and remove your data from our servers.
            </AlertDialogDescription>
          )}
        </AlertDialogHeader>
        <AlertDialogFooter>
          {associatedRoomsCount && associatedRoomsCount > 0 ? (
            <>
              <AlertDialogCancel onClick={() => onOpenChange(false)}>
                Close
              </AlertDialogCancel>
            </>
          ) : (
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
          )}
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
