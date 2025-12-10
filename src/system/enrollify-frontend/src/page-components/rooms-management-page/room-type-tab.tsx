import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { useState } from "react";
import { RoomTypesTable, type RoomType } from "./room-types-table";
import { RoomTypeForm } from "./room-type-form";

interface RoomTypeTabProps {
  roomTypes: RoomType[];
}

export function RoomTypeTab({ roomTypes }: RoomTypeTabProps) {
  const [showTypeForm, setShowTypeForm] = useState(false);
  return (
    <>
      <div className="flex justify-end">
        <Button
          onClick={() => {
            setShowTypeForm(true);
          }}
          className="gap-2 bg-primary text-primary-foreground hover:bg-primary/90"
        >
          <Plus className="size-4" />
          Add Room Type
        </Button>
      </div>

      {showTypeForm && <RoomTypeForm />}

      <RoomTypesTable
        roomTypes={roomTypes}
        onEdit={(roomType) => console.log(roomType)}
        onDelete={(id) => console.log(id)}
      />
    </>
  );
}
