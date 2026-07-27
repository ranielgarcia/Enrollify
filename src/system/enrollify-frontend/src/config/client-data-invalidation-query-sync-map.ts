import type { QueryKey } from "@tanstack/react-query";
import {
  ClientDataInvalidationTypes,
  type ClientDataInvalidationType,
} from "@/config/client-data-invalidation-types";
import { queryKeys as classSectionKeys } from "@/api/collections/class-section-collection";

/**
 * Maps each client data invalidation `type` string to an array of TanStack Query keys
 * that should be invalidated when that client data invalidation event is received over SignalR.
 * Use `queryKeys.base()` for a broad invalidation of all queries in a domain,
 * or a more specific key (e.g., `queryKeys.detail(id)`) when you know the
 * exact resource that changed.
 */
export const clientDataInvalidationQuerySyncMap: Partial<
  Record<ClientDataInvalidationType, QueryKey[]>
> = {
  // ── Class Sections ──────────────────────────────────────────────────────────
  [ClientDataInvalidationTypes.ClassSectionMovedToDraftEvent]: [
    classSectionKeys.base(),
  ],
};
