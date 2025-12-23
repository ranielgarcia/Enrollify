import React, { useEffect, useState, useRef } from "react";

import { useMsal } from "@azure/msal-react";
import { useQueryClient } from "@tanstack/react-query";

import {
  AuthenticationContext,
  type IAuthenticationContext,
  defaultAuthenticationContext,
} from "./authenticationContext";
import { EventType, type AuthenticationResult } from "@azure/msal-browser";
import { useGetMeDetailsSuspense } from "@/api/collections/me-collection";

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
    refetch: refetchMeDetails,
  } = useGetMeDetailsSuspense();

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
            refreshUserContext: refetchMeDetails,
          } as IAuthenticationContext);
        }
      }
    });

    return () => {
      if (callbackId) {
        instance.removeEventCallback(callbackId);
      }
    };
  }, [instance, refetchMeDetails]);

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

  return (
    <AuthenticationContext.Provider value={contextValue}>
      {children}
    </AuthenticationContext.Provider>
  );
};
