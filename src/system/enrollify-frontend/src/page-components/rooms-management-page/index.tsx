import { useState } from "react";
import { type RoomType } from "./room-types-table";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { RoomsTab } from "./rooms-tab";
import type { Room } from "./rooms-table";
import { RoomTypeTab } from "./room-type-tab";

export default function RoomsPage() {
  const [rooms] = useState<Room[]>([
    {
      id: 1,
      roomNumber: "101",
      building: "Science Building",
      capacity: 30,
      type: "Lecture Hall",
      college: "CCS",
    },
    {
      id: 2,
      roomNumber: "102",
      building: "Science Building",
      capacity: 50,
      type: "Lecture Hall",
      college: "CCS",
    },
    {
      id: 3,
      roomNumber: "201",
      building: "Engineering Building",
      capacity: 25,
      type: "Lab",
      college: "CCS",
    },
    {
      id: 4,
      roomNumber: "301",
      building: "Arts Building",
      capacity: 40,
      type: "Seminar Room",
      college: "CCS",
    },
    {
      id: 5,
      roomNumber: "501",
      building: "Main Building",
      capacity: 200,
      type: "Auditorium",
      college: "CCS",
    },
    {
      id: 6,
      roomNumber: "202",
      building: "Engineering Building",
      capacity: 20,
      type: "Tutorial Room",
      college: "CCS",
    },
  ]);

  const [roomTypes] = useState<RoomType[]>([
    {
      id: "1",
      name: "Lecture Hall",
      description: "Large classroom for lectures",
    },
    {
      id: "2",
      name: "Lab",
      description: "Equipment and experiment space",
    },
    {
      id: "3",
      name: "Seminar Room",
      description: "Interactive discussion space",
    },
    {
      id: "4",
      name: "Tutorial Room",
      description: "Small group study room",
    },
    {
      id: "5",
      name: "Auditorium",
      description: "Large assembly hall",
    },
  ]);

  return (
    <main>
      <div className="p-4 md:p-8">
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-foreground mb-2">
            Room Management
          </h1>
          <p className="text-muted-foreground">
            Manage classroom and facility resources
          </p>
        </div>

        <Tabs defaultValue="rooms" className="space-y-4">
          <TabsList>
            <TabsTrigger value="rooms">Rooms</TabsTrigger>
            <TabsTrigger value="room-types">Room Types</TabsTrigger>
          </TabsList>

          <TabsContent value="rooms" className="space-y-4">
            <RoomsTab rooms={rooms} roomTypes={roomTypes} />
          </TabsContent>

          <TabsContent value="room-types" className="space-y-4">
            <RoomTypeTab roomTypes={roomTypes} />
          </TabsContent>
        </Tabs>
      </div>
    </main>
  );
}
