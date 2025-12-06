import React, { use } from "react";
import type { components } from "@/api/generated/api";

export interface IAuthorizationContext {
  user: components["schemas"]["EnrollifyCoreAuthenticationUserContext"] | null;
  refreshUserContext: () => void;
}

export const defaultAuthorizationContext: IAuthorizationContext = {
  user: null,
  refreshUserContext: () => console.error("refreshUserContext not implemented"),
};

export const AuthorizationContext = React.createContext<IAuthorizationContext>(
  defaultAuthorizationContext
);

export const AuthorizationConsumer = AuthorizationContext.Consumer;

export const useAuthorizationContext = () => use(AuthorizationContext);
