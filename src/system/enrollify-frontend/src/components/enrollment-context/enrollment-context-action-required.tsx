import {
  BellIcon,
  CircleXIcon,
  InfoIcon,
  TriangleAlertIcon,
} from "lucide-react";
import type React from "react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

// ─── Types ────────────────────────────────────────────────────────────────────

export type AppBannerVariant = "warning" | "destructive" | "info" | "default";

export interface AppBannerItem {
  id: string;
  variant?: AppBannerVariant;
  /** Override the default variant icon. */
  icon?: React.ReactNode;
  title: string;
  description?: string;
  action?: {
    label: string;
    onClick: () => void;
  };
}

// ─── Variant config ───────────────────────────────────────────────────────────

const variantConfig: Record<
  AppBannerVariant,
  {
    containerClass: string;
    iconClass: string;
    DefaultIcon: React.ComponentType<{ className?: string }>;
  }
> = {
  warning: {
    containerClass:
      "bg-amber-50 border-amber-200 dark:bg-amber-950/20 dark:border-amber-800",
    iconClass: "text-amber-600 dark:text-amber-400",
    DefaultIcon: TriangleAlertIcon,
  },
  destructive: {
    containerClass:
      "bg-red-50 border-red-200 dark:bg-red-950/20 dark:border-red-800",
    iconClass: "text-red-600 dark:text-red-400",
    DefaultIcon: CircleXIcon,
  },
  info: {
    containerClass:
      "bg-blue-50 border-blue-200 dark:bg-blue-950/20 dark:border-blue-800",
    iconClass: "text-blue-600 dark:text-blue-400",
    DefaultIcon: InfoIcon,
  },
  default: {
    containerClass: "bg-muted border-border",
    iconClass: "text-muted-foreground",
    DefaultIcon: BellIcon,
  },
};

// ─── Component ────────────────────────────────────────────────────────────────

interface Props {
  items?: AppBannerItem[];
}

export default function EnrollmentContextActionRequired({ items = [] }: Props) {
  if (items.length === 0) return null;

  return (
    <div className="sticky top-0 z-10 flex flex-col">
      {items.map((item) => {
        const variant = item.variant ?? "default";
        const { containerClass, iconClass, DefaultIcon } =
          variantConfig[variant];

        return (
          <div
            key={item.id}
            className={cn(
              "flex items-center gap-3 border-b px-4 py-2.5 lg:px-6",
              containerClass,
            )}
          >
            <div className={cn("shrink-0", iconClass)}>
              {item.icon ?? <DefaultIcon className="size-4" />}
            </div>

            <div className="flex min-w-0 flex-1 flex-col">
              <span className="text-sm font-medium leading-snug">
                {item.title}
              </span>
              {item.description && (
                <span className="text-muted-foreground mt-0.5 text-xs">
                  {item.description}
                </span>
              )}
            </div>

            {item.action && (
              <Button
                size="sm"
                variant="outline"
                className="shrink-0"
                onClick={item.action.onClick}
              >
                {item.action.label}
              </Button>
            )}
          </div>
        );
      })}
    </div>
  );
}
