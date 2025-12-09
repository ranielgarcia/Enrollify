import HomePage from "@/page-components/home-page";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/portal/home")({
  component: HomePage,
  loader: () => ({
    crumb: undefined,
  }),
});
