import AcademicYearManagementPage from "@/page-components/academic-year-management-page";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute, redirect } from "@tanstack/react-router";

export const Route = createFileRoute(
  "/portal/curriculum-and-scheduling/academic-year",
)({
  component: AcademicYearManagementPage,
  loader: (): RouteLoaderData => ({
    crumb: "Academic Year",
  }),
  beforeLoad: async ({ context: { authorization } }): Promise<void> => {
    if (!authorization?.isReady) {
      return;
    }

    const canAccess = await authorization.checkPolicy(
      "canViewAcademicYearsAndTerms",
    );
    if (!canAccess) {
      throw redirect({
        to: "/portal/home",
      });
    }
  },
});
