import createQueryOptions from "@/hooks/create-query-options";
import createMutationOptions from "@/hooks/create-mutation-options";
import {
  NotificationSchema,
  type Notification,
  type NotificationCategory as NotificationCategoryType,
  type NotificationSeverity as NotificationSeverityType,
  type NotificationReadState,
} from "../models/notification";
import { pagedResultSchema, type PagedResult } from "../models/paged-result";
import { toast } from "sonner";

export interface NotificationListParams {
  page?: number;
  pageSize?: number;
  category?: NotificationCategoryType | "all";
  severity?: NotificationSeverityType | "all";
  readState?: NotificationReadState;
  search?: string;
}

const queryKeys = {
  base: () => ["notifications"],
  filter: (params: NotificationListParams) => [
    ...queryKeys.base(),
    "filter",
    ...Object.values(params),
  ],
  unreadCount: () => [...queryKeys.base(), "unread-count"],
  markRead: (notificationId: number) => [
    ...queryKeys.base(),
    "mark-read",
    notificationId,
  ],
  markUnread: (notificationId: number) => [
    ...queryKeys.base(),
    "mark-unread",
    notificationId,
  ],
  dismiss: (notificationId: number) => [
    ...queryKeys.base(),
    "dismiss",
    notificationId,
  ],
  readAll: () => [...queryKeys.base(), "read-all"],
  dismissAll: () => [...queryKeys.base(), "dismiss-all"],
};

const pagedNotificationsSchema = pagedResultSchema(NotificationSchema);

export const filterNotificationsOptions = (params: NotificationListParams) =>
  createQueryOptions({
    path: "/api/notifications/filter/{page}/{pageSize}",
    pathParams: {
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 10,
    },
    params: {
      SearchTerm: params.search,
      Category: params.category,
      Severity: params.severity,
      ReadState: params.readState,
    },
    options: {
      queryKey: queryKeys.filter(params),
      staleTime: 1000 * 60 * 2,
      select: (pagedResults): PagedResult<Notification> => {
        // Handle empty response or string response
        if (
          !pagedResults ||
          (typeof pagedResults === "string" && pagedResults === "")
        ) {
          return {
            items: [],
            page: params.page ?? 1,
            pageSize: params.pageSize ?? 10,
            totalCount: 0,
            totalPages: 0,
          };
        }
        const data =
          typeof pagedResults === "string"
            ? JSON.parse(pagedResults)
            : pagedResults;
        return pagedNotificationsSchema.parse(data);
      },
    },
  });

export const getUnreadCountOptions = () =>
  createQueryOptions({
    path: "/api/notifications/unread-count",
    options: {
      queryKey: queryKeys.unreadCount(),
      staleTime: 1000 * 60 * 1,
      select: (count): number => {
        return typeof count === "number" ? count : 0;
      },
    },
  });

export const markNotificationAsReadOptions = (notificationId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/notifications/{id}/read",
    pathParams: {
      id: notificationId,
    },
    mutationKey: queryKeys.markRead(notificationId),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => {
        toast.success("Notification marked as read");
      },
    },
  });

export const markNotificationAsUnreadOptions = (notificationId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/notifications/{id}/unread",
    pathParams: {
      id: notificationId,
    },
    mutationKey: queryKeys.markUnread(notificationId),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => {
        toast.success("Notification marked as unread");
      },
    },
  });

export const dismissNotificationOptions = (notificationId: number) =>
  createMutationOptions({
    httpVerb: "put",
    path: "/api/notifications/{id}/dismiss",
    pathParams: {
      id: notificationId,
    },
    mutationKey: queryKeys.dismiss(notificationId),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => {
        toast.success("Notification dismissed");
      },
    },
  });

export const markAllNotificationsAsReadOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/notifications/read-all",
    mutationKey: queryKeys.readAll(),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => {
        toast.success("All notifications marked as read");
      },
    },
  });

export const dismissAllNotificationsOptions = () =>
  createMutationOptions({
    httpVerb: "post",
    path: "/api/notifications/dismiss-all",
    mutationKey: queryKeys.dismissAll(),
    options: {
      meta: { invalidateQueries: [queryKeys.base()] },
      onSuccess: () => {
        toast.success("All notifications cleared");
      },
    },
  });
