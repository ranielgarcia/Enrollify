import {
  queryOptions,
  type UseSuspenseQueryOptions,
  type UseQueryOptions,
} from "@tanstack/react-query";
import axios from "axios";

import { Config } from "@/infrastructure/configurations/app-config";
import type { paths as ApiPaths } from "@/api/generated/api";
import { getCurrentAccessToken } from "@/infrastructure/authentication/tokenFetcher";
import { msalInstance } from "@/infrastructure/authentication/auth-config";

type ApiPath = keyof ApiPaths;

// Extract the JSON response type for a GET operation on a given path.
type JsonResponseForPath<P extends ApiPath> = ApiPaths[P] extends {
  get: infer GetOp;
}
  ? GetOp extends {
      responses: {
        200: {
          content: { "application/json": infer T };
        };
      };
    }
    ? T
    : never
  : never;

// Helper type to extract parameters from a GET operation
type GetOperationParams<P extends ApiPath> = ApiPaths[P] extends {
  get: infer GetOp;
}
  ? GetOp extends { parameters: infer Params }
    ? Params
    : never
  : never;

// Extract the query params type for a GET operation on a given path.
// Handles both required (query:) and optional (query?:) query parameters.
type QueryParamsForPath<P extends ApiPath> =
  GetOperationParams<P> extends { query?: infer Q }
    ? Q
    : GetOperationParams<P> extends { query: infer Q }
      ? Q
      : undefined;

// Extract the path params type for a GET operation on a given path.
// Handles both required (path:) and optional (path?:) path parameters.
type PathParamsForPath<P extends ApiPath> =
  GetOperationParams<P> extends { path?: infer PathP }
    ? PathP
    : GetOperationParams<P> extends { path: infer PathP }
      ? PathP
      : undefined;

interface QueryParams<
  P extends ApiPath,
  TData = JsonResponseForPath<P>,
  TError = unknown,
> {
  path: P;
  pathParams?: PathParamsForPath<P>;
  params?: QueryParamsForPath<P>;
  options?: Omit<
    UseQueryOptions<JsonResponseForPath<P>, TError, TData>,
    "queryFn"
  > & {
    queryKey?: readonly unknown[];
  };
  forceRefreshToken?: boolean;
}

/**
 * Interpolates path parameters into the URL path.
 * e.g., "/api/room-types/{roomTypeId}/rooms/count" with { roomTypeId: "123" }
 * becomes "/api/room-types/123/rooms/count"
 */
const interpolatePath = <P extends ApiPath>(
  path: P,
  pathParams: Record<string, string | number> | undefined,
): string => {
  if (!pathParams) return path;

  let interpolatedPath: string = path;
  for (const [key, value] of Object.entries(pathParams)) {
    interpolatedPath = interpolatedPath.replace(`{${key}}`, String(value));
  }
  return interpolatedPath;
};

/**
 * Creates a query function for fetching data from the API.
 */
const createQueryFn =
  <P extends ApiPath>(
    path: P,
    pathParams: PathParamsForPath<P> | undefined,
    params: QueryParamsForPath<P> | undefined,
    forceRefreshToken: boolean | undefined,
  ) =>
  async () => {
    const accessToken = await getCurrentAccessToken({
      msalInstance,
      forceRefreshToken,
    });

    const interpolatedPath = interpolatePath(
      path,
      pathParams as Record<string, string | number> | undefined,
    );

    const response = await axios.get<JsonResponseForPath<P>>(
      `${Config.API_URL}${interpolatedPath}`,
      {
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
        params: params as QueryParamsForPath<P>,
      },
    );

    return response.data;
  };

/**
 * Creates query options for use with `useQuery`.
 */
const createAppQueryOptions = <
  P extends ApiPath,
  TData = JsonResponseForPath<P>,
  TError = unknown,
>({
  path,
  pathParams,
  params,
  options,
  forceRefreshToken,
}: QueryParams<P, TData, TError>) => {
  // Build query key including path params for proper cache isolation
  const baseKey = pathParams !== undefined ? [path, pathParams] : [path];
  const queryKey =
    options?.queryKey ??
    (params !== undefined ? [...baseKey, params] : baseKey);

  return queryOptions({
    queryKey,
    retry: (failureCount, error) => {
      if (failureCount >= 3) return false;
      const status = (error as any)?.response?.status;
      if (status === 401 || status === 403 || status === 404) return false;
      return true;
    },
    retryDelay: (attemptIndex) =>
      Math.min(1000 * Math.pow(2, attemptIndex), 30000) +
      Math.random() * 1000,
    ...options,
    queryFn: createQueryFn(path, pathParams, params, forceRefreshToken),
  });
};

/**
 * Creates suspense query options for use with `useSuspenseQuery`.
 * Unlike regular query options, suspense queries cannot use `skipToken`.
 */
export const createAppSuspenseQueryOptions = <
  P extends ApiPath,
  TData = JsonResponseForPath<P>,
  TError = unknown,
>({
  path,
  pathParams,
  params,
  options,
  forceRefreshToken,
}: QueryParams<P, TData, TError>): UseSuspenseQueryOptions<
  JsonResponseForPath<P>,
  TError,
  TData
> => {
  // Build query key including path params for proper cache isolation
  const baseKey = pathParams !== undefined ? [path, pathParams] : [path];
  const queryKey =
    options?.queryKey ??
    (params !== undefined ? [...baseKey, params] : baseKey);

  return {
    queryKey,
    retry: (failureCount: number, error: TError) => {
      if (failureCount >= 3) return false;
      const status = (error as any)?.response?.status;
      if (status === 401 || status === 403 || status === 404) return false;
      return true;
    },
    retryDelay: (attemptIndex: number) =>
      Math.min(1000 * Math.pow(2, attemptIndex), 30000) +
      Math.random() * 1000,
    ...options,
    queryFn: createQueryFn(path, pathParams, params, forceRefreshToken),
  };
};

export default createAppQueryOptions;
