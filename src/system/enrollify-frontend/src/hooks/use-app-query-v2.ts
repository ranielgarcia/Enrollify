import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import {
  type UseQueryOptions,
  useQuery as useReactQuery,
} from "@tanstack/react-query";
import axios from "axios";

import { loginRequest } from "@/infrastructure/auth/authConfig";
import { Config } from "@/infrastructure/Configurations/app-config";
import type { paths as ApiPaths } from "@/api/generated/api";

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

// Extract the query params type for a given path, if present.
type QueryParamsForPath<P extends ApiPath> = ApiPaths[P] extends {
  parameters: { query?: infer Q };
}
  ? Q extends never
    ? undefined
    : Q
  : undefined;

interface useQueryParams<P extends ApiPath, TError = unknown> {
  path: P;
  params?: QueryParamsForPath<P>;
  queryOptions?: UseQueryOptions<JsonResponseForPath<P>, TError> & {
    queryKey?: readonly unknown[];
  };
  forceRefreshToken?: boolean;
}

const useAppQuery = <P extends ApiPath, TError = unknown>({
  path,
  params,
  queryOptions,
  forceRefreshToken = false,
}: useQueryParams<P, TError>) => {
  const { instance, accounts } = useMsal();

  const request = {
    ...loginRequest,
    account: accounts[0],
    forceRefresh: forceRefreshToken,
  };

  queryOptions = {
    // default inferred queryKey based on path + params; allow override
    queryKey:
      queryOptions?.queryKey ??
      (params !== undefined ? [path, params] : [path]),
    ...queryOptions,
    queryFn: async () => {
      let accessToken = null;

      try {
        const tokenResponse = await instance.acquireTokenSilent(request);
        accessToken = tokenResponse.accessToken;
      } catch (e) {
        if (e instanceof InteractionRequiredAuthError) {
          const tokenResponse = await instance.acquireTokenPopup(request);
          accessToken = tokenResponse.accessToken;
        }
      }

      if (!accessToken) {
        // Either throw to set error state...
        throw new Error("Failed to acquire access token.");
        // Or return an empty shape:
        // return {} as TData;
      }

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
  } as UseQueryOptions<JsonResponseForPath<P>, TError>;

  return useReactQuery({ ...queryOptions });
};

export default useAppQuery;
