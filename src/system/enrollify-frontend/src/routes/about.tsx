import { createFileRoute } from "@tanstack/react-router";
import Dashboard from "@/app/dashboard/page";

export const Route = createFileRoute("/about")({
  component: About,
});

function About() {
  return <Dashboard />;
}
