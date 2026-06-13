import SectionsManagementPageV2 from "@/page-components/sections-management-page-v2";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute(
  "/portal/curriculum-and-scheduling/scheduling/sections",
)({
  component: SectionsManagementPageV2,
  loader: (): RouteLoaderData => ({
    crumb: "Class Sections (v2)",
  }),
  beforeLoad: async ({ context: { authorization } }): Promise<void> => {
    if (!authorization?.isReady) {
      return;
    }

    const canAccess = await authorization.checkPolicy("canViewClassSections");
    if (!canAccess) {
      throw redirect({
        to: "/portal/home",
      });
    }
  },
});
