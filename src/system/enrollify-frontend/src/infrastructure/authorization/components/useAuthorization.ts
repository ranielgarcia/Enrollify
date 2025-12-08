import { use } from "react";
import { AuthorizationContext } from "../AuthorizationContext";
import type { AuthorizationScope } from "../AuthorizationEvaluationContext";

export const useAuthorization = () => {
  const context = use(AuthorizationContext);

  if (!context) {
    throw new Error(
      "useAuthorization must be used within AuthorizationProvider"
    );
  }

  const checkPolicy = async (
    policyName: string,
    resource?: string,
    scope?: AuthorizationScope
  ): Promise<boolean> => {
    const result = await context.authorize(policyName, resource, scope);
    return result.succeeded;
  };

  return {
    authorize: context.authorize,
    checkPolicy,
    hasRole: context.hasRole,
    hasPermission: context.hasPermission,
  };
};
