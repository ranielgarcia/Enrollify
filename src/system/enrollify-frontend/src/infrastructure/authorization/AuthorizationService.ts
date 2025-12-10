import type { AuthorizationEvaluationContext } from "./models/AuthorizationEvaluationContext";
import type { IAuthorizationPolicy } from "./policies/IAuthorizationPolicy";
import type { AuthorizationResult } from "./AuthorizationResult";
import type { IAuthorizationHandler } from "./handlers/IAuthorizationHandler";
import type { PolicyRegistry } from "./policies/PolicyRegistry";
import type { IAuthorizationRequirement } from "./requirements/IAuthorizationRequirement";
import type { AuthorizationScope } from "./models/AuthorizationScope";
import type { PolicyName } from "./models/PolicyNames";
import type { Permission } from "./models/PermissionsEnum";
import type { Role } from "./models/Roles";
import type { UserContext } from "../../api/models/UserContext";

export class AuthorizationService {
  private handlerMap: Map<string, IAuthorizationHandler>;
  private policyRegistry: PolicyRegistry;
  private handlers: IAuthorizationHandler[];

  constructor(
    policyRegistry: PolicyRegistry,
    handlers: IAuthorizationHandler[]
  ) {
    this.policyRegistry = policyRegistry;
    this.handlers = handlers;
    // Create a map of requirement types to handlers for faster lookup
    this.handlerMap = new Map();
    handlers.forEach((handler) => {
      this.handlerMap.set(handler.requirementType, handler);
    });
  }

  async authorize(
    policyName: PolicyName,
    context: AuthorizationEvaluationContext
  ): Promise<AuthorizationResult> {
    const policy = this.policyRegistry.get(policyName);

    if (!policy) {
      return {
        succeeded: false,
        failureReasons: [`Policy '${policyName}' not found`],
        policy: policyName,
      };
    }

    return this.evaluatePolicy(policy, context);
  }

  async authorizeWithRequirements(
    requirements: IAuthorizationRequirement[],
    context: AuthorizationEvaluationContext,
    requireAll: boolean = true
  ): Promise<AuthorizationResult> {
    const failureReasons: string[] = [];

    // Evaluate each requirement using appropriate handler
    const results = await Promise.all(
      requirements.map(async (req) => {
        try {
          const handler = this.handlerMap.get(req.type);

          if (!handler) {
            // Fallback to requirement's own evaluate method
            return await Promise.resolve(req.evaluate(context));
          }

          return await Promise.resolve(handler.handle(req, context));
        } catch (error) {
          failureReasons.push(
            `Requirement '${req.type}' evaluation failed: ${error}`
          );
          return false;
        }
      })
    );

    const succeeded = requireAll
      ? results.every((r) => r)
      : results.some((r) => r);

    return {
      succeeded,
      failureReasons: succeeded
        ? undefined
        : failureReasons.length > 0
          ? failureReasons
          : ["One or more requirements failed"],
    };
  }

  private async evaluatePolicy(
    policy: IAuthorizationPolicy,
    context: AuthorizationEvaluationContext
  ): Promise<AuthorizationResult> {
    return this.authorizeWithRequirements(
      policy.requirements,
      context,
      policy.requireAll
    );
  }

  hasRole(user: UserContext | null, ...roles: Role[]): boolean {
    return roles.some((role) =>
      user?.roles?.map((r) => r.id)?.includes(role.id)
    );
  }

  hasPermission(
    user: UserContext | null,
    permission: Permission,
    authorizationScope?: AuthorizationScope
  ): boolean {
    if (!user?.roles) return false;

    // Iterate through all roles to find the permission
    for (const role of user.roles) {
      if (!role.permissionScopes) continue;

      for (const permissionScope of role.permissionScopes) {
        // If scope is specified, check if it matches
        if (authorizationScope) {
          const scopeMatches =
            permissionScope.permissionScope?.name ===
              authorizationScope.scope.name &&
            (!authorizationScope.scope.id ||
              permissionScope.permissionScope?.value ===
                authorizationScope.scope.id);

          if (!scopeMatches) continue;
        }

        // Check if the permission exists in this scope
        const hasPermission = permissionScope.permissions?.some(
          (p) => p.name === permission.name
        );

        if (hasPermission) return true;
      }
    }

    return false;
  }

  /**
   * Register a new handler at runtime
   */
  registerHandler(handler: IAuthorizationHandler): void {
    this.handlers.push(handler);
    this.handlerMap.set(handler.requirementType, handler);
  }

  /**
   * Get all registered handlers
   */
  getHandlers(): IAuthorizationHandler[] {
    return [...this.handlers];
  }
}
