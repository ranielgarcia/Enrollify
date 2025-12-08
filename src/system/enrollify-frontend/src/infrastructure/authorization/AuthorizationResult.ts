export interface AuthorizationResult {
  succeeded: boolean;
  failureReasons?: string[];
  policy?: string;
}
