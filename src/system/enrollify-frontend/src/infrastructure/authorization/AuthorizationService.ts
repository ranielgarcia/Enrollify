import type {
  AuthorizationContext,
  AuthorizationScope,
} from "./AuthorizationEvaluationContext";
import type { AuthorizationPolicy } from "./AuthorizationPolicy";
import type { AuthorizationResult } from "./AuthorizationResult";
import type { IAuthorizationHandler } from "./handlers/IAuthorizationHandler";
import type { PolicyRegistry } from "./policies/PolicyRegistry";
import type { IAuthorizationRequirement } from "./requirements/IAuthorizationRequirement";
import type { components } from "@/api/generated/api";

type UserContext =
  components["schemas"]["EnrollifyCoreAuthenticationUserContext"];

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
    policyName: string,
    context: AuthorizationContext
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
    context: AuthorizationContext,
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
    policy: AuthorizationPolicy,
    context: AuthorizationContext
  ): Promise<AuthorizationResult> {
    return this.authorizeWithRequirements(
      policy.requirements,
      context,
      policy.requireAll
    );
  }

  hasRole(user: UserContext | null, ...roles: string[]): boolean {
    return roles.some((role) =>
      user?.roles?.map((r) => r.name)?.includes(role)
    );
  }

  hasPermission(
    user: UserContext | null,
    permission: string,
    scope?: AuthorizationScope
  ): boolean {
    if (!user?.roles) return false;

    // Iterate through all roles to find the permission
    for (const role of user.roles) {
      if (!role.permissionScopes) continue;

      for (const permissionScope of role.permissionScopes) {
        // If scope is specified, check if it matches
        if (scope) {
          const scopeMatches =
            permissionScope.permissionScope?.name === scope.type &&
            (!scope.id || permissionScope.permissionScope?.value === scope.id);

          if (!scopeMatches) continue;
        }

        // Check if the permission exists in this scope
        const hasPermission = permissionScope.permissions?.some(
          (p) => p.name === permission
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
