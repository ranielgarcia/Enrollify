import {
  mutationOptions,
  type UseMutationOptions,
} from "@tanstack/react-query";
import axios, { AxiosError, type AxiosProgressEvent } from "axios";
import { Config } from "@/infrastructure/configurations/app-config";
import type { paths as ApiPaths } from "@/api/generated/api";
import { getCurrentAccessToken } from "@/infrastructure/authentication/tokenFetcher";
import { msalInstance } from "@/infrastructure/authentication/auth-config";
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

// Extract the path params type for a GET operation on a given path.
type PathParamsForPathVerb<P extends ApiPath, V extends HttpVerb> =
  ApiPaths[P] extends Record<V, infer Op>
    ? Op extends { parameters: { path: infer Q } }
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
  pathParams?: PathParamsForPathVerb<P, V>;
  isMultipart?: boolean;
  onUploadProgressCallBack?: (progressEvent: AxiosProgressEvent) => void;
  forceRefreshToken?: boolean;
  options?: UseMutationOptions<
    JsonResponseForPathVerb<P, V>,
    AxiosError,
    JsonRequestBodyForPathVerb<P, V>
  >;
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

const createMutationOptions = <P extends ApiPath, V extends HttpVerb = "post">({
  mutationKey,
  path,
  params,
  pathParams,
  onUploadProgressCallBack,
  httpVerb = "post" as V,
  isMultipart = false,
  forceRefreshToken = false,
  options,
}: UseMutationParams<P, V>) => {
  const appMutationOptions = mutationOptions({
    mutationKey: Array.isArray(mutationKey)
      ? mutationKey
      : [mutationKey, params],
    ...options,
    mutationFn: async (
      formData: FormData | JsonRequestBodyForPathVerb<P, V>
    ) => {
      const accessToken = await getCurrentAccessToken({
        msalInstance,
        forceRefreshToken,
      });

      const interpolatedPath = interpolatePath(
        path,
        pathParams as Record<string, string | number> | undefined
      );

      if (isMultipart) {
        const response = await axios<JsonResponseForPathVerb<P, V>>({
          method: httpVerb,
          url: `${Config.API_URL}${interpolatedPath}`,
          data: formData as FormData,
          headers: {
            Authorization: `Bearer ${accessToken}`,
            "Content-Type": "multipart/form-data",
            Accept: "*/*",
          },
          params: params as QueryParamsForPathVerb<P, V>,
          onUploadProgress: onUploadProgressCallBack,
        });

        return response.data;
      }

      const response = await axios<JsonResponseForPathVerb<P, V>>({
        method: httpVerb,
        url: `${Config.API_URL}${interpolatedPath}`,
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

  return appMutationOptions;
};

export default createMutationOptions;
