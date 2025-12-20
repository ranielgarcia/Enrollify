import { StrictMode, use } from "react";
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
import { QueryClient } from "@tanstack/react-query";
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

const queryClient = new QueryClient({
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

// Create a new router instance
const router = createRouter({
  routeTree,
  context: {
    msal,
    authorization,
  },
  defaultPreload: "intent",
  scrollRestoration: true,
  defaultStructuralSharing: true,
  defaultPreloadStaleTime: 0,
});

// Register the router instance for type safety
declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router;
    routeLoaderData: RouteLoaderData;
  }
}

// eslint-disable-next-line react-refresh/only-export-components
function App() {
  const msal = useMsal();
  const authorizationContext = use(AuthorizationContext);

  const activeAccount = msalInstance.getActiveAccount();

  if (!activeAccount) {
    const accounts = msalInstance.getAllAccounts();
    if (accounts.length > 0) {
      msalInstance.setActiveAccount(accounts[0]);
    }
  }

  msalInstance.addEventCallback(async (event) => {
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

  //enable account storage event
  msalInstance.enableAccountStorageEvents();

  return (
    <ThemeProvider defaultTheme="light" storageKey="vite-ui-theme">
      <RouterProvider
        router={router}
        context={{ msal, authorization: authorizationContext }}
      />
      <ReactQueryDevtools initialIsOpen={false} />
    </ThemeProvider>
  );
}

// Render the app
const rootElement = document.getElementById("root")!;
if (!rootElement.innerHTML) {
  const root = ReactDOM.createRoot(rootElement);
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
          <AuthenticationProvider>
            <AuthorizationProvider>
              <App />
            </AuthorizationProvider>
          </AuthenticationProvider>
        </PersistQueryClientProvider>
      </MsalProvider>
    </StrictMode>
  );
}
