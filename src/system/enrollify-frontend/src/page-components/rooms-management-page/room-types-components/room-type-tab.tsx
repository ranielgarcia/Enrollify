import { RoomTypesTable } from "./room-types-table";
import { RoomTypeFormDrawer } from "./room-type-form-drawer";
import type { RoomType } from "../../../api/models/room-type";
import { DeleteRoomTypeAlertDialog } from "./delete-room-type-alert-dialog";
import { useCrudState } from "@/hooks/use-crud-state";

interface RoomTypeTabProps {
  roomTypes?: RoomType[];
}

export function RoomTypeTab({ roomTypes }: RoomTypeTabProps) {
  const {
    isFormOpen,
    entityToEdit,
    entityToDelete,
    handleEdit,
    handleDelete,
    handleFormOpenChange,
    handleDeleteDialogOpenChange,
  } = useCrudState<RoomType>();

  return (
    <>
      <div className="flex justify-end">
        <RoomTypeFormDrawer
          onOpenChange={handleFormOpenChange}
          roomTypeToUpdate={entityToEdit}
          isOpen={isFormOpen}
          setIsOpen={handleFormOpenChange}
        />
      </div>

      <RoomTypesTable
        roomTypes={roomTypes}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      <DeleteRoomTypeAlertDialog
        isOpen={!!entityToDelete}
        onOpenChange={handleDeleteDialogOpenChange}
        roomTypeToDelete={entityToDelete}
      />
    </>
  );
}
