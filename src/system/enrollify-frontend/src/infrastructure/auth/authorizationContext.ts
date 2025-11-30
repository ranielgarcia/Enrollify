import React, { use } from "react";
import { type UserContext } from "./user-context";

export interface IAuthorizationContext {
  user: UserContext | null;
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
