import { createFileRoute } from "@tanstack/react-router";
import { AdminPortalLayout } from "@/portal/portal-layout";
import { useAuthenticationContext } from "@/infrastructure/authentication/authenticationContext";

export const Route = createFileRoute("/portal/dashboard")({
  component: About,
  loader: () => ({
    crumb: "Dashboard",
  }),
});

function About() {
  const userContext = useAuthenticationContext();

  console.log(
    userContext.user?.roles?.at(0)?.permissionScopes?.at(0)?.permissions
  );
  return <AdminPortalLayout />;
}
