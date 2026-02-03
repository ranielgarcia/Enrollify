import createAppQueryOptions, {
  createAppSuspenseQueryOptions,
} from "@/hooks/create-query-options";
import { useQuery, useSuspenseQuery } from "@tanstack/react-query";

const queryKeys = {
  me: () => ["me"],
};

export const getMeQueryOptions = () =>
  createAppQueryOptions({
    path: "/api/me",
    options: {
      meta: { persist: true },
      queryKey: queryKeys.me(),
      staleTime: 1000 * 60 * 5, // 5 minutes - use cached data without refetching
      gcTime: 1000 * 60 * 60 * 24, // 24 hours - keep in cache for persistence
    },
  });

export const getMeSuspenseQueryOptions = () =>
  createAppSuspenseQueryOptions({
    path: "/api/me",
    options: {
      meta: { persist: true },
      queryKey: queryKeys.me(),
      staleTime: 1000 * 60 * 5,
      gcTime: 1000 * 60 * 60 * 24,
    },
  });

export const useGetMeDetails = (options?: { enabled?: boolean }) =>
  useQuery({
    ...getMeQueryOptions(),
    enabled: options?.enabled ?? true,
  });

export const useGetMeDetailsSuspense = () =>
  useSuspenseQuery(getMeSuspenseQueryOptions());
