import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { RoomsTab } from "./room-components/rooms-tab";
import { RoomTypeTab } from "./room-types-components/room-type-tab";
import { getAllRoomTypesOptions } from "@/api/collections/room-type-collection";
import { useSuspenseQuery } from "@tanstack/react-query";
import { getAllRooms } from "@/api/collections/room-collection";
import { getAllBuildingsOptions } from "@/api/collections/building-collection";
import { ManagementPageLayout } from "@/components/management-page-layout";
import { useState } from "react";

export default function RoomsPage() {
  const { data: rooms } = useSuspenseQuery(getAllRooms());
  const { data: roomTypes } = useSuspenseQuery(getAllRoomTypesOptions());
  const { data: buildings } = useSuspenseQuery(getAllBuildingsOptions());

  const [activeTab, setActiveTab] = useState(
    () => sessionStorage.getItem("rooms-tab") ?? "rooms",
  );

  const handleTabChange = (value: string) => {
    setActiveTab(value);
    sessionStorage.setItem("rooms-tab", value);
  };

  return (
    <ManagementPageLayout
      title="Room Management"
      description="Manage classroom and facility resources"
    >
      <Tabs
        value={activeTab}
        onValueChange={handleTabChange}
        className="space-y-4"
      >
        <TabsList>
          <TabsTrigger value="rooms">Rooms</TabsTrigger>
          <TabsTrigger value="room-types">Room Types</TabsTrigger>
        </TabsList>

        <TabsContent value="rooms" className="space-y-4">
          <RoomsTab
            rooms={rooms}
            roomTypes={roomTypes}
            buildings={buildings}
          />
        </TabsContent>

        <TabsContent value="room-types" className="space-y-4">
          <RoomTypeTab roomTypes={roomTypes ?? []} />
        </TabsContent>
      </Tabs>
    </ManagementPageLayout>
  );
}
