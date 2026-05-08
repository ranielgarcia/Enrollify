import SectionsManagementPage from "@/page-components/sections-management-page";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute(
  "/portal/curriculum-and-scheduling/sections",
)({
  component: SectionsManagementPage,
  loader: (): RouteLoaderData => ({
    crumb: "Class Sections",
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
