import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute } from "@tanstack/react-router";

// @ts-expect-error - route will be registered after vite dev regenerates routeTree.gen.ts
export const Route = createFileRoute("/portal/teachers")({
  loader: (): RouteLoaderData => ({
    crumb: "Teachers Management",
  }),
});
