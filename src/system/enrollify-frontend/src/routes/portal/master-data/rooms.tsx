import { getAllRoomTypesOptions } from "@/api/collections/room-type-collection";
import RoomManagement from "@/page-components/rooms-management-page";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/portal/master-data/rooms")({
  beforeLoad: async ({ context: { queryClient } }): Promise<void> => {
    await queryClient.prefetchQuery(getAllRoomTypesOptions());
  },
  loader: (): RouteLoaderData => ({
    crumb: "Rooms",
  }),
  component: RoomManagement,
});
