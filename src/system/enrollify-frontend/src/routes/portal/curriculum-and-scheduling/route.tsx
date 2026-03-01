import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/portal/curriculum-and-scheduling")({
  loader: (): RouteLoaderData => ({
    crumb: "Curriculum and Scheduling",
  }),
});
