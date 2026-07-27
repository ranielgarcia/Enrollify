import type { QueryKey } from "@tanstack/react-query";
import type { NotificationType } from "@/config/notification-types";
import { NotificationTypes } from "@/config/notification-types";
import { queryKeys as classSectionKeys } from "@/api/collections/class-section-collection";
import { queryKeys as courseKeys } from "@/api/collections/course-collection";

/**
 * Maps each notification `type` string to an array of TanStack Query keys
 * that should be invalidated when that notification is received over SignalR.
 *
 * --- How to add a new entry ---
 * 1. Add the backend type string to `NotificationTypeConstants.cs`
 * 2. Mirror it in `notification-types.ts` (`NotificationTypes`)
 * 3. Add an entry here: `NotificationTypes.YourNewType: [relevantKeys.base()]`
 *
 * Use `queryKeys.base()` for a broad invalidation of all queries in a domain,
 * or a more specific key (e.g., `queryKeys.detail(id)`) when you know the
 * exact resource that changed.
 */
export const notificationQuerySyncMap: Partial<
  Record<NotificationType, QueryKey[]>
> = {
  // ── Class Sections ──────────────────────────────────────────────────────────
  [NotificationTypes.BulkInitializeClassSectionsForAcademicYear]: [
    classSectionKeys.base(),
  ],
  [NotificationTypes.CreateClassSection]: [classSectionKeys.base()],
  // [NotificationTypes.BulkAssignClassSectionAdviser]: [classSectionKeys.base()],
  [NotificationTypes.ComputeAndGetValidationIssuesForClassSection]: [
    classSectionKeys.base(),
  ],

  // ── Courses ─────────────────────────────────────────────────────────────────
  [NotificationTypes.CourseCreatedEvent]: [courseKeys.all()],
};
