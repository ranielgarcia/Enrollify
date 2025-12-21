import React, { useEffect, useState, useRef } from "react";

import { useMsal } from "@azure/msal-react";
import { useQueryClient } from "@tanstack/react-query";

import { OverlayLoader } from "@/components/app-loading-overlay";
import useAppQuery from "@/hooks/use-app-query-v2";

import {
  AuthenticationContext,
  type IAuthenticationContext,
  defaultAuthenticationContext,
} from "./authenticationContext";
import { EventType, type AuthenticationResult } from "@azure/msal-browser";
// import useAppMutation from "@/hooks/use-app-mutation-v2";

export const AuthenticationProvider = ({
  children,
}: Readonly<{
  children: React.ReactNode;
}>): React.ReactElement => {
  const queryClient = useQueryClient();
  const queryClientRef = useRef(queryClient);
  const [contextValue, setContextValue] = useState<IAuthenticationContext>(
    defaultAuthenticationContext
  );

  const { instance, accounts } = useMsal();

  const {
    data: userContext,
    isSuccess: isGetUserContextSuccessful,
    isLoading: isLoadingUserContext,
  } = useAppQuery({
    path: "/api/me",
    queryOptions: {
      meta: { persist: true },
      queryKey: ["/api/me"],
      staleTime: 1000 * 60 * 5, // 5 minutes - use cached data without refetching
      gcTime: 1000 * 60 * 60 * 24, // 24 hours - keep in cache for persistence
    },
  });

  useEffect(() => {
    const callbackId = instance.addEventCallback(async (event) => {
      if (event.eventType === EventType.LOGIN_SUCCESS && event.payload) {
        const payload = event.payload as AuthenticationResult;
        const account = payload.account;

        if (account) {
          setContextValue({
            user: {
              fullName: account.name ?? "",
              email: account.username,
            },
            isLoading: false,
            refreshUserContext: () => {
              queryClientRef.current.invalidateQueries({
                queryKey: ["/api/me"],
              });
            },
          } as IAuthenticationContext);
        }
      }
    });

    return () => {
      if (callbackId) {
        instance.removeEventCallback(callbackId);
      }
    };
  }, [instance]);

  useEffect(() => {
    if (
      instance &&
      accounts.length > 0 &&
      isGetUserContextSuccessful &&
      userContext
    ) {
      // const currentAccount = accounts[0];

      setContextValue({
        user: userContext,
        isLoading: isLoadingUserContext,
        refreshUserContext: () => {
          queryClientRef.current.invalidateQueries({
            queryKey: ["/api/me"],
          });
        },
      } as IAuthenticationContext);
    } else if (instance && accounts.length > 0) {
      const currentAccount = accounts[0];
      setContextValue({
        user: {
          fullName: currentAccount?.name,
          email: currentAccount?.username,
        },
        isLoading: isLoadingUserContext,
        refreshUserContext: () => {
          queryClientRef.current.invalidateQueries({
            queryKey: ["/api/me"],
          });
        },
      } as IAuthenticationContext);
    }
  }, [
    instance,
    accounts,
    isGetUserContextSuccessful,
    userContext,
    isLoadingUserContext,
    queryClientRef,
  ]);

  if (isLoadingUserContext) {
    return (
      <OverlayLoader
        isLoading={isLoadingUserContext}
        text="Loading, please wait..."
        size="lg"
      />
    );
  }

  return (
    <AuthenticationContext.Provider value={contextValue}>
      {children}
    </AuthenticationContext.Provider>
  );
};
