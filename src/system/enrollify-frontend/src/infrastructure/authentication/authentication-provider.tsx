import React, { useEffect, useMemo } from "react";

import { useMsal } from "@azure/msal-react";

import {
  AuthenticationContext,
  type IAuthenticationContext,
  defaultAuthenticationContext,
} from "./authentication-context";
import { EventType } from "@azure/msal-browser";
import { useGetMeDetailsSuspense } from "@/api/collections/me-collection";

export const AuthenticationProvider = ({
  children,
}: Readonly<{
  children: React.ReactNode;
}>): React.ReactElement => {
  const { instance, accounts } = useMsal();

  const {
    data: userContext,
    isSuccess: isGetUserContextSuccessful,
    isLoading: isLoadingUserContext,
    refetch: refreshUserContext,
  } = useGetMeDetailsSuspense();

  // Derive context value from state instead of using effects
  const contextValue = useMemo<IAuthenticationContext>(() => {
    if (
      instance &&
      accounts.length > 0 &&
      isGetUserContextSuccessful &&
      userContext
    ) {
      return {
        user: userContext,
        isLoading: isLoadingUserContext,
        refreshUserContext,
      };
    } else if (instance && accounts.length > 0) {
      const currentAccount = accounts[0];
      return {
        user: {
          fullName: currentAccount?.name,
          email: currentAccount?.username,
        },
        isLoading: isLoadingUserContext,
        refreshUserContext,
      };
    }

    return defaultAuthenticationContext;
  }, [
    instance,
    accounts,
    isGetUserContextSuccessful,
    userContext,
    isLoadingUserContext,
    refreshUserContext,
  ]);

  useEffect(() => {
    const callbackId = instance.addEventCallback(async (event) => {
      if (event.eventType === EventType.LOGIN_SUCCESS && event.payload) {
        // Trigger a refetch when login succeeds
        refreshUserContext();
      }
    });

    return () => {
      if (callbackId) {
        instance.removeEventCallback(callbackId);
      }
    };
  }, [instance, refreshUserContext]);

  return (
    <AuthenticationContext.Provider value={contextValue}>
      {children}
    </AuthenticationContext.Provider>
  );
};
