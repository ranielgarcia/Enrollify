import type { IAuthorizationRequirement } from "../requirements/IAuthorizationRequirement";
import type { PolicyName } from "../models/PolicyNames";

export interface IAuthorizationPolicy {
  name: PolicyName;
  requirements: IAuthorizationRequirement[];
  requireAll?: boolean; // AND vs OR logic
}
