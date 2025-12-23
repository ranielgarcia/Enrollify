import { StrictMode, use, useMemo, useEffect, Suspense } from "react";
import ReactDOM from "react-dom/client";
import { RouterProvider, createRouter } from "@tanstack/react-router";
import "./index.css";

// Import the generated route tree
import { routeTree } from "./routeTree.gen";
import { MsalProvider, useMsal, type IMsalContext } from "@azure/msal-react";
import { msalInstance } from "./infrastructure/authentication/authConfig";
import {
  EventType,
  InteractionType,
  type AuthenticationResult,
} from "@azure/msal-browser";
import { handleLogin } from "./infrastructure/authentication/msal";
import { AuthenticationProvider } from "./infrastructure/authentication/authenticationProvider";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { ThemeProvider } from "./components/theming/theme-provider";
import {
  MutationCache,
  QueryCache,
  QueryClient,
  useQueryClient,
  type QueryKey,
} from "@tanstack/react-query";
import {
  PersistQueryClientProvider,
  type PersistedClient,
  type Persister,
} from "@tanstack/react-query-persist-client";
import type { RouteLoaderData } from "./types/route.types";
import { AuthorizationProvider } from "./infrastructure/authorization/AuthorizationProvider";
import {
  AuthorizationContext,
  type IAuthorizationContextValue,
} from "./infrastructure/authorization/AuthorizationContext";
import type { AxiosError } from "axios";
import {
  formatValidationErrors,
  parseApiError,
  type ProblemDetails,
} from "./lib/axios-utils";
import { toast } from "sonner";
import { OverlayLoader } from "./components/app-loading-overlay";

// Register the router instance for type safety
declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router;
    routeLoaderData: RouteLoaderData;
  }
}

declare module "@tanstack/react-query" {
  interface Register {
    mutationMeta: {
      invalidateQueries?: ReadonlyArray<QueryKey>;
    };
    queryMeta: {
      persist?: boolean;
    };
  }
}

function hasMutationMeta(
  meta: Record<string, unknown> | undefined
): meta is { invalidateQueries: ReadonlyArray<QueryKey> } {
  return meta != null && "invalidateQueries" in meta;
}

const queryClient = new QueryClient({
  mutationCache: new MutationCache({
    onError: (error) => {
      console.error("Global Mutation error handler:", error);
      const axiosError = error as AxiosError<ProblemDetails>;
      const parsed = parseApiError(axiosError);

      // Show error toast with title and detail
      toast.error(parsed.title, {
        description: parsed.validationErrors
          ? formatValidationErrors(parsed.validationErrors)
          : parsed.detail || "Please try again.",
      });
    },
    onSettled: (_data, _error, _variables, _context, mutation) => {
      // global automatic query invalidation
      // add meta object into useMutation to trigger this
      if (hasMutationMeta(mutation.meta)) {
        mutation.meta.invalidateQueries.forEach((queryKey) => {
          queryClient.invalidateQueries({ queryKey });
        });
      }
    },
  }),
  queryCache: new QueryCache({
    onError: (error) => {
      console.error(error);
    },
  }),
  defaultOptions: {
    queries: {
      // refetchOnWindowFocus: false,
      gcTime: 1000 * 60 * 60 * 24, // 24 hours - how long inactive data stays in cache
    },
  },
});

const persister: Persister = {
  persistClient: async (client: PersistedClient) => {
    localStorage.setItem("REACT_QUERY_CACHE", JSON.stringify(client));
  },
  restoreClient: async () => {
    const cache = localStorage.getItem("REACT_QUERY_CACHE");
    return cache ? JSON.parse(cache) : undefined;
  },
  removeClient: async () => {
    localStorage.removeItem("REACT_QUERY_CACHE");
  },
};

const msal = {} as IMsalContext;
const authorization = {} as IAuthorizationContextValue;
const queryClientType = {} as QueryClient;

// Create a new router instance
const router = createRouter({
  routeTree,
  context: {
    msal,
    authorization,
    queryClient: queryClientType,
  },
  defaultPreload: "intent",
  scrollRestoration: true,
  defaultStructuralSharing: true,
  defaultPreloadStaleTime: 0,
});

// eslint-disable-next-line react-refresh/only-export-components
function App() {
  const msal = useMsal();
  const authorizationContext = use(AuthorizationContext);
  const queryClient = useQueryClient();

  // Set active account on initial mount
  useEffect(() => {
    const activeAccount = msalInstance.getActiveAccount();
    if (!activeAccount) {
      const accounts = msalInstance.getAllAccounts();
      if (accounts.length > 0) {
        msalInstance.setActiveAccount(accounts[0]);
      }
    }
  }, []);

  // Handle MSAL events with proper cleanup
  useEffect(() => {
    const callbackId = msalInstance.addEventCallback(async (event) => {
      if (event.eventType === EventType.LOGIN_SUCCESS && event.payload) {
        const payload = event.payload as AuthenticationResult;
        const account = payload.account;
        msalInstance.setActiveAccount(account);
      }

      // To prevent: BrowserAuthError: monitor_window_timeout: Token acquisition in iframe failed due to timeout.
      // http://github.com/AzureAD/microsoft-authentication-library-for-js/issues/5724#issuecomment-2406003107
      if (
        event.eventType === EventType.ACQUIRE_TOKEN_FAILURE &&
        event.interactionType === InteractionType.Silent
      ) {
        handleLogin();
      }
    });

    // Enable account storage event
    msalInstance.enableAccountStorageEvents();

    // Cleanup: remove event callback when component unmounts
    return () => {
      if (callbackId) {
        msalInstance.removeEventCallback(callbackId);
      }
    };
  }, []);

  // Memoize the router context to prevent unnecessary re-renders
  const routerContext = useMemo(
    () => ({ msal, authorization: authorizationContext, queryClient }),
    [msal, authorizationContext, queryClient]
  );

  return (
    <ThemeProvider defaultTheme="light" storageKey="vite-ui-theme">
      <RouterProvider router={router} context={routerContext} />
      <ReactQueryDevtools initialIsOpen={false} />
    </ThemeProvider>
  );
}

// Render the app
const rootElement = document.getElementById("root")!;
if (!rootElement.innerHTML) {
  const root = ReactDOM.createRoot(rootElement);
  msalInstance.initialize().then(() => {
    root.render(
      <StrictMode>
        <MsalProvider instance={msalInstance}>
          <PersistQueryClientProvider
            client={queryClient}
            persistOptions={{
              persister,
              dehydrateOptions: {
                shouldDehydrateQuery: (query) => query.meta?.persist === true,
              },
            }}
          >
            <Suspense
              fallback={
                <OverlayLoader
                  isLoading={true}
                  text="Loading profile..."
                  size="lg"
                />
              }
            >
              <AuthenticationProvider>
                <AuthorizationProvider>
                  <App />
                </AuthorizationProvider>
              </AuthenticationProvider>
            </Suspense>
          </PersistQueryClientProvider>
        </MsalProvider>
      </StrictMode>
    );
  });
}
