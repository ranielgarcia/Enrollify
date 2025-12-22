import { getAllRoomTypesOptions } from "@/api/collections/room-types-collections";
import RoomManagement from "@/page-components/rooms-management-page";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/portal/master-data/rooms")({
  loader: async ({ context: { queryClient } }): Promise<RouteLoaderData> => {
    await queryClient.prefetchQuery(getAllRoomTypesOptions());

    return {
      crumb: "Rooms",
    };
  },
  component: RoomManagement,
});
