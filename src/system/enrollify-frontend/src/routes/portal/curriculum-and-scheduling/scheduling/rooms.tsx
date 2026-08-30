import RoomSchedulePage from "@/page-components/room-schedule-page";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute(
  "/portal/curriculum-and-scheduling/scheduling/rooms",
)({
  component: RoomSchedulePage,
  loader: (): RouteLoaderData => ({
    crumb: "Room Schedule",
  }),
  beforeLoad: async ({ context: { authorization } }): Promise<void> => {
    if (!authorization?.isReady) {
      return;
    }

    const canAccess = await authorization.checkPolicy("canViewRooms");
    if (!canAccess) {
      throw redirect({
        to: "/portal/home",
      });
    }
  },
});
