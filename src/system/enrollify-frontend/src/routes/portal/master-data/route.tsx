// import { createScope } from "@/infrastructure/authorization/models/AuthorizationScope";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/portal/master-data")({
  beforeLoad: async ({ context: { authorization } }) => {
    // Wait for authorization to be ready before making authorization decisions
    // If not ready, don't redirect - allow the page to load and handle auth later
    if (!authorization?.isReady) {
      return;
    }

    const canViewRooms = await authorization.checkPolicy("canViewRooms");
    if (!canViewRooms) {
      throw redirect({
        to: "/portal/home",
      });
    }
  },
  loader: (): RouteLoaderData => ({
    crumb: "Master Data Management",
  }),
});
