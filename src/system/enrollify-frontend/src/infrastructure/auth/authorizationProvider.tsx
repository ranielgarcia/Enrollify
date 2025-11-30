import React, { useEffect, useRef, useState } from "react";

import { type AuthenticationResult, EventType } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import { useQueryClient } from "@tanstack/react-query";

import { OverlayLoader } from "@/components/app-loading-overlay";
import useAppQuery from "@/hooks/use-app-query";

import { type UserContext } from "./user-context";
import {
  AuthorizationContext,
  type IAuthorizationContext,
  defaultAuthorizationContext,
} from "./authorizationContext";

const getUserContextStateEndpoint = "/user-context-state/get";

export const AuthorizationProvider = ({
  children,
}: Readonly<{
  children: React.ReactNode;
}>): React.ReactElement => {
  const queryClient = useQueryClient();
  const queryClientRef = useRef(queryClient);
  const [contextValue, setContextValue] = useState<IAuthorizationContext>(
    defaultAuthorizationContext
  );
  const { instance, accounts } = useMsal();

  const {
    data: userContext,
    isSuccess: isGetUserContextSuccessful,
    isLoading: isLoadingUserContext,
  } = useAppQuery<UserContext>({
    path: getUserContextStateEndpoint,
    queryOptions: {
      queryKey: [getUserContextStateEndpoint],
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
              FullName: account.name ?? "",
              Email: account.username,
            } as UserContext,
          } as IAuthorizationContext);
        }
      }
    });

    return () => {
      if (callbackId) {
        instance.removeEventCallback(callbackId);
      }
    };
  });

  useEffect(() => {
    if (
      instance &&
      accounts.length > 0 &&
      isGetUserContextSuccessful &&
      userContext
    ) {
      const currentAccount = accounts[0];
      const onboardingCompletionNeeded =
        userContext.hasDefaultTenantInfo && userContext.isTenantOwner;
      setContextValue({
        userName: currentAccount?.name,
        userEmail: currentAccount?.username,
        userContext: userContext,
        refreshUserContext: () => {
          queryClientRef.current.invalidateQueries({
            queryKey: [getUserContextStateEndpoint],
          });
        },
        onboardingCompletionNeeded: onboardingCompletionNeeded,
      } as IAuthorizationContext);
    } else if (instance && accounts.length > 0) {
      const currentAccount = accounts[0];
      setContextValue({
        userName: currentAccount?.name,
        userEmail: currentAccount?.username,
      } as IAuthorizationContext);
    }
  }, [
    instance,
    accounts,
    isGetUserContextSuccessful,
    userContext,
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
    <AuthorizationContext.Provider value={contextValue}>
      {children}
    </AuthorizationContext.Provider>
  );
};
