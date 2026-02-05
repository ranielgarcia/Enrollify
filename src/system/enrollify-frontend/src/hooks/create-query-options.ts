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

// Extract the query params type for a GET operation on a given path.
type QueryParamsForPath<P extends ApiPath> = ApiPaths[P] extends {
  get: infer GetOp;
}
  ? GetOp extends { parameters: { query: infer Q } }
    ? Q
    : undefined
  : undefined;

// Extract the path params type for a GET operation on a given path.
type PathParamsForPath<P extends ApiPath> = ApiPaths[P] extends {
  get: infer GetOp;
}
  ? GetOp extends { parameters: { path: infer PathP } }
    ? PathP
    : undefined
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
  pathParams: Record<string, string | number> | undefined
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
    forceRefreshToken: boolean | undefined
  ) =>
  async () => {
    const accessToken = await getCurrentAccessToken({
      msalInstance,
      forceRefreshToken,
    });

    const interpolatedPath = interpolatePath(
      path,
      pathParams as Record<string, string | number> | undefined
    );

    const response = await axios.get<JsonResponseForPath<P>>(
      `${Config.API_URL}${interpolatedPath}`,
      {
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
        params: params as QueryParamsForPath<P>,
      }
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
    ...options,
    queryFn: createQueryFn(path, pathParams, params, forceRefreshToken),
  };
};

export default createAppQueryOptions;
