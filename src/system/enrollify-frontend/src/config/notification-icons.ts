import type { LucideIcon } from "lucide-react";
import {
  CircleCheckIcon,
  InfoIcon,
  TriangleAlertIcon,
  OctagonXIcon,
  GraduationCap,
  Scroll,
  CalendarClock,
  Megaphone,
  ShieldCheck,
  Settings2,
} from "lucide-react";
import {
  NotificationCategory,
  NotificationReferenceType,
  NotificationSeverity,
  type NotificationCategory as NotificationCategoryType,
  type NotificationReferenceType as NotificationReferenceTypeType,
  type NotificationSeverity as NotificationSeverityType,
} from "@/api/models/notification";

/**
 * Severity -> icon + color classes. Mirrors the icon choices already used
 * for toasts in components/ui/sonner.tsx so severity reads consistently
 * across the app (toast vs. persistent notification feed).
 */
export const NotificationSeverityIcons: Record<
  NotificationSeverityType,
  { icon: LucideIcon; iconClassName: string; badgeClassName: string }
> = {
  [NotificationSeverity.Success]: {
    icon: CircleCheckIcon,
    iconClassName: "text-green-600 dark:text-green-500",
    badgeClassName:
      "bg-green-500/10 text-green-700 dark:text-green-400 border-green-500/20",
  },
  [NotificationSeverity.Info]: {
    icon: InfoIcon,
    iconClassName: "text-blue-600 dark:text-blue-500",
    badgeClassName:
      "bg-blue-500/10 text-blue-700 dark:text-blue-400 border-blue-500/20",
  },
  [NotificationSeverity.Warning]: {
    icon: TriangleAlertIcon,
    iconClassName: "text-yellow-600 dark:text-yellow-500",
    badgeClassName:
      "bg-yellow-500/10 text-yellow-700 dark:text-yellow-400 border-yellow-500/20",
  },
  [NotificationSeverity.Error]: {
    icon: OctagonXIcon,
    iconClassName: "text-destructive",
    badgeClassName: "bg-destructive/10 text-destructive border-destructive/20",
  },
};

/**
 * Category -> icon, shown as a small badge/chip on each notification card.
 */
export const NotificationCategoryIcons: Record<
  NotificationCategoryType,
  LucideIcon
> = {
  [NotificationCategory.Academic]: GraduationCap,
  [NotificationCategory.Enrollment]: Scroll,
  [NotificationCategory.System]: Settings2,
  [NotificationCategory.Admin]: Megaphone,
  [NotificationCategory.Audit]: ShieldCheck,
};

/**
 * ReferenceType -> icon + route builder, used to render the "View" action
 * that deep-links to the entity the notification is about.
 */
export const NotificationReferenceTypeConfig: Record<
  NotificationReferenceTypeType,
  { icon: LucideIcon; buildHref: (referenceId: number) => string }
> = {
  [NotificationReferenceType.Curriculum]: {
    icon: Scroll,
    buildHref: (referenceId) =>
      `/portal/curriculum-and-scheduling/curriculum/${referenceId}`,
  },
  [NotificationReferenceType.Scheduling]: {
    icon: CalendarClock,
    buildHref: () => `/portal/curriculum-and-scheduling/scheduling/sections`,
  },
};
