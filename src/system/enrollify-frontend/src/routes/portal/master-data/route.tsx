// import { createScope } from "@/infrastructure/authorization/models/AuthorizationScope";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/portal/master-data")({
  loader: (): RouteLoaderData => ({
    crumb: "Master Data Management",
  }),
});
