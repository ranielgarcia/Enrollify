import { queryOptions, type UseQueryOptions } from "@tanstack/react-query";
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

interface useQueryParams<P extends ApiPath, TError = unknown> {
  path: P;
  params?: QueryParamsForPath<P>;
  options?: UseQueryOptions<JsonResponseForPath<P>, TError> & {
    queryKey?: readonly unknown[];
  };
  forceRefreshToken?: boolean;
}

const createAppQueryOptions = <P extends ApiPath, TError = unknown>({
  path,
  params,
  options,
  forceRefreshToken,
}: useQueryParams<P, TError>) => {
  options = queryOptions({
    // default inferred queryKey based on path + params; allow override
    queryKey:
      options?.queryKey ?? (params !== undefined ? [path, params] : [path]),
    ...options,
    queryFn: async () => {
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
    },
  });

  return options;
};

export default createAppQueryOptions;
