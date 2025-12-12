import { RoomTypesTable, type RoomType } from "./room-types-table";
import { RoomTypeFormDrawer } from "./room-type-form-drawer";

interface RoomTypeTabProps {
  roomTypes: RoomType[];
}

export function RoomTypeTab({ roomTypes }: RoomTypeTabProps) {
  return (
    <>
      <div className="flex justify-end">
        <RoomTypeFormDrawer />
      </div>

      <RoomTypesTable
        roomTypes={roomTypes}
        onEdit={(roomType) => console.log(roomType)}
        onDelete={(id) => console.log(id)}
      />
    </>
  );
}
