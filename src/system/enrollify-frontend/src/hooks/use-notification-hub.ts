import { useEffect, useRef } from "react";
import { useMsal } from "@azure/msal-react";
import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
  type HubConnection,
} from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { getCurrentAccessToken } from "@/infrastructure/authentication/tokenFetcher";
import { Config } from "@/infrastructure/configurations/app-config";
import { notificationQueryKeys } from "@/api/collections/notifications-collection";

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

    const connection = new HubConnectionBuilder()
      .withUrl(getNotificationHubUrl(), {
        accessTokenFactory: async () => {
          const token = await getCurrentAccessToken({ msalInstance: instance });
          return token ?? "";
        },
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on("ReceiveNotification", () => {
      queryClient.invalidateQueries({ queryKey: notificationQueryKeys.all });
    });

    connection.onreconnected(() => {
      queryClient.invalidateQueries({ queryKey: notificationQueryKeys.all });
    });

    connection.onclose((error) => {
      if (error) {
        console.error(
          "Notification hub connection closed with an error:",
          error,
        );
      }
    });

    connection
      .start()
      .catch((error) =>
        console.error("Failed to start notification hub connection:", error),
      );

    connectionRef.current = connection;

    return () => {
      if (connection.state !== HubConnectionState.Disconnected) {
        connection.stop().catch(() => undefined);
      }
      connectionRef.current = null;
    };
  }, [instance, queryClient]);
}
