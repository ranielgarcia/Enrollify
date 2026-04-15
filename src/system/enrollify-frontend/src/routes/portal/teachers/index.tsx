import TeachersManagementPage from "@/page-components/teachers-management-page";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute } from "@tanstack/react-router";

// @ts-expect-error - route will be registered after vite dev regenerates routeTree.gen.ts
export const Route = createFileRoute("/portal/teachers/")({
  component: TeachersManagementPage,
  loader: (): RouteLoaderData => ({
    crumb: "Teachers",
  }),
});
