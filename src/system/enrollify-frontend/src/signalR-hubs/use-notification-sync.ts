import type { QueryClient } from "@tanstack/react-query";
import { notificationQuerySyncMap } from "@/config/notification-query-sync-map";

/**
 * Invalidates TanStack Query caches based on a notification's `type` field.
 *
 * Called from `useNotificationHub` after each `ReceiveNotification` event.
 * Looks up the notification type in `notificationQuerySyncMap` and fires
 * `queryClient.invalidateQueries` for every matched query key.
 *
 * Is a no-op when the type has no entry in the map (safe fallback).
 */
export function syncQueriesForNotification(
  type: string,
  queryClient: QueryClient,
): void {
  const keysToInvalidate =
    notificationQuerySyncMap[type as keyof typeof notificationQuerySyncMap];

  if (!keysToInvalidate?.length) return;

  for (const queryKey of keysToInvalidate) {
    queryClient.invalidateQueries({ queryKey });
  }
}
