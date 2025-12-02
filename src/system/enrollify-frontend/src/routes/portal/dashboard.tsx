import { createFileRoute } from "@tanstack/react-router";
import { AdminPortalLayout } from "@/portal/portal-layout";

export const Route = createFileRoute("/portal/dashboard")({
  component: About,
});

function About() {
  return <AdminPortalLayout />;
}
