import type { AuthorizationEvaluationContext } from "../AuthorizationEvaluationContext";
import type { IAuthorizationRequirement } from "../requirements/IAuthorizationRequirement";

export interface IAuthorizationHandler {
  /**
   * The type of requirement this handler can process
   */
  readonly requirementType: string;

  /**
   * Determines whether the handler can handle the given requirement
   */
  canHandle(requirement: IAuthorizationRequirement): boolean;

  /**
   * Handles the authorization requirement evaluation
   * @returns Promise<boolean> indicating if the requirement is satisfied
   */
  handle(
    requirement: IAuthorizationRequirement,
    context: AuthorizationEvaluationContext
  ): Promise<boolean> | boolean;
}
