import { RoomTypesTable } from "./room-types-table";
import { RoomTypeFormDrawer } from "./room-type-form-drawer";
import { useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import type { RoomType } from "./models/RoomType";
import { DeleteRoomTypeAlertDialog } from "./delete-room-type-alert-dialog";

interface RoomTypeTabProps {
  roomTypes: RoomType[];
}

export function RoomTypeTab({ roomTypes }: RoomTypeTabProps) {
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const queryClient = useQueryClient();
  const [roomTypeToEdit, setRoomTypeToEdit] = useState<RoomType | undefined>();
  const [roomTypeToDelete, setRoomTypeToDelete] = useState<
    RoomType | undefined
  >();

  const refreshRoomTypesTable = () => {
    queryClient.invalidateQueries({ queryKey: ["/api/room-types"] });
  };

  const handleEdit = (roomType: RoomType) => {
    setRoomTypeToEdit(roomType);
    setIsFormOpen(true);
  };

  const handleDrawerOnOpenChange = (open: boolean) => {
    setRoomTypeToEdit(undefined);
    setIsFormOpen(open);
  };

  const handleDeleteAlertDialogOnOpenChange = (open: boolean) => {
    if (!open) {
      setRoomTypeToDelete(undefined);
    }
  };

  return (
    <>
      <div className="flex justify-end">
        <RoomTypeFormDrawer
          onOpenChange={handleDrawerOnOpenChange}
          roomTypeToUpdate={roomTypeToEdit}
          isOpen={isFormOpen}
          setIsOpen={setIsFormOpen}
          onSuccessful={refreshRoomTypesTable}
        />
      </div>

      <RoomTypesTable
        roomTypes={roomTypes}
        onEdit={handleEdit}
        onDelete={setRoomTypeToDelete}
      />

      <DeleteRoomTypeAlertDialog
        isOpen={!!roomTypeToDelete}
        openOpenChange={handleDeleteAlertDialogOnOpenChange}
        roomTypeToDelete={roomTypeToDelete}
        onSuccessful={refreshRoomTypesTable}
      />
    </>
  );
}
