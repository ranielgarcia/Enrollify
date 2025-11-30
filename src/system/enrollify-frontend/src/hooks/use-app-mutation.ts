import { InteractionRequiredAuthError } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import { useMutation as useReactMutation } from "@tanstack/react-query";
import axios, { AxiosError, type AxiosProgressEvent } from "axios";

import { loginRequest } from "@/infrastructure/auth/authConfig";
import { Config } from "@/infrastructure/Configurations/app-config";

interface useQueryParams {
  httpVerb?: "post" | "delete" | "put";
  mutationKey: string;
  path: string;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  params?: any;
  isMultipart?: boolean;
  onUploadProgressCallBack?: (progressEvent: AxiosProgressEvent) => void;
  forceRefreshToken?: boolean;
}

const useAppMutation = <TData extends object>({
  mutationKey,
  path,
  params,
  onUploadProgressCallBack,
  httpVerb = "post",
  isMultipart = false,
  forceRefreshToken = false,
}: useQueryParams) => {
  const { instance, accounts } = useMsal();

  const request = {
    ...loginRequest,
    account: accounts[0],
    forceRefresh: forceRefreshToken,
  };

  return useReactMutation({
    mutationKey: [mutationKey, params],
    onError: (err: AxiosError) => err,
    mutationFn: async (formData: FormData | object) => {
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

      if (isMultipart) {
        const response = await axios.post<TData>(
          `${Config.API_URL}${path}`,
          formData,
          {
            headers: {
              Authorization: `Bearer ${accessToken}`,
              "Content-Type": "multipart/form-data",
              Accept: "*/*",
            },
            params: params,
            onUploadProgress: onUploadProgressCallBack,
          }
        );

        return response.data;
      }

      const response = await axios<TData>({
        method: httpVerb,
        url: `${Config.API_URL}${path}`,
        data: formData,
        headers: {
          Authorization: `Bearer ${accessToken}`,
          "Content-Type": "application/json; charset=utf8",
        },
        params: params,
      });

      return response.data;
    },
  });
};

export default useAppMutation;
