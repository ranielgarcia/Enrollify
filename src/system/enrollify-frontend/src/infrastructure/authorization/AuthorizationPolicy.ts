import type { IAuthorizationRequirement } from "./requirements/IAuthorizationRequirement";

export interface AuthorizationPolicy {
  name: string;
  requirements: IAuthorizationRequirement[];
  requireAll?: boolean; // AND vs OR logic
}
