import {
  queryOptions,
  type UseSuspenseQueryOptions,
  type UseQueryOptions,
} from "@tanstack/react-query";
import axios from "axios";

import { Config } from "@/infrastructure/Configurations/app-config";
import type { paths as ApiPaths } from "@/api/generated/api";
import { getCurrentAccessToken } from "@/infrastructure/authentication/tokenFetcher";
import { msalInstance } from "@/infrastructure/authentication/authConfig";

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

interface QueryParams<
  P extends ApiPath,
  TData = JsonResponseForPath<P>,
  TError = unknown,
> {
  path: P;
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
 * Creates a query function for fetching data from the API.
 */
const createQueryFn =
  <P extends ApiPath>(
    path: P,
    params: QueryParamsForPath<P> | undefined,
    forceRefreshToken: boolean | undefined
  ) =>
  async () => {
    const accessToken = await getCurrentAccessToken({
      msalInstance,
      forceRefreshToken,
    });

    const response = await axios.get<JsonResponseForPath<P>>(
      `${Config.API_URL}${path}`,
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
  params,
  options,
  forceRefreshToken,
}: QueryParams<P, TData, TError>) => {
  return queryOptions({
    queryKey:
      options?.queryKey ?? (params !== undefined ? [path, params] : [path]),
    ...options,
    queryFn: createQueryFn(path, params, forceRefreshToken),
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
  params,
  options,
  forceRefreshToken,
}: QueryParams<P, TData, TError>): UseSuspenseQueryOptions<
  JsonResponseForPath<P>,
  TError,
  TData
> => {
  return {
    queryKey:
      options?.queryKey ?? (params !== undefined ? [path, params] : [path]),
    ...options,
    queryFn: createQueryFn(path, params, forceRefreshToken),
  };
};

export default createAppQueryOptions;
