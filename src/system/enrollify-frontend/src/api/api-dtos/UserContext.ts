import type { components } from "@/api/generated/api";

export type UserContext =
  | components["schemas"]["EnrollifyCoreAuthenticationUserContext"]
  | null;
