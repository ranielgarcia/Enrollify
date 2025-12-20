import { RoomTypesTable } from "./room-types-table";
import { RoomTypeFormDrawer } from "./room-type-form-drawer";
import { useState } from "react";
import type { RoomType } from "./models/RoomType";
import { DeleteRoomTypeAlertDialog } from "./delete-room-type-alert-dialog";

interface RoomTypeTabProps {
  roomTypes: RoomType[];
  refreshRoomTypesTable: () => void;
}

export function RoomTypeTab({
  roomTypes,
  refreshRoomTypesTable,
}: RoomTypeTabProps) {
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [roomTypeToEdit, setRoomTypeToEdit] = useState<RoomType | undefined>();
  const [roomTypeToDelete, setRoomTypeToDelete] = useState<
    RoomType | undefined
  >();

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
        onOpenChange={handleDeleteAlertDialogOnOpenChange}
        roomTypeToDelete={roomTypeToDelete}
        onSuccessful={refreshRoomTypesTable}
      />
    </>
  );
}
