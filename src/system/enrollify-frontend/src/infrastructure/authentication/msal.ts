import { Config } from "../Configurations/app-config";
import { loginRequest, msalInstance } from "./authConfig";
import { getCurrentToken } from "./tokenFetcher";

export async function getToken() {
  const authToken = await getCurrentToken(msalInstance);
  // console.log("AUTH TOKEN:", authToken);
  return authToken;
}

export const handleLogin = () => {
  if (Config.SIGNIN_FLOW === "popup") {
    msalInstance.loginPopup(loginRequest).catch((e) => {
      console.error(`loginPopup failed: ${e}`);
    });
  } else if (Config.SIGNIN_FLOW === "redirect") {
    msalInstance.loginRedirect(loginRequest).catch((e) => {
      console.error(`loginRedirect failed: ${e}`);
    });
  }
};

export const handleLogout = () => {
  if (Config.SIGNIN_FLOW === "popup") {
    const logoutRequest = {
      account: msalInstance.getActiveAccount(),
      postLogoutRedirectUri: "/",
      mainWindowRedirectUri: "/",
    };
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    msalInstance.logoutPopup(logoutRequest).catch((e: any) => {
      console.error(`logoutPopup failed: ${e}`);
    });
  } else if (Config.SIGNIN_FLOW === "redirect") {
    const logoutRequest = {
      account: msalInstance.getActiveAccount(),
      postLogoutRedirectUri: "/",
    };
    msalInstance.logoutRedirect(logoutRequest).catch((e) => {
      console.error(`logoutRedirect failed: ${e}`);
    });
  }
};
