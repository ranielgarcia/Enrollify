import NotificationsPage from "@/page-components/notifications-page";
import { searchParams } from "@/page-components/notifications-page/searchParams";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute } from "@tanstack/react-router";
import { createStandardSchemaV1 } from "nuqs";

export const Route = createFileRoute("/portal/notifications")({
  component: NotificationsPage,
  validateSearch: createStandardSchemaV1(searchParams, {
    // https://nuqs.dev/docs/adapters#tanstack-router
    partialOutput: true,
  }),
  loader: (): RouteLoaderData => ({
    crumb: "Notifications",
  }),
});
