import type { RoomType } from "../../../api/models/room-type";
import {
  countRoomsByRoomTypeOptions,
  deleteRoomTypeOptions,
} from "@/api/collections/room-type-collection";
import { useQuery } from "@tanstack/react-query";
import { DeleteAlertDialog } from "@/components/delete-alert-dialog";

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

  return (
    <DeleteAlertDialog
      entityToDelete={roomTypeToDelete}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      entityLabel="Room Type"
      getEntityName={(rt) => rt.name}
      deleteMutationOptions={deleteRoomTypeOptions(
        roomTypeToDelete?.id ?? 0
      )}
      validationQuery={{
        data: associatedRoomsCount,
        isPending: isLoadingAssociatedRooms,
      }}
      validationMessage={`This room type cannot be deleted because it has ${associatedRoomsCount} room(s) associated with it. Please reassign or remove these rooms from this room type before deleting.`}
    />
  );
}
