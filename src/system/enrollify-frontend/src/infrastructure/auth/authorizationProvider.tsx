import React, { useEffect, useState, useRef } from "react";

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
import { EventType, type AuthenticationResult } from "@azure/msal-browser";

const getMyDetailsEndpoint = "/me";

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
    path: getMyDetailsEndpoint,
    queryOptions: {
      queryKey: [getMyDetailsEndpoint],
    },
  });

  const roles = useAppQuery({
    path: "/roles",
  });

  console.log(roles.data);

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
      // const currentAccount = accounts[0];

      setContextValue({
        user: userContext,
        refreshUserContext: () => {
          queryClientRef.current.invalidateQueries({
            queryKey: [getMyDetailsEndpoint],
          });
        },
      } as IAuthorizationContext);
    } else if (instance && accounts.length > 0) {
      const currentAccount = accounts[0];
      setContextValue({
        user: {
          fullName: currentAccount?.name,
          email: currentAccount?.username,
        },
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
