import { useEffect, useState, type PropsWithChildren } from "react";
import { useAuthenticationContext } from "../authentication/authentication-context";
import { AuthorizationService } from "./AuthorizationService";
import { PolicyRegistry } from "./policies/PolicyRegistry";
import type { IAuthorizationHandler } from "./handlers/IAuthorizationHandler";
import { RoleHandler } from "./handlers/RoleHandler";
import { PermissionHandler } from "./handlers/PermissionHandler";
import type { AuthorizationResult } from "./AuthorizationResult";
import type { AuthorizationEvaluationContext } from "./models/AuthorizationEvaluationContext";
import { AuthorizationContext } from "./AuthorizationContext";
import type { AuthorizationResource } from "./models/AuthorizationResource";
import type { PolicyName } from "./models/PolicyNames";
import type { Permission } from "./models/PermissionsEnum";
import type { Role } from "./models/Roles";
import { registerRoomPolicies } from "./policies/rooms-policies";
import { registerCollegesPolicies } from "./policies/colleges-policies";
import { registerBuildingsPolicies } from "./policies/buildings-policies";
import { registerDepartmentPolicies } from "./policies/department-policies";
import { registerCoursesPolicies } from "./policies/courses-policies";
import { registerSubjectsPolicies } from "./policies/subjects-policies";
import { registerCurriculumPolicies } from "./policies/curriculum-policies";

export const AuthorizationProvider: React.FC<PropsWithChildren> = ({
  children,
}) => {
  // const queryClient = useQueryClient();
  const { user, isLoading: isUserLoading } = useAuthenticationContext();
  const [authService, setAuthService] = useState<AuthorizationService | null>(
    null
  );

  // Authorization is ready when auth service is initialized and user data is loaded
  const isReady =
    authService !== null &&
    !isUserLoading &&
    user !== null &&
    user !== undefined;

  useEffect(() => {
    // Initialize authorization service with handlers and policies
    const policyRegistry = new PolicyRegistry();
    registerRoomPolicies(policyRegistry);
    registerCollegesPolicies(policyRegistry);
    registerBuildingsPolicies(policyRegistry);
    registerDepartmentPolicies(policyRegistry);
    registerCoursesPolicies(policyRegistry);
    registerSubjectsPolicies(policyRegistry);
    registerCurriculumPolicies(policyRegistry);

    const handlers: IAuthorizationHandler[] = [
      new RoleHandler(),
      new PermissionHandler(),
    ];

    // eslint-disable-next-line react-hooks/set-state-in-effect
    setAuthService(new AuthorizationService(policyRegistry, handlers));
  }, []);

  const authorize = async (
    policyName: PolicyName,
    resource?: AuthorizationResource
  ): Promise<AuthorizationResult> => {
    if (!authService || !user) {
      return { succeeded: false, failureReasons: ["Not authenticated"] };
    }

    const context: AuthorizationEvaluationContext = {
      user,
      resource,
    };
    return authService.authorize(policyName, context);
  };

  /**
   * Check if authorization against a policy succeeds.
   * @param policyName - The policy to evaluate
   * @param resource - Optional resource for scoped/entity-level authorization
   */
  const checkPolicy = async (
    policyName: PolicyName,
    resource?: AuthorizationResource
  ): Promise<boolean> => {
    const result = await authorize(policyName, resource);
    return result.succeeded;
  };

  const currentUserHasRole = (...roles: Role[]): boolean => {
    if (!authService || !user) return false;
    return authService.hasRole(user, ...roles);
  };

  const currentUserHasPermission = (
    permission: Permission,
    resource?: AuthorizationResource
  ): boolean => {
    if (!authService || !user) return false;
    return authService.hasPermission(user, permission, resource);
  };

  return (
    <AuthorizationContext.Provider
      value={{
        isReady,
        authorize,
        checkPolicy,
        currentUserHasRole,
        currentUserHasPermission,
        authService,
      }}
    >
      {children}
    </AuthorizationContext.Provider>
  );
};
