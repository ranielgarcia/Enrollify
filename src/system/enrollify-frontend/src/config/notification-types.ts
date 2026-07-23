/**
 * Canonical notification type string constants.
 *
 * These mirror `NotificationTypeConstants` in the backend
 * (Enrollify.Application/Features/Notifications/NotificationTypeConstants.cs).
 *
 * Convention: add a new entry here whenever a new notification type is
 * introduced on the backend. The value must match the backend string exactly.
 * These constants are the source of truth used by `notification-query-sync-map.ts`
 * to map notification events to TanStack Query invalidations.
 */
export const NotificationTypes = {
  // ── Class Sections ──────────────────────────────────────────────────────────
  BulkInitializeClassSectionsForAcademicYear:
    "BulkInitializeClassSectionsForAcademicYear",
  CreateClassSection: "CreateClassSection",
  BulkAssignClassSectionAdviser: "BulkAssignClassSectionAdviser",
  ComputeAndGetValidationIssuesForClassSection:
    "ComputeAndGetValidationIssuesForClassSection",

  // ── Courses / Curricula ─────────────────────────────────────────────────────
  CourseCreatedEvent: "CourseCreatedEvent",
  SyncCourseCurriculumAssignmentsForAcademicYear:
    "SyncCourseCurriculumAssignmentsForAcademicYear",
} as const;

export type NotificationType =
  (typeof NotificationTypes)[keyof typeof NotificationTypes];
