import TeachersManagementPage from "@/page-components/teachers-management-page";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute(
  "/portal/curriculum-and-scheduling/teachers",
)({
  component: TeachersManagementPage,
  loader: (): RouteLoaderData => ({
    crumb: "Teacher Management",
  }),
  beforeLoad: async ({ context: { authorization } }): Promise<void> => {
    // Wait for authorization to be ready before making authorization decisions
    // If not ready, don't redirect - allow the page to load and handle auth later
    if (!authorization?.isReady) {
      return;
    }

    const canAccess = await authorization.checkPolicy("canViewTeachers");
    if (!canAccess) {
      throw redirect({
        to: "/portal/home",
      });
    }
  },
});
