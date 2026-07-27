import { useEffect, useRef } from "react";
import { useMsal } from "@azure/msal-react";
import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
  type ILogger,
  type HubConnection,
} from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { getCurrentAccessToken } from "@/infrastructure/authentication/tokenFetcher";
import { Config } from "@/infrastructure/configurations/app-config";
import { syncQueriesForClientDataInvalidation } from "./use-client-data-invalidation-sync";

const getClientDataInvalidationHubUrl = () =>
  `${Config.API_URL.replace(/\/api\/?$/, "")}/hubs/client-data-invalidation`;

export function useClientDataInvalidationHub() {
  const { instance } = useMsal();
  const queryClient = useQueryClient();
  const connectionRef = useRef<HubConnection | null>(null);

  useEffect(() => {
    // Track whether this effect instance has been cleaned up. In React Strict
    // Mode (dev) the cleanup runs immediately after the first mount while
    // start() is still awaiting the /negotiate response. Without this flag that
    // race would log a noisy false-positive "connection was stopped during
    // negotiation" error on every page load.
    let cancelled = false;

    // SignalR logs the start() rejection at Error level through its own internal
    // logger, which the level-based `configureLogging` cannot suppress. When
    // Strict Mode aborts the first connection mid-negotiate, that surfaces as a
    // "stopped during negotiation" (and a follow-up 1006) error even though it's
    // a benign, expected dev-only teardown. Downgrade only that case; every
    // other warning/error still reaches the console.
    const logger: ILogger = {
      log(logLevel, message) {
        if (
          cancelled &&
          (message.includes("stopped during negotiation") ||
            message.includes("1006"))
        ) {
          return;
        }
        if (logLevel >= LogLevel.Warning) {
          const line = `[SignalR] ${message}`;
          if (logLevel >= LogLevel.Error) {
            console.error(line);
          } else {
            console.warn(line);
          }
        }
      },
    };

    const connection = new HubConnectionBuilder()
      .withUrl(getClientDataInvalidationHubUrl(), {
        accessTokenFactory: async () => {
          const token = await getCurrentAccessToken({ msalInstance: instance });
          return token ?? "";
        },
      })
      .withAutomaticReconnect()
      .configureLogging(logger)
      .build();

    connection.on(
      "ReceiveClientDataInvalidation",
      (clientDataInvalidationType: string) => {
        syncQueriesForClientDataInvalidation(
          clientDataInvalidationType,
          queryClient,
        );
      },
    );

    connection.onreconnected(() => {
      console.info("Client data invalidation hub reconnected.");
    });

    connection.onclose((error) => {
      if (error) {
        console.error(
          "Client data invalidation hub connection closed with an error:",
          error,
        );
      }
    });

    // Assign before start() so a fast cleanup can stop it properly.
    connectionRef.current = connection;

    connection.start().catch((error) => {
      if (!cancelled) {
        console.error(
          "Failed to start client data invalidation hub connection:",
          error,
        );
      }
    });

    return () => {
      cancelled = true;
      connectionRef.current = null;
      if (connection.state !== HubConnectionState.Disconnected) {
        connection.stop().catch(() => undefined);
      }
    };
  }, [instance, queryClient]);
}
