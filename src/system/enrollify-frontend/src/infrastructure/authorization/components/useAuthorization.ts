import { use } from "react";
import { AuthorizationContext } from "../AuthorizationContext";

export const useAuthorization = () => {
  const context = use(AuthorizationContext);

  if (!context) {
    throw new Error(
      "useAuthorization must be used within AuthorizationProvider"
    );
  }

  return context;
};
