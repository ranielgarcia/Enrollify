import {
  InteractionRequiredAuthError,
  type IPublicClientApplication,
} from "@azure/msal-browser";

import { loginRequest } from "./auth-config";

interface getCurrentTokenProps {
  msalInstance: IPublicClientApplication;
  forceRefreshToken?: boolean;
}

export async function getCurrentAccessToken({
  msalInstance,
  forceRefreshToken,
}: getCurrentTokenProps): Promise<string> {
  const acquireAccessToken = async () => {
    const activeAccount = msalInstance.getActiveAccount();
    const accounts = msalInstance.getAllAccounts();

    if (!activeAccount && accounts.length === 0) {
      /*
       * User is not signed in. Throw error or wait for user to login.
       * Do not attempt to log a user in outside of the context of MsalProvider
       */
      return null;
    }
    const request = {
      ...loginRequest,
      account: activeAccount || accounts[0],
      forceRefresh: forceRefreshToken,
    };

    try {
      const tokenResponse = await msalInstance.acquireTokenSilent(request);
      return tokenResponse.accessToken;
    } catch (error) {
      console.error("Error acquiring token silently:", error);

      if (error instanceof InteractionRequiredAuthError) {
        try {
          const tokenResponse = await msalInstance.acquireTokenPopup(request);
          return tokenResponse.accessToken;
        } catch (popupError) {
          console.error("Error acquiring token via popup:", popupError);
          return null;
        }
      }

      // For non-interaction errors, return null immediately
      return null;
    }
  };

  let accessToken = null;

  if (typeof window !== "undefined") {
    accessToken = await acquireAccessToken();
  }

  if (!accessToken) {
    // Either throw to set error state...
    throw new Error("Failed to acquire access token.");
    // Or return an empty shape:
    // return {} as TData;
  }

  return accessToken;
}
