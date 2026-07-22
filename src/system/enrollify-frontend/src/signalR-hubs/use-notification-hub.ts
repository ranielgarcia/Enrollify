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
import { queryKeys } from "@/api/collections/notifications-collection";
import { NotificationSchema } from "@/api/models/notification";
import { toast } from "sonner";

const getNotificationHubUrl = () =>
  `${Config.API_URL.replace(/\/api\/?$/, "")}/hubs/notifications`;

/**
 * Establishes a SignalR connection to the backend notifications hub so new
 * notifications can be pushed in real time instead of relying purely on the
 * 30s polling interval used by the notifications query layer.
 *
 * Inert (no-op) until:
 *   1. `Config.ENABLE_NOTIFICATION_HUB` is `true`
 *      (set `VITE_ENABLE_NOTIFICATION_HUB=true`).
 *   2. The backend exposes `/hubs/notifications`
 *      (see docs/plans/system-notifications-plan.md, Phase 5).
 *
 * TODO(notifications-backend): once the hub exists, consider merging the
 * pushed notification payload directly into the query cache instead of a
 * blanket invalidate, and remove this comment.
 */
export function useNotificationHub() {
  const { instance } = useMsal();
  const queryClient = useQueryClient();
  const connectionRef = useRef<HubConnection | null>(null);

  useEffect(() => {
    if (!Config.ENABLE_NOTIFICATION_HUB) {
      return;
    }

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
      .withUrl(getNotificationHubUrl(), {
        accessTokenFactory: async () => {
          const token = await getCurrentAccessToken({ msalInstance: instance });
          return token ?? "";
        },
      })
      .withAutomaticReconnect()
      .configureLogging(logger)
      .build();

    connection.on("ReceiveNotification", (notification) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.base() });
      const parsedNotification = NotificationSchema.parse(notification);

      if (parsedNotification.severity === "Info") {
        toast.info(parsedNotification.title, {
          description: parsedNotification.message,
        });
      } else if (parsedNotification.severity === "Warning") {
        toast.warning(parsedNotification.title, {
          description: parsedNotification.message,
        });
      } else if (parsedNotification.severity === "Error") {
        toast.error(parsedNotification.title, {
          description: parsedNotification.message,
        });
      } else if (parsedNotification.severity === "Success") {
        toast.success(parsedNotification.title, {
          description: parsedNotification.message,
        });
      }
    });

    connection.onreconnected(() => {
      queryClient.invalidateQueries({ queryKey: queryKeys.base() });
    });

    connection.onclose((error) => {
      if (error) {
        console.error(
          "Notification hub connection closed with an error:",
          error,
        );
      }
    });

    // Assign before start() so a fast cleanup can stop it properly.
    connectionRef.current = connection;

    connection.start().catch((error) => {
      if (!cancelled) {
        console.error("Failed to start notification hub connection:", error);
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
