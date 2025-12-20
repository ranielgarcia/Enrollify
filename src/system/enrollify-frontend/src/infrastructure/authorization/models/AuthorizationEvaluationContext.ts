import type { AuthorizationResource } from "./AuthorizationResource";
import type { UserContext } from "../../../api/models/UserContext";

/**
 * Context for evaluating authorization requirements.
 *
 * @example
 * // Feature-level authorization
 * const context: AuthorizationEvaluationContext = {
 *   user: currentUser,
 *   resource: { scope: "Rooms" }
 * };
 *
 * // Entity-level authorization
 * const context: AuthorizationEvaluationContext = {
 *   user: currentUser,
 *   resource: { scope: "Rooms", entityId: 42 }
 * };
 */
export interface AuthorizationEvaluationContext {
  /**
   * The authenticated user context from the backend.
   */
  user: UserContext;

  /**
   * Optional: The resource being authorized against.
   * Use for both feature-level and entity-level authorization.
   */
  resource?: AuthorizationResource;
}
