import { useEffect, useState } from "react";
import { useAuthorization } from "./useAuthorization";
import type { AuthorizationScope } from "../models/AuthorizationScope";
import type { PolicyName } from "../models/PolicyNames";
import type { Role } from "../models/Roles";
import type { Permission } from "../models/PermissionsEnum";

export interface AuthorizedProps {
  policy?: PolicyName;
  roles?: Role[];
  permissions?: Permission[];
  scope?: AuthorizationScope;
  requireAll?: boolean;
  fallback?: React.ReactNode;
  unauthorized?: React.ReactNode;
  children: React.ReactNode;
}

export const Authorized: React.FC<AuthorizedProps> = ({
  policy,
  roles,
  permissions,
  scope,
  requireAll = true,
  fallback = null,
  unauthorized = null,
  children,
}) => {
  const [isAuthorized, setIsAuthorized] = useState<boolean | null>(null);
  const { authorize, hasRole, hasPermission } = useAuthorization();

  useEffect(() => {
    const checkAuthorization = async () => {
      // Check policy
      if (policy) {
        const result = await authorize(policy, undefined, scope);
        setIsAuthorized(result.succeeded);
        return;
      }

      // Check roles
      if (roles && roles.length > 0) {
        const roleCheck = requireAll
          ? roles.every((role) => hasRole(role))
          : roles.some((role) => hasRole(role));

        if (!roleCheck) {
          setIsAuthorized(false);
          return;
        }
      }

      // Check permissions
      if (permissions && permissions.length > 0) {
        const permCheck = requireAll
          ? permissions.every((perm) => hasPermission(perm, scope))
          : permissions.some((perm) => hasPermission(perm, scope));

        setIsAuthorized(permCheck);
        return;
      }

      // No checks specified
      setIsAuthorized(true);
    };

    checkAuthorization();
  }, [
    policy,
    roles,
    permissions,
    scope,
    requireAll,
    authorize,
    hasRole,
    hasPermission,
  ]);

  if (isAuthorized === null) {
    return <>{fallback}</>;
  }

  if (!isAuthorized) {
    return <>{unauthorized}</>;
  }

  return <>{children}</>;
};
