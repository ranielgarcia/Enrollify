import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { RoomsTab } from "./room-components/rooms-tab";
import { RoomTypeTab } from "./room-types-components/room-type-tab";
import { OverlayLoader } from "@/components/app-loading-overlay";
import { getAllRoomTypesOptions } from "@/api/collections/room-type-collection";
import { useQuery } from "@tanstack/react-query";
import { getAllRooms } from "@/api/collections/room-collection";
import { getAllBuildingsOptions } from "@/api/collections/building-collection";

export default function RoomsPage() {
  const { data: rooms, isPending: isLoadingRooms } = useQuery(getAllRooms());
  const { data: roomTypes, isPending: isLoadingRoomTypes } = useQuery(
    getAllRoomTypesOptions()
  );
  const { data: buildings, isPending: isLoadingBuildings } = useQuery(
    getAllBuildingsOptions()
  );

  return (
    <main>
      <OverlayLoader
        isLoading={isLoadingRooms || isLoadingRoomTypes || isLoadingBuildings}
        text="Loading"
        size="sm"
      />
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
      </div>
    </main>
  );
}
