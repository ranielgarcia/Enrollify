import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import {
  type UseQueryOptions,
  useQuery as useReactQuery,
} from "@tanstack/react-query";
import axios from "axios";

import { loginRequest } from "@/infrastructure/authentication/auth-config";
import { Config } from "@/infrastructure/configurations/app-config";
import type { paths as ApiPaths } from "@/api/generated/api";

type ApiPath = keyof ApiPaths;

interface useQueryParams<TData extends object, TError = unknown> {
  path: ApiPath;
  params?: object;
  queryOptions?: UseQueryOptions<TData, TError>;
  forceRefreshToken?: boolean;
}

const useAppQuery = <TData extends object, TError = unknown>({
  path,
  params,
  queryOptions,
  forceRefreshToken = false,
}: useQueryParams<TData, TError>) => {
  const { instance, accounts } = useMsal();

  const request = {
    ...loginRequest,
    account: accounts[0],
    forceRefresh: forceRefreshToken,
  };

  queryOptions = {
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

      const response = await axios.get<TData>(`${Config.API_URL}${path}`, {
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
        params: params,
      });

      return response.data;
    },
  } as UseQueryOptions<TData, TError>;

  return useReactQuery({ ...queryOptions });
};

export default useAppQuery;
