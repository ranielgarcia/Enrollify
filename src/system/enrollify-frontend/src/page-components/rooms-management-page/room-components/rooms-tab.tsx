import { RoomsTable } from "./rooms-table";
import type { RoomType } from "../../../api/models/room-type";
import type { Room } from "@/api/models/room";
import type { Building } from "@/api/models/building";
import { RoomFormDrawer } from "./room-form-drawer";
import { DeleteRoomAlertDialog } from "./delete-room-alert-dialog";
import { useCrudState } from "@/hooks/use-crud-state";

interface RoomsTabProps {
  rooms?: Room[];
  roomTypes?: RoomType[];
  buildings?: Building[];
}

export function RoomsTab({ roomTypes, rooms, buildings }: RoomsTabProps) {
  const {
    isFormOpen,
    entityToEdit,
    entityToDelete,
    handleEdit,
    handleDelete,
    handleFormOpenChange,
    handleDeleteDialogOpenChange,
  } = useCrudState<Room>();

  return (
    <>
      <div className="flex justify-end">
        <RoomFormDrawer
          roomTypes={roomTypes}
          buildings={buildings}
          onOpenChange={handleFormOpenChange}
          roomToUpdate={entityToEdit}
          isOpen={isFormOpen}
          setIsOpen={handleFormOpenChange}
        />
      </div>
      <RoomsTable
        rooms={rooms}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      <DeleteRoomAlertDialog
        roomToDelete={entityToDelete}
        isOpen={!!entityToDelete}
        onOpenChange={handleDeleteDialogOpenChange}
      />
    </>
  );
}
