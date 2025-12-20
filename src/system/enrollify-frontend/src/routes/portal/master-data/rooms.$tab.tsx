import RoomManagement from "@/page-components/rooms-management-page";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/portal/master-data/rooms/$tab")({
  loader: (): RouteLoaderData => ({
    crumb: "Rooms",
  }),
  component: RoomManagement,
});
