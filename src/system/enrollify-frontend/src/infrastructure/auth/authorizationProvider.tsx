import React, { useEffect, useState, useRef } from "react";

import { useMsal } from "@azure/msal-react";
import { useQueryClient } from "@tanstack/react-query";

import { OverlayLoader } from "@/components/app-loading-overlay";
import useAppQuery from "@/hooks/use-app-query-v2";

import {
  AuthorizationContext,
  type IAuthorizationContext,
  defaultAuthorizationContext,
} from "./authorizationContext";
import { EventType, type AuthenticationResult } from "@azure/msal-browser";
// import useAppMutation from "@/hooks/use-app-mutation-v2";

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
  } = useAppQuery({
    path: "/api/me",
    queryOptions: {
      queryKey: ["/api/me"],
    },
  });

  // const { mutateAsync, isSuccess, isError, isPending, data } = useAppMutation({
  //   path: "/api/me",
  //   httpVerb: "post",
  //   mutationKey: "",
  // });

  // const currentUser = apiClient
  //   .GET("/api/me")
  //   .then((response) => response.data);

  // console.log(currentUser);

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
            queryKey: ["/api/me"],
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
