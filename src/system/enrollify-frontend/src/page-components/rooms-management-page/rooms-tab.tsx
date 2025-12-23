import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { useState } from "react";
import { RoomForm } from "./room-form";
import { RoomsTable, type Room } from "./rooms-table";
import type { RoomType } from "../../api/models/room-type";

interface RoomsTabProps {
  rooms: Room[];
  roomTypes?: RoomType[];
}

export function RoomsTab({ roomTypes, rooms }: RoomsTabProps) {
  const [showForm, setShowForm] = useState(false);

  const handleDeleteRoom = (id: number) => {
    console.log(id);
    // setRooms(rooms.filter((room) => room.id !== id));
  };

  const handleEditRoom = (room: Room) => {
    console.log("[v0] Edit room:", room);
    // Implement edit functionality as needed
  };

  const roomTypeNames = roomTypes?.map((rt) => rt.name);

  return (
    <>
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div className="relative flex-1 max-w-md">
          {/* Search component can be added here if needed */}
        </div>
        <Button
          onClick={() => setShowForm(true)}
          className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90"
        >
          <Plus className="size-4" />
          Add Room
        </Button>
      </div>

      {showForm && (
        <RoomForm
          onClose={() => setShowForm(false)}
          onSave={(room) => {
            console.log(room);
            setShowForm(false);
          }}
          roomTypes={roomTypeNames}
        />
      )}

      <RoomsTable
        rooms={rooms}
        onEdit={handleEditRoom}
        onDelete={handleDeleteRoom}
      />
    </>
  );
}
