import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/portal/master-data-management")({
  loader: (): RouteLoaderData => ({
    crumb: "Master Data Management",
  }),
});
