import { DeleteRoomOptions } from "@/api/collections/room-collection";
import type { Room } from "@/api/models/room";
import { DeleteAlertDialog } from "@/components/delete-alert-dialog";

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
  return (
    <DeleteAlertDialog
      entityToDelete={roomToDelete}
      isOpen={isOpen}
      onOpenChange={onOpenChange}
      entityLabel="Room"
      getEntityName={(r) => r.roomNumber}
      deleteMutationOptions={DeleteRoomOptions(roomToDelete?.id ?? 0)}
    />
  );
}
