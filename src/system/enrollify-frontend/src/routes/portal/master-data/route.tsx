// import { createScope } from "@/infrastructure/authorization/models/AuthorizationScope";
import type { RouteLoaderData } from "@/types/route.types";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/portal/master-data")({
  // beforeLoad: async ({ context: { authorization }, location }) => {
  //   // var authorizationScope = new AuthorizationScope
  //   authorization?.authService
  //   authorization?.authorize("AdminOnly", "", createScope("Roles"));
  // },
  loader: (): RouteLoaderData => ({
    crumb: "Master Data Management",
  }),
});
