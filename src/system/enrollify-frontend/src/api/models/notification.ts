import { z } from "zod";
import { BasicUserInfoSchema } from "@/api/models/basic-user-info";

/**
 * Mirrors `NotificationSeverityEnum` (Enrollify.Core/Constants/NotificationSeverityEnum.cs).
 */
export const NotificationSeverity = {
  Info: "Info",
  Warning: "Warning",
  Error: "Error",
  Success: "Success",
} as const;
export type NotificationSeverity =
  (typeof NotificationSeverity)[keyof typeof NotificationSeverity];

/**
 * Mirrors `NotificationCategoryEnum` (Enrollify.Core/Constants/NotificationCategoryEnum.cs).
 */
export const NotificationCategory = {
  System: "System",
  Academic: "Academic",
  Enrollment: "Enrollment",
  Admin: "Admin",
  Audit: "Audit",
} as const;
export type NotificationCategory =
  (typeof NotificationCategory)[keyof typeof NotificationCategory];

/**
 * Mirrors `NotificationReferenceTypeEnum` (Enrollify.Core/Constants/NotificationReferenceTypeEnum.cs).
 */
export const NotificationReferenceType = {
  Curriculum: "Curriculum",
  Scheduling: "Scheduling",
} as const;
export type NotificationReferenceType =
  (typeof NotificationReferenceType)[keyof typeof NotificationReferenceType];

export const NotificationReadState = {
  All: "all",
  Unread: "unread",
  Read: "read",
} as const;
export type NotificationReadState =
  (typeof NotificationReadState)[keyof typeof NotificationReadState];

/**
 * Shape mirrors the eventual `GET /notifications` DTO (see
 * docs/plans/system-notifications-plan.md). Backed by a mock data source
 * until the WebAPI endpoints exist — see notifications-collection.ts.
 */
export const NotificationSchema = z.object({
  id: z.number(),
  type: z.string(),
  title: z.string(),
  message: z.string(),
  severity: z.enum([
    NotificationSeverity.Info,
    NotificationSeverity.Warning,
    NotificationSeverity.Error,
    NotificationSeverity.Success,
  ]),
  category: z.enum([
    NotificationCategory.System,
    NotificationCategory.Academic,
    NotificationCategory.Enrollment,
    NotificationCategory.Admin,
    NotificationCategory.Audit,
  ]),
  referenceType: z
    .enum([
      NotificationReferenceType.Curriculum,
      NotificationReferenceType.Scheduling,
    ])
    .nullish(),
  referenceId: z.number().nullish(),
  isRead: z.boolean(),
  isDismissed: z.boolean(),
  createdAt: z.string(),
  readAt: z.string().nullish(),
  dismissedAt: z.string().nullish(),
  createdBy: BasicUserInfoSchema.nullish(),
});

export type Notification = z.infer<typeof NotificationSchema>;
