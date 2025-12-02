import { createFileRoute } from "@tanstack/react-router";
import { AdminPortalLayout } from "@/portal/portal-layout";

export const Route = createFileRoute("/portal")({
  component: About,
});

function About() {
  return <AdminPortalLayout />;
}
