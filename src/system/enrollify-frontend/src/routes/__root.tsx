import type { IAuthorizationContextValue } from "@/infrastructure/authorization/AuthorizationContext";
import type { IMsalContext } from "@azure/msal-react";
import type { QueryClient } from "@tanstack/react-query";
import { createRootRouteWithContext, Outlet } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/react-router-devtools";
import { NuqsAdapter } from "nuqs/adapters/react";

export const Route = createRootRouteWithContext<{
  msal: IMsalContext;
  authorization: IAuthorizationContextValue | null;
  queryClient: QueryClient;
}>()({
  component: () => {
    return (
      <NuqsAdapter>
        <Outlet />
        <TanStackRouterDevtools position="bottom-right" />
      </NuqsAdapter>
    );
  },
  loader: () => ({
    crumb: undefined,
  }),
  notFoundComponent: () => <div>404 Not Found</div>,
});
