import { useEffect, useState, type PropsWithChildren } from "react";
import { useAuthenticationContext } from "../authentication/authenticationContext";
import { AuthorizationService } from "./AuthorizationService";
import { PolicyRegistry } from "./policies/PolicyRegistry";
import { registerDefaultPolicies } from "./policies/defaultPolicies";
import type { IAuthorizationHandler } from "./handlers/IAuthorizationHandler";
import { RoleHandler } from "./handlers/RoleHandler";
import { PermissionHandler } from "./handlers/PermissionHandler";
import { ScopeHandler } from "./handlers/ScopeHandler";
import type { AuthorizationResult } from "./AuthorizationResult";
import type { AuthorizationEvaluationContext } from "./models/AuthorizationEvaluationContext";
import { AuthorizationContext } from "./AuthorizationContext";
import type { AuthorizationScope } from "./models/AuthorizationScope";
import type { PolicyName } from "./models/PolicyNames";
import type { Permission } from "./models/Permissions";
import type { Role } from "./models/Roles";

export const AuthorizationProvider: React.FC<PropsWithChildren> = ({
  children,
}) => {
  const { user } = useAuthenticationContext();
  const [authService, setAuthService] = useState<AuthorizationService | null>(
    null
  );

  useEffect(() => {
    // Initialize authorization service with handlers and policies
    const policyRegistry = new PolicyRegistry();
    registerDefaultPolicies(policyRegistry);

    const handlers: IAuthorizationHandler[] = [
      new RoleHandler(),
      new PermissionHandler(),
      new ScopeHandler(),
    ];

    // eslint-disable-next-line react-hooks/set-state-in-effect
    setAuthService(new AuthorizationService(policyRegistry, handlers));
  }, []);

  const authorize = async (
    policyName: PolicyName,
    resource?: string,
    authorizationScope?: AuthorizationScope
  ): Promise<AuthorizationResult> => {
    if (!authService || !user) {
      return { succeeded: false, failureReasons: ["Not authenticated"] };
    }

    const context: AuthorizationEvaluationContext = {
      user,
      resource,
      authorizationScope,
    };
    return authService.authorize(policyName, context);
  };

  const hasRole = (...roles: Role[]): boolean => {
    if (!authService || !user) return false;
    return authService.hasRole(user, ...roles);
  };

  const hasPermission = (
    permission: Permission,
    scope?: AuthorizationScope
  ): boolean => {
    if (!authService || !user) return false;
    return authService.hasPermission(user, permission, scope);
  };

  return (
    <AuthorizationContext.Provider
      value={{ authorize, hasRole, hasPermission, authService }}
    >
      {children}
    </AuthorizationContext.Provider>
  );
};
