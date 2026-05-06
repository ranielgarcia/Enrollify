import SectionsManagementPage from "@/page-components/sections-management-page";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute(
  "/portal/curriculum-and-scheduling/sections" as any,
)({
  component: SectionsManagementPage,
  loader: (): RouteLoaderData => ({
    crumb: "Class Sections",
  }),
  beforeLoad: async ({ context }: { context: any }): Promise<void> => {
    const { authorization } = context;
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
