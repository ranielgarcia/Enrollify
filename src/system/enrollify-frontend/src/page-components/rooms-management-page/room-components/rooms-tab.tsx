import { useState } from "react";
import { RoomsTable } from "./rooms-table";
import type { RoomType } from "../../../api/models/room-type";
import type { Room } from "@/api/models/room";
import type { Building } from "@/api/models/building";
import { RoomFormDrawer } from "./room-form-drawer";
import { DeleteRoomAlertDialog } from "./delete-room-alert-dialog";

interface RoomsTabProps {
  rooms?: Room[];
  roomTypes?: RoomType[];
  buildings?: Building[];
}

export function RoomsTab({ roomTypes, rooms, buildings }: RoomsTabProps) {
  const [isFormOpen, setIsFormOpen] = useState<boolean>(false);
  const [roomToEdit, setRoomToEdit] = useState<Room | undefined>();
  const [roomToDelete, setRoomToDelete] = useState<Room | undefined>();

  const handleEdit = (room: Room) => {
    setRoomToEdit(room);
    setIsFormOpen(true);
  };

  const handleDrawerOnOpenChange = (open: boolean) => {
    setRoomToEdit(undefined);
    setIsFormOpen(open);
  };

  const handleDeleteAlertDialogOnOpenChange = (open: boolean) => {
    if (!open) {
      setRoomToDelete(undefined);
    }
  };

  return (
    <>
      <div className="flex justify-end">
        <RoomFormDrawer
          roomTypes={roomTypes}
          buildings={buildings}
          onOpenChange={handleDrawerOnOpenChange}
          roomToUpdate={roomToEdit}
          isOpen={isFormOpen}
          setIsOpen={setIsFormOpen}
        />
      </div>
      <RoomsTable
        rooms={rooms}
        onEdit={handleEdit}
        onDelete={setRoomToDelete}
      />

      <DeleteRoomAlertDialog
        roomToDelete={roomToDelete}
        isOpen={!!roomToDelete}
        onOpenChange={handleDeleteAlertDialogOnOpenChange}
      />
    </>
  );
}
