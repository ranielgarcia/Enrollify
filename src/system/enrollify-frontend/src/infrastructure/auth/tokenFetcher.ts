import { PublicClientApplication } from "@azure/msal-browser";

import { Config } from "../Configurations/app-config";

export async function getCurrentToken(
  msalInstance: PublicClientApplication
): Promise<string | null> {
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
      scopes: [Config.Auth.Scope],
      account: activeAccount || accounts[0],
    };

    try {
      const authResult = await msalInstance.acquireTokenSilent(request);
      return authResult.accessToken;
    } catch (error) {
      console.error("Error acquiring token:", error);
      // If silent acquisition fails, try acquiring token through popup or redirect
      try {
        const authResult = await msalInstance.acquireTokenPopup(request);
        return authResult.accessToken;
      } catch (error) {
        console.error("Error acquiring token:", error);
        return null;
      }
    }
  };

  let token = null;

  if (typeof window !== "undefined") {
    token = await acquireAccessToken();
  }

  return token;
}
