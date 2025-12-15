import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import { useMutation as useReactMutation } from "@tanstack/react-query";
import axios, { AxiosError, type AxiosProgressEvent } from "axios";

import { loginRequest } from "@/infrastructure/authentication/authConfig";
import { Config } from "@/infrastructure/Configurations/app-config";
import type { paths as ApiPaths } from "@/api/generated/api";
type HttpVerb = "post" | "put" | "delete";
type ApiPath = keyof ApiPaths;

// Extract response type for a given path + verb
type JsonResponseForPathVerb<P extends ApiPath, V extends HttpVerb> =
  ApiPaths[P] extends Record<V, infer Op>
    ? Op extends {
        responses: {
          200: {
            content: { "application/json": infer T };
          };
        };
      }
      ? T
      : never
    : never;

// Extract query params type for a given path + verb (operation-level params)
type QueryParamsForPathVerb<P extends ApiPath, V extends HttpVerb> =
  ApiPaths[P] extends Record<V, infer Op>
    ? Op extends { parameters: { query: infer Q } }
      ? Q
      : undefined
    : undefined;

// Extract request body type (application/json) for a given path + verb
type JsonRequestBodyForPathVerb<P extends ApiPath, V extends HttpVerb> =
  ApiPaths[P] extends Record<V, infer Op>
    ? Op extends {
        requestBody: {
          content: { "application/json": infer B };
        };
      }
      ? B
      : unknown
    : unknown;

interface UseMutationParams<P extends ApiPath, V extends HttpVerb> {
  httpVerb?: V;
  mutationKey: string | readonly unknown[];
  path: P;
  params?: QueryParamsForPathVerb<P, V>;
  isMultipart?: boolean;
  onUploadProgressCallBack?: (progressEvent: AxiosProgressEvent) => void;
  forceRefreshToken?: boolean;
}

const useAppMutation = <P extends ApiPath, V extends HttpVerb = "post">({
  mutationKey,
  path,
  params,
  onUploadProgressCallBack,
  httpVerb = "post" as V,
  isMultipart = false,
  forceRefreshToken = false,
}: UseMutationParams<P, V>) => {
  const { instance, accounts } = useMsal();

  const request = {
    ...loginRequest,
    account: accounts[0],
    forceRefresh: forceRefreshToken,
  };

  return useReactMutation<
    JsonResponseForPathVerb<P, V>,
    AxiosError,
    JsonRequestBodyForPathVerb<P, V>
  >({
    mutationKey: Array.isArray(mutationKey)
      ? mutationKey
      : [mutationKey, params],
    onError: (err: AxiosError) => err,
    mutationFn: async (
      formData: FormData | JsonRequestBodyForPathVerb<P, V>
    ) => {
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

      if (isMultipart && httpVerb === "post") {
        const response = await axios.post<JsonResponseForPathVerb<P, V>>(
          `${Config.API_URL}${path}`,
          formData as FormData,
          {
            headers: {
              Authorization: `Bearer ${accessToken}`,
              "Content-Type": "multipart/form-data",
              Accept: "*/*",
            },
            params: params as QueryParamsForPathVerb<P, V>,
            onUploadProgress: onUploadProgressCallBack,
          }
        );

        return response.data;
      }

      const response = await axios<JsonResponseForPathVerb<P, V>>({
        method: httpVerb,
        url: `${Config.API_URL}${path}`,
        data: formData,
        headers: {
          Authorization: `Bearer ${accessToken}`,
          "Content-Type": "application/json; charset=utf8",
        },
        params: params as QueryParamsForPathVerb<P, V>,
      });

      return response.data;
    },
  });
};

export default useAppMutation;
