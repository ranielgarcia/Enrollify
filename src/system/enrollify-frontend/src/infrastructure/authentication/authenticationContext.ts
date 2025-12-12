import React, { use } from "react";
import type { UserContext } from "../../api/models/UserContext";

export interface IAuthenticationContext {
  user: UserContext;
  isLoading: boolean;
  refreshUserContext: () => void;
}

export const defaultAuthenticationContext: IAuthenticationContext = {
  user: null,
  isLoading: true,
  refreshUserContext: () => console.error("refreshUserContext not implemented"),
};

export const AuthenticationContext =
  React.createContext<IAuthenticationContext>(defaultAuthenticationContext);

export const AuthenticationConsumer = AuthenticationContext.Consumer;

export const useAuthenticationContext = () => use(AuthenticationContext);
