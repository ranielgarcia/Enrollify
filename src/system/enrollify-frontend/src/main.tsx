import { StrictMode } from "react";
import ReactDOM from "react-dom/client";
import { RouterProvider, createRouter } from "@tanstack/react-router";
import "./index.css";

// Import the generated route tree
import { routeTree } from "./routeTree.gen";
import { MsalProvider, useMsal, type IMsalContext } from "@azure/msal-react";
import { msalInstance } from "./infrastructure/auth/authConfig";
import {
  EventType,
  InteractionType,
  type AuthenticationResult,
} from "@azure/msal-browser";
import { handleLogin } from "./infrastructure/auth/msal";
import { AuthorizationProvider } from "./infrastructure/auth/authorizationProvider";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { ThemeProvider } from "./components/theming/theme-provider";
import { QueryClientProvider, QueryClient } from "@tanstack/react-query";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
    },
  },
});

const msal = {} as IMsalContext;

// Create a new router instance
const router = createRouter({
  routeTree,
  context: {
    msal,
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
  }
}

// eslint-disable-next-line react-refresh/only-export-components
function App() {
  const msal = useMsal();

  const activeAccount = msalInstance.getActiveAccount();
  console.log(activeAccount);

  if (!activeAccount) {
    const accounts = msalInstance.getAllAccounts();
    if (accounts.length > 0) {
      msalInstance.setActiveAccount(accounts[0]);
    }
  }

  msalInstance.addEventCallback(async (event) => {
    console.log(event.eventType);
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
    <QueryClientProvider client={queryClient}>
      <AuthorizationProvider>
        <ThemeProvider defaultTheme="light" storageKey="vite-ui-theme">
          <RouterProvider router={router} context={{ msal }} />
          <ReactQueryDevtools initialIsOpen={false} />
        </ThemeProvider>
      </AuthorizationProvider>
    </QueryClientProvider>
  );
}

// Render the app
const rootElement = document.getElementById("root")!;
if (!rootElement.innerHTML) {
  const root = ReactDOM.createRoot(rootElement);
  root.render(
    <StrictMode>
      <MsalProvider instance={msalInstance}>
        <App />
      </MsalProvider>
    </StrictMode>
  );
}
