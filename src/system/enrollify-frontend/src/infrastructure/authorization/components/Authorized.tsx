import { useEffect, useState } from "react";
import { useAuthorization } from "./useAuthorization";
import type { AuthorizationResource } from "../models/AuthorizationResource";
import type { PolicyName } from "../models/PolicyNames";
import type { Role } from "../models/Roles";
import type { Permission } from "../models/PermissionsEnum";

export interface AuthorizedProps {
  policy?: PolicyName;
  roles?: Role[];
  permissions?: Permission[];
  /**
   * Resource for scoped/entity-level authorization.
   * Used with policy and permission checks.
   */
  resource?: AuthorizationResource;
  requireAll?: boolean;
  fallback?: React.ReactNode;
  unauthorized?: React.ReactNode;
  children: React.ReactNode;
}

export const Authorized: React.FC<AuthorizedProps> = ({
  policy,
  roles,
  permissions,
  resource,
  requireAll = true,
  fallback = null,
  unauthorized = null,
  children,
}) => {
  const [isAuthorized, setIsAuthorized] = useState<boolean | null>(null);
  const { authorize, currentUserHasRole, currentUserHasPermission } =
    useAuthorization();

  useEffect(() => {
    const checkAuthorization = async () => {
      try {
        // Check policy
        if (policy) {
          const result = await authorize(policy, resource);
          setIsAuthorized(result.succeeded);
          return;
        }

        // Check roles
        if (roles && roles.length > 0) {
          const roleCheck = requireAll
            ? roles.every((role) => currentUserHasRole(role))
            : roles.some((role) => currentUserHasRole(role));

          if (!roleCheck) {
            setIsAuthorized(false);
            return;
          }
        }

        // Check permissions
        if (permissions && permissions.length > 0) {
          const permCheck = requireAll
            ? permissions.every((perm) =>
                currentUserHasPermission(perm, resource)
              )
            : permissions.some((perm) =>
                currentUserHasPermission(perm, resource)
              );

          setIsAuthorized(permCheck);
          return;
        }
      } catch (error) {
        console.error("Authorization check failed:", error);
        setIsAuthorized(false);
      }

      // No checks specified
      setIsAuthorized(true);
    };

    checkAuthorization();
  }, [
    policy,
    roles,
    permissions,
    resource,
    requireAll,
    authorize,
    currentUserHasRole,
    currentUserHasPermission,
  ]);

  if (isAuthorized === null) {
    return <>{fallback}</>;
  }

  if (!isAuthorized) {
    return <>{unauthorized}</>;
  }

  return <>{children}</>;
};
