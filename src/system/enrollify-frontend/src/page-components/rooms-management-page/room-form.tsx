import type React from "react";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { X } from "lucide-react";
import type { Room } from "./rooms-table";

interface RoomFormProps {
  onClose: () => void;
  onSave: (room: Room) => void;
  roomTypes?: string[];
}

export function RoomForm({ onClose, roomTypes = [] }: RoomFormProps) {
  const [formData, setFormData] = useState({
    roomNumber: "",
    building: "",
    capacity: 30,
    type: roomTypes[0] || "Lecture Hall",
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // onSave({
    //   id: Date.now().toString(),
    //   ...formData,
    // });
    setFormData({
      roomNumber: "",
      building: "",
      capacity: 30,
      type: roomTypes[0] || "Lecture Hall",
    });
  };

  // const availableRoomTypes =
  //   roomTypes.length > 0
  //     ? roomTypes
  //     : ["Lecture Hall", "Lab", "Seminar Room", "Tutorial Room", "Auditorium"];

  return (
    <Card className="border-2 border-accent">
      <CardHeader className="flex flex-row items-center justify-between space-y-0">
        <CardTitle>Add New Room</CardTitle>
        <Button variant="ghost" size="sm" onClick={onClose}>
          <X className="size-4" />
        </Button>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="text-sm font-medium text-foreground block mb-1">
                Room Number
              </label>
              <Input
                placeholder="e.g., 101"
                value={formData.roomNumber}
                onChange={(e) =>
                  setFormData({ ...formData, roomNumber: e.target.value })
                }
                required
              />
            </div>
            <div>
              <label className="text-sm font-medium text-foreground block mb-1">
                Building
              </label>
              <Input
                placeholder="e.g., Science Building"
                value={formData.building}
                onChange={(e) =>
                  setFormData({ ...formData, building: e.target.value })
                }
                required
              />
            </div>
            <div>
              <label className="text-sm font-medium text-foreground block mb-1">
                Capacity
              </label>
              <Input
                type="number"
                min="1"
                max="500"
                value={formData.capacity}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    capacity: Number.parseInt(e.target.value),
                  })
                }
              />
            </div>
            <div>
              <label className="text-sm font-medium text-foreground block mb-1">
                Room Type
              </label>
              {/* <select
                name="roomType"
                value={formData.type}
                onChange={(e) =>
                  setFormData({ ...formData, type: e.target.value })
                }
                className="w-full px-3 py-2 rounded-lg border border-border bg-background text-foreground"
              >
                {availableRoomTypes.map((type) => (
                  <option key={type} value={type}>
                    {type}
                  </option>
                ))}
              </select> */}
            </div>
          </div>
          <div className="flex gap-2 justify-end pt-4">
            <Button variant="outline" type="button" onClick={onClose}>
              Cancel
            </Button>
            <Button
              type="submit"
              className="bg-primary text-primary-foreground hover:bg-primary/90"
            >
              Save Room
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
