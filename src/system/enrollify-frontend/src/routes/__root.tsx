import type { IAuthorizationContextValue } from "@/infrastructure/authorization/AuthorizationContext";
import type { IMsalContext } from "@azure/msal-react";
import { createRootRouteWithContext, Outlet } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/react-router-devtools";

export const Route = createRootRouteWithContext<{
  msal: IMsalContext;
  authorization: IAuthorizationContextValue | null;
}>()({
  component: () => {
    return (
      <>
        <Outlet />
        <TanStackRouterDevtools position="bottom-right" />
      </>
    );
  },
  loader: () => ({
    crumb: undefined,
  }),
  notFoundComponent: () => <div>404 Not Found</div>,
});
