/**
 * Canonical client data invalidation type string constants.
 * Convention: add a new entry here whenever a new client data invalidation type is
 * introduced on the backend. The value must match the backend string exactly.
 * These constants are the source of truth used by `client-data-invalidation-query-sync-map.ts`
 * to map client data invalidation events to TanStack Query invalidations.
 */
export const ClientDataInvalidationTypes = {
  ClassSectionMovedToDraftEvent: "ClassSectionMovedToDraftEvent",
  ClassSectionMovedToValidatingEvent: "ClassSectionMovedToValidatingEvent",
} as const;

export type ClientDataInvalidationType =
  (typeof ClientDataInvalidationTypes)[keyof typeof ClientDataInvalidationTypes];
