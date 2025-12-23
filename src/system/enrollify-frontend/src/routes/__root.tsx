import { getMeQueryOptions } from "@/api/collections/me-collection";
import type { IAuthorizationContextValue } from "@/infrastructure/authorization/AuthorizationContext";
import type { IMsalContext } from "@azure/msal-react";
import type { QueryClient } from "@tanstack/react-query";
import { createRootRouteWithContext, Outlet } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/react-router-devtools";

export const Route = createRootRouteWithContext<{
  msal: IMsalContext;
  authorization: IAuthorizationContextValue | null;
  queryClient: QueryClient;
}>()({
  component: () => {
    return (
      <>
        <Outlet />
        <TanStackRouterDevtools position="bottom-right" />
      </>
    );
  },
  beforeLoad: async ({ context: { queryClient } }) => {
    await queryClient.prefetchQuery(getMeQueryOptions());
  },
  loader: () => ({
    crumb: undefined,
  }),
  notFoundComponent: () => <div>404 Not Found</div>,
});
