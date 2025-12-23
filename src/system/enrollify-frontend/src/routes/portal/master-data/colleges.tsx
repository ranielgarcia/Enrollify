import { getAllCollegesOptions } from "@/api/collections/college-collection";
import CollegesPage from "@/page-components/colleges-management-page";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/portal/master-data/colleges")({
  beforeLoad: async ({
    context: { queryClient, authorization },
  }): Promise<void> => {
    // Wait for authorization to be ready before making authorization decisions
    // If not ready, don't redirect - allow the page to load and handle auth later
    if (!authorization?.isReady) {
      return;
    }

    const canViewRooms = await authorization.checkPolicy("canViewColleges");
    if (!canViewRooms) {
      throw redirect({
        to: "/portal/home",
      });
    }

    await queryClient.prefetchQuery(getAllCollegesOptions());
  },
  loader: (): RouteLoaderData => ({
    crumb: "Colleges",
  }),
  component: CollegesPage,
});
