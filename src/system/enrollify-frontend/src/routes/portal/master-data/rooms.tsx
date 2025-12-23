import { getAllRoomTypesOptions } from "@/api/collections/room-type-collection";
import RoomManagement from "@/page-components/rooms-management-page";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/portal/master-data/rooms")({
  beforeLoad: async ({
    context: { queryClient, authorization },
  }): Promise<void> => {
    // Wait for authorization to be ready before making authorization decisions
    // If not ready, don't redirect - allow the page to load and handle auth later
    if (!authorization?.isReady) {
      return;
    }

    const canAccess = await authorization.checkPolicy("canViewRooms");
    if (!canAccess) {
      throw redirect({
        to: "/portal/home",
      });
    }

    await queryClient.prefetchQuery(getAllRoomTypesOptions());
  },
  loader: (): RouteLoaderData => ({
    crumb: "Rooms",
  }),
  component: RoomManagement,
});
