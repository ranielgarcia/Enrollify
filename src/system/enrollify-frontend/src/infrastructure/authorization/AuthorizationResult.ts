import type { PolicyName } from "./models/PolicyNames";

export interface AuthorizationResult {
  succeeded: boolean;
  failureReasons?: string[];
  policy?: PolicyName;
}
