import { createFileRoute } from "@tanstack/react-router";
import { AdminPortalLayout } from "@/portal/portal-layout";
import { useAuthorizationContext } from "@/infrastructure/auth/authorizationContext";

export const Route = createFileRoute("/portal/dashboard")({
  component: About,
  loader: () => ({
    crumb: "Dashboard",
  }),
});

function About() {
  const userContext = useAuthorizationContext();

  console.log(userContext.user?.email);
  return <AdminPortalLayout />;
}
