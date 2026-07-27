import type { QueryClient } from "@tanstack/react-query";
import { clientDataInvalidationQuerySyncMap } from "@/config/client-data-invalidation-query-sync-map";

/**
 * Invalidates TanStack Query caches based on a client data invalidation `type` value.
 *
 * Called from `useClientDataInvalidationHub` after each `ReceiveClientDataInvalidation` event.
 * Looks up the client data invalidation type in `clientDataInvalidationQuerySyncMap` and fires
 * `queryClient.invalidateQueries` for every matched query key.
 *
 * Is a no-op when the type has no entry in the map (safe fallback).
 */
export function syncQueriesForClientDataInvalidation(
  type: string,
  queryClient: QueryClient,
): void {
  const keysToInvalidate =
    clientDataInvalidationQuerySyncMap[
      type as keyof typeof clientDataInvalidationQuerySyncMap
    ];

  if (!keysToInvalidate?.length) return;

  for (const queryKey of keysToInvalidate) {
    queryClient.invalidateQueries({ queryKey });
  }
}
