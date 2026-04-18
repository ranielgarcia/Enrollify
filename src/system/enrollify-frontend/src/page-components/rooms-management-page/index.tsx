import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { getAllRoomTypesOptions } from "@/api/collections/room-type-collection";
import { useSuspenseQuery } from "@tanstack/react-query";
import { getAllRooms } from "@/api/collections/room-collection";
import { getAllBuildingsOptions } from "@/api/collections/building-collection";
import { ManagementPageLayout } from "@/components/management-page-layout";
import { useState } from "react";
import { Bookmark, DoorOpen } from "lucide-react";
import { useCrudState } from "@/hooks/use-crud-state";
import type { RoomType } from "@/api/models/room-type";
import { RoomTypeFormDrawer } from "./room-types-components/room-type-form-drawer";
import { RoomFormDrawer } from "./room-components/room-form-drawer";
import type { Room } from "@/api/models/room";
import { DeleteRoomAlertDialog } from "./room-components/delete-room-alert-dialog";
import { RoomsTable } from "./room-components/rooms-table";
import { RoomTypesTable } from "./room-types-components/room-types-table";
import { DeleteRoomTypeAlertDialog } from "./room-types-components/delete-room-type-alert-dialog";

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

  const {
    isFormOpen: isRoomTypeFormOpen,
    entityToEdit: roomTypeToEdit,
    entityToDelete: roomTypeToDelete,
    handleEdit: handleRoomTypeEdit,
    handleDelete: handleRoomTypeDelete,
    handleFormOpenChange: handleRoomTypeFormOpenChange,
    handleDeleteDialogOpenChange: handleRoomTypeDeleteDialogOpenChange,
  } = useCrudState<RoomType>();

  const {
    isFormOpen: isRoomFormOpen,
    entityToEdit: roomToEdit,
    entityToDelete: roomToDelete,
    handleEdit: handleRoomEdit,
    handleDelete: handleRoomDelete,
    handleFormOpenChange: handleRoomFormOpenChange,
    handleDeleteDialogOpenChange: handleRoomDeleteDialogOpenChange,
  } = useCrudState<Room>();

  return (
    <ManagementPageLayout
      title="Room Management"
      description="Manage classroom and facility resources"
      createNewItemButton={
        activeTab === "room-types" ? (
          <RoomTypeFormDrawer
            onOpenChange={handleRoomTypeFormOpenChange}
            roomTypeToUpdate={roomTypeToEdit}
            isOpen={isRoomTypeFormOpen}
            setIsOpen={handleRoomTypeFormOpenChange}
          />
        ) : (
          <RoomFormDrawer
            roomTypes={roomTypes}
            buildings={buildings}
            onOpenChange={handleRoomFormOpenChange}
            roomToUpdate={roomToEdit}
            isOpen={isRoomFormOpen}
            setIsOpen={handleRoomFormOpenChange}
          />
        )
      }
      icon={<DoorOpen />}
    >
      <Tabs
        value={activeTab}
        onValueChange={handleTabChange}
        className="min-h-0 flex-1 space-y-4"
      >
        <TabsList variant="line">
          <TabsTrigger value="rooms">
            <DoorOpen className="size-4" />
            Rooms
          </TabsTrigger>
          <TabsTrigger value="room-types">
            <Bookmark />
            Room Types
          </TabsTrigger>
        </TabsList>

        <TabsContent value="rooms" className="flex min-h-0 flex-col space-y-4">
          <RoomsTable
            rooms={rooms}
            onEdit={handleRoomEdit}
            onDelete={handleRoomDelete}
          />
          <DeleteRoomAlertDialog
            roomToDelete={roomToDelete}
            isOpen={!!roomToDelete}
            onOpenChange={handleRoomDeleteDialogOpenChange}
          />
        </TabsContent>

        <TabsContent value="room-types" className="flex min-h-0 flex-col space-y-4">
          <RoomTypesTable
            roomTypes={roomTypes}
            onEdit={handleRoomTypeEdit}
            onDelete={handleRoomTypeDelete}
          />
          <DeleteRoomTypeAlertDialog
            isOpen={!!roomTypeToDelete}
            onOpenChange={handleRoomTypeDeleteDialogOpenChange}
            roomTypeToDelete={roomTypeToDelete}
          />
        </TabsContent>
      </Tabs>
    </ManagementPageLayout>
  );
}
