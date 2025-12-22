import "@tanstack/react-query";

declare module "@tanstack/react-query" {
  interface MutationMeta extends Record<string, unknown> {
    /**
     * Query keys to invalidate when the mutation settles (succeeds or fails).
     * Should match the query key structure used in your query options.
     */
    invalidateQueries?: readonly unknown[];

    /**
     * Whether to persist this mutation's data in the cache.
     */
    persist?: boolean;
  }

  interface QueryMeta extends Record<string, unknown> {
    /**
     * Whether to persist this query's data in local storage.
     */
    persist?: boolean;
  }
}
