import { createFileRoute } from "@tanstack/react-router";
import { AdminPortalLayout } from "@/portal/portal-layout";

export const Route = createFileRoute("/portal/dashboard")({
  component: About,
  loader: () => ({
    crumb: "Dashboard",
  }),
});

function About() {
  return <AdminPortalLayout />;
}
