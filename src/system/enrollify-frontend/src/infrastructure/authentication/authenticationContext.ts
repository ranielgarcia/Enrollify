import React, { use } from "react";
import type { UserContext } from "../authorization/models/UserContext";

export interface IAuthenticationContext {
  user: UserContext;
  refreshUserContext: () => void;
}

export const defaultAuthenticationContext: IAuthenticationContext = {
  user: null,
  refreshUserContext: () => console.error("refreshUserContext not implemented"),
};

export const AuthenticationContext =
  React.createContext<IAuthenticationContext>(defaultAuthenticationContext);

export const AuthenticationConsumer = AuthenticationContext.Consumer;

export const useAuthenticationContext = () => use(AuthenticationContext);
