import { queryOptions, mutationOptions } from "@tanstack/react-query";
import { toast } from "sonner";
import { sub } from "date-fns";
import type { PagedResult } from "@/api/models/paged-result";
import {
  NotificationCategory,
  NotificationReferenceType,
  NotificationSeverity,
  type Notification,
  type NotificationCategory as NotificationCategoryType,
  type NotificationReadState,
  type NotificationSeverity as NotificationSeverityType,
} from "@/api/models/notification";

/**
 * ---------------------------------------------------------------------------
 * MOCK DATA LAYER — Notifications
 * ---------------------------------------------------------------------------
 * The backend currently only has the Notification domain model + fan-out
 * publisher (Enrollify.Core/Aggregates/NotificationAggregate). The
 * query/command layer (GetUserNotifications, GetUnreadNotificationCount,
 * MarkNotificationAsRead, DismissNotification) and the matching WebAPI
 * endpoints do not exist yet — see docs/plans/system-notifications-plan.md
 * (Phase 3 & 5).
 *
 * TODO(notifications-backend): once those endpoints ship —
 *   1. Regenerate API types (`npm run generate:api:win`).
 *   2. Replace the `queryFn`/`mutationFn` bodies below with
 *      `createAppQueryOptions` / `createMutationOptions` calls against the
 *      real paths (see subject-collection.ts for the pattern).
 *   3. Remove `mockNotificationsStore` and the `simulateNetwork` helper.
 *   4. Query keys, param shapes, and the `Notification` model are already
 *      designed to match the planned DTOs, so call sites (drawer, page,
 *      bell trigger) should not need to change.
 * ---------------------------------------------------------------------------
 */

export interface NotificationListParams {
  page?: number;
  perPage?: number;
  category?: NotificationCategoryType | "all";
  severity?: NotificationSeverityType | "all";
  readState?: NotificationReadState;
  search?: string;
}

export const notificationQueryKeys = {
  all: ["notifications"] as const,
  list: (params: NotificationListParams) =>
    ["notifications", "list", params] as const,
  unreadCount: () => ["notifications", "unread-count"] as const,
};

const simulateNetwork = (ms = 300) =>
  new Promise((resolve) => setTimeout(resolve, ms));

const now = new Date();

let mockNotificationsStore: Notification[] = [
  {
    id: 1,
    type: "CurriculumPublished",
    title: "Curriculum published",
    message:
      "BSCS 2024 Curriculum has been published and is now available for enrollment planning.",
    severity: NotificationSeverity.Success,
    category: NotificationCategory.Academic,
    referenceType: NotificationReferenceType.Curriculum,
    referenceId: 12,
    isRead: false,
    isDismissed: false,
    createdAt: sub(now, { minutes: 6 }).toISOString(),
    readAt: null,
    dismissedAt: null,
    createdBy: { firstName: "System", lastName: "" },
  },
  {
    id: 2,
    type: "BulkSectionInitializationCompleted",
    title: "Section initialization completed",
    message:
      "Bulk class section initialization for Academic Year 2026-2027 finished successfully. 48 sections were created.",
    severity: NotificationSeverity.Success,
    category: NotificationCategory.Enrollment,
    referenceType: NotificationReferenceType.Scheduling,
    referenceId: 48,
    isRead: false,
    isDismissed: false,
    createdAt: sub(now, { minutes: 22 }).toISOString(),
    readAt: null,
    dismissedAt: null,
    createdBy: { firstName: "System", lastName: "" },
  },
  {
    id: 3,
    type: "ScheduleConflictDetected",
    title: "Schedule conflict detected",
    message:
      "3 class sections have room conflicts for Monday 8:00 AM. Review the scheduling board to resolve them.",
    severity: NotificationSeverity.Error,
    category: NotificationCategory.Enrollment,
    referenceType: NotificationReferenceType.Scheduling,
    referenceId: null,
    isRead: false,
    isDismissed: false,
    createdAt: sub(now, { hours: 1 }).toISOString(),
    readAt: null,
    dismissedAt: null,
    createdBy: { firstName: "System", lastName: "" },
  },
  {
    id: 4,
    type: "EnrollmentPeriodClosingSoon",
    title: "Enrollment period closing soon",
    message:
      "The enrollment window for Academic Year 2026-2027 closes in 3 days. Ensure all students have finalized their schedules.",
    severity: NotificationSeverity.Warning,
    category: NotificationCategory.Enrollment,
    referenceType: null,
    referenceId: null,
    isRead: false,
    isDismissed: false,
    createdAt: sub(now, { hours: 3 }).toISOString(),
    readAt: null,
    dismissedAt: null,
    createdBy: { firstName: "System", lastName: "" },
  },
  {
    id: 5,
    type: "SubjectPrerequisiteUpdated",
    title: "Subject prerequisite updated",
    message:
      "CS301 - Data Structures now requires CS101 as a prerequisite. Existing curricula referencing this subject were re-validated.",
    severity: NotificationSeverity.Info,
    category: NotificationCategory.Academic,
    referenceType: NotificationReferenceType.Curriculum,
    referenceId: 7,
    isRead: true,
    isDismissed: false,
    createdAt: sub(now, { hours: 5 }).toISOString(),
    readAt: sub(now, { hours: 4 }).toISOString(),
    dismissedAt: null,
    createdBy: { firstName: "Maria", lastName: "Santos" },
  },
  {
    id: 6,
    type: "RoomCapacityExceeded",
    title: "Room capacity exceeded",
    message:
      "Section BSCS-3A exceeds the assigned room's capacity by 4 students. Consider reassigning to a larger room.",
    severity: NotificationSeverity.Warning,
    category: NotificationCategory.Enrollment,
    referenceType: NotificationReferenceType.Scheduling,
    referenceId: 3,
    isRead: true,
    isDismissed: false,
    createdAt: sub(now, { hours: 7 }).toISOString(),
    readAt: sub(now, { hours: 6 }).toISOString(),
    dismissedAt: null,
    createdBy: { firstName: "System", lastName: "" },
  },
  {
    id: 7,
    type: "NewBuildingAdded",
    title: "New building added",
    message:
      "\u201cSt. Michael Hall\u201d was added with 5 floors and 22 rooms.",
    severity: NotificationSeverity.Info,
    category: NotificationCategory.Admin,
    referenceType: null,
    referenceId: null,
    isRead: true,
    isDismissed: false,
    createdAt: sub(now, { hours: 10 }).toISOString(),
    readAt: sub(now, { hours: 9 }).toISOString(),
    dismissedAt: null,
    createdBy: { firstName: "Juan", lastName: "Dela Cruz" },
  },
  {
    id: 8,
    type: "TranscriptExportFailed",
    title: "Transcript export failed",
    message:
      "The batch transcript export job for College of Engineering failed after 2 retries. Please try again or contact support.",
    severity: NotificationSeverity.Error,
    category: NotificationCategory.System,
    referenceType: null,
    referenceId: null,
    isRead: false,
    isDismissed: false,
    createdAt: sub(now, { hours: 14 }).toISOString(),
    readAt: null,
    dismissedAt: null,
    createdBy: { firstName: "System", lastName: "" },
  },
  {
    id: 9,
    type: "RolePermissionsUpdated",
    title: "Role permissions updated",
    message:
      "The \u201cRegistrar\u201d role was granted permission to manage room types.",
    severity: NotificationSeverity.Info,
    category: NotificationCategory.Admin,
    referenceType: null,
    referenceId: null,
    isRead: true,
    isDismissed: false,
    createdAt: sub(now, { days: 1, hours: 2 }).toISOString(),
    readAt: sub(now, { days: 1, hours: 1 }).toISOString(),
    dismissedAt: null,
    createdBy: { firstName: "Admin", lastName: "User" },
  },
  {
    id: 10,
    type: "CurriculumEquivalenceGroupCreated",
    title: "Equivalence group created",
    message:
      "A new subject equivalence group \u201cGeneral Education Electives\u201d was created with 6 subjects.",
    severity: NotificationSeverity.Success,
    category: NotificationCategory.Academic,
    referenceType: NotificationReferenceType.Curriculum,
    referenceId: 4,
    isRead: true,
    isDismissed: false,
    createdAt: sub(now, { days: 1, hours: 6 }).toISOString(),
    readAt: sub(now, { days: 1, hours: 5 }).toISOString(),
    dismissedAt: null,
    createdBy: { firstName: "Maria", lastName: "Santos" },
  },
  {
    id: 11,
    type: "DataRetentionPurgeCompleted",
    title: "Data retention purge completed",
    message:
      "Nightly cleanup job purged 128 expired notifications and 4 soft-deleted records past their retention window.",
    severity: NotificationSeverity.Info,
    category: NotificationCategory.Audit,
    referenceType: null,
    referenceId: null,
    isRead: true,
    isDismissed: false,
    createdAt: sub(now, { days: 2 }).toISOString(),
    readAt: sub(now, { days: 1, hours: 20 }).toISOString(),
    dismissedAt: null,
    createdBy: { firstName: "System", lastName: "" },
  },
  {
    id: 12,
    type: "TeacherAssignmentConflict",
    title: "Teacher assignment conflict",
    message:
      "Prof. Reyes is assigned to two overlapping sections on Tuesday 1:00 PM. Please reassign one of them.",
    severity: NotificationSeverity.Error,
    category: NotificationCategory.Enrollment,
    referenceType: NotificationReferenceType.Scheduling,
    referenceId: null,
    isRead: true,
    isDismissed: false,
    createdAt: sub(now, { days: 2, hours: 5 }).toISOString(),
    readAt: sub(now, { days: 2, hours: 4 }).toISOString(),
    dismissedAt: null,
    createdBy: { firstName: "System", lastName: "" },
  },
  {
    id: 13,
    type: "AcademicYearActivated",
    title: "Academic year activated",
    message: "Academic Year 2026-2027 is now the active enrollment year.",
    severity: NotificationSeverity.Success,
    category: NotificationCategory.Academic,
    referenceType: null,
    referenceId: null,
    isRead: true,
    isDismissed: true,
    createdAt: sub(now, { days: 3 }).toISOString(),
    readAt: sub(now, { days: 3 }).toISOString(),
    dismissedAt: sub(now, { days: 2, hours: 22 }).toISOString(),
    createdBy: { firstName: "Admin", lastName: "User" },
  },
  {
    id: 14,
    type: "CourseCreated",
    title: "New course created",
    message:
      "\u201cBachelor of Science in Information Technology\u201d was added under College of Computing Studies.",
    severity: NotificationSeverity.Success,
    category: NotificationCategory.Academic,
    referenceType: null,
    referenceId: null,
    isRead: true,
    isDismissed: false,
    createdAt: sub(now, { days: 4 }).toISOString(),
    readAt: sub(now, { days: 4 }).toISOString(),
    dismissedAt: null,
    createdBy: { firstName: "Juan", lastName: "Dela Cruz" },
  },
  {
    id: 15,
    type: "SystemMaintenanceScheduled",
    title: "Scheduled maintenance this weekend",
    message:
      "Enrollify will be unavailable Saturday 11:00 PM - Sunday 2:00 AM for scheduled database maintenance.",
    severity: NotificationSeverity.Warning,
    category: NotificationCategory.System,
    referenceType: null,
    referenceId: null,
    isRead: false,
    isDismissed: false,
    createdAt: sub(now, { days: 5 }).toISOString(),
    readAt: null,
    dismissedAt: null,
    createdBy: { firstName: "System", lastName: "" },
  },
  {
    id: 16,
    type: "SubjectEquivalenceRemoved",
    title: "Equivalence link removed",
    message:
      "\u201cCS101\u201d was removed from the \u201cIntro Programming Equivalents\u201d group.",
    severity: NotificationSeverity.Info,
    category: NotificationCategory.Academic,
    referenceType: NotificationReferenceType.Curriculum,
    referenceId: 4,
    isRead: true,
    isDismissed: true,
    createdAt: sub(now, { days: 6 }).toISOString(),
    readAt: sub(now, { days: 6 }).toISOString(),
    dismissedAt: sub(now, { days: 5, hours: 12 }).toISOString(),
    createdBy: { firstName: "Maria", lastName: "Santos" },
  },
];

function filterNotifications(params: NotificationListParams): Notification[] {
  const {
    category = "all",
    severity = "all",
    readState = "all",
    search = "",
  } = params;

  const query = search.trim().toLowerCase();

  return mockNotificationsStore
    .filter((n) => !n.isDismissed)
    .filter((n) => category === "all" || n.category === category)
    .filter((n) => severity === "all" || n.severity === severity)
    .filter((n) => {
      if (readState === "unread") return !n.isRead;
      if (readState === "read") return n.isRead;
      return true;
    })
    .filter((n) => {
      if (!query) return true;
      return (
        n.title.toLowerCase().includes(query) ||
        n.message.toLowerCase().includes(query)
      );
    })
    .sort(
      (a, b) =>
        new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
    );
}

/** Paged, filterable list of the current user's notifications (dismissed items are always excluded). */
export const getNotificationsOptions = (
  params: NotificationListParams = {},
) => {
  const { page = 1, perPage = 10 } = params;

  return queryOptions({
    queryKey: notificationQueryKeys.list(params),
    queryFn: async (): Promise<PagedResult<Notification>> => {
      await simulateNetwork();

      const filtered = filterNotifications(params);
      const totalCount = filtered.length;
      const totalPages = Math.max(1, Math.ceil(totalCount / perPage));
      const start = (page - 1) * perPage;
      const items = filtered.slice(start, start + perPage);

      return { items, page, pageSize: perPage, totalCount, totalPages };
    },
    // Interim live-update mechanism until the SignalR hub is wired (see
    // hooks/use-notification-hub.ts).
    refetchInterval: 30_000,
  });
};

/** Unread notification count, used for the bell badge. */
export const getUnreadCountOptions = () =>
  queryOptions({
    queryKey: notificationQueryKeys.unreadCount(),
    queryFn: async (): Promise<number> => {
      await simulateNetwork(150);
      return mockNotificationsStore.filter((n) => !n.isDismissed && !n.isRead)
        .length;
    },
    refetchInterval: 30_000,
  });

export const markNotificationAsReadOptions = (id: number) =>
  mutationOptions({
    mutationKey: ["notifications", "mark-read", id],
    mutationFn: async () => {
      await simulateNetwork(150);
      const notification = mockNotificationsStore.find((n) => n.id === id);
      if (notification && !notification.isRead) {
        notification.isRead = true;
        notification.readAt = new Date().toISOString();
      }
      return notification;
    },
    meta: { invalidateQueries: [notificationQueryKeys.all] },
  });

export const markNotificationAsUnreadOptions = (id: number) =>
  mutationOptions({
    mutationKey: ["notifications", "mark-unread", id],
    mutationFn: async () => {
      await simulateNetwork(150);
      const notification = mockNotificationsStore.find((n) => n.id === id);
      if (notification) {
        notification.isRead = false;
        notification.readAt = null;
      }
      return notification;
    },
    meta: { invalidateQueries: [notificationQueryKeys.all] },
  });

export const dismissNotificationOptions = (id: number) =>
  mutationOptions({
    mutationKey: ["notifications", "dismiss", id],
    mutationFn: async () => {
      await simulateNetwork(150);
      const notification = mockNotificationsStore.find((n) => n.id === id);
      if (notification) {
        notification.isDismissed = true;
        notification.dismissedAt = new Date().toISOString();
      }
      return notification;
    },
    meta: { invalidateQueries: [notificationQueryKeys.all] },
  });

export const markAllNotificationsAsReadOptions = () =>
  mutationOptions({
    mutationKey: ["notifications", "mark-all-read"],
    mutationFn: async () => {
      await simulateNetwork(250);
      mockNotificationsStore = mockNotificationsStore.map((n) =>
        n.isDismissed || n.isRead
          ? n
          : { ...n, isRead: true, readAt: new Date().toISOString() },
      );
    },
    meta: { invalidateQueries: [notificationQueryKeys.all] },
    onSuccess: () => {
      toast.success("All notifications marked as read");
    },
  });

export const dismissAllNotificationsOptions = () =>
  mutationOptions({
    mutationKey: ["notifications", "dismiss-all"],
    mutationFn: async () => {
      await simulateNetwork(250);
      mockNotificationsStore = mockNotificationsStore.map((n) =>
        n.isDismissed
          ? n
          : { ...n, isDismissed: true, dismissedAt: new Date().toISOString() },
      );
    },
    meta: { invalidateQueries: [notificationQueryKeys.all] },
    onSuccess: () => {
      toast.success("All notifications cleared");
    },
  });
