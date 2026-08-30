import {
  CalendarX2,
  ClipboardX,
  DatabaseZap,
  Gauge,
  ShieldAlert,
  SlidersHorizontal,
  Unplug,
  type LucideIcon,
} from "lucide-react";

import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";

type HealthSeverity = "error" | "warn" | "info" | "neutral";

interface HealthIndicatorConfig {
  key: string;
  label: string;
  description: string;
  icon: LucideIcon;
  severity: HealthSeverity;
}

const HEALTH_INDICATORS: HealthIndicatorConfig[] = [
  {
    key: "SCHEDULE_CONFLICTS",
    label: "Schedule Conflicts",
    description: "Hard schedule conflicts across offerings",
    icon: CalendarX2,
    severity: "error",
  },
  {
    key: "SCHEDULE_POLICY_VIOLATIONS",
    label: "Policy Violations",
    description: "Schedule policy rule violations",
    icon: ShieldAlert,
    severity: "warn",
  },
  {
    key: "CAPACITY_CONSTRAINTS",
    label: "Capacity Exceeded",
    description: "Offerings exceeding enrollment capacity",
    icon: Gauge,
    severity: "warn",
  },
  {
    key: "RESOURCE_MISALIGNMENTS",
    label: "Misaligned Resources",
    description: "Resource assignments mismatched to requirements",
    icon: Unplug,
    severity: "warn",
  },
  {
    key: "MISSING_REQUIREMENTS",
    label: "Missing Fields",
    description: "Required fields not yet filled in",
    icon: ClipboardX,
    severity: "info",
  },
  {
    key: "DATA_INCONSISTENCIES",
    label: "Data Issues",
    description: "Data integrity issues blocking processing",
    icon: DatabaseZap,
    severity: "error",
  },
  {
    key: "DEFAULT_VALUES",
    label: "Placeholder Values",
    description: "Offerings still using placeholder values",
    icon: SlidersHorizontal,
    severity: "neutral",
  },
];
const SEVERITY_STYLES: Record<
  HealthSeverity,
  { border: string; iconColor: string; badgeBg: string; badgeText: string }
> = {
  error: {
    border: "border-l-rose-400/50 dark:border-l-rose-500/40",
    iconColor: "text-rose-500 dark:text-rose-400",
    badgeBg: "bg-rose-100 dark:bg-rose-900/40",
    badgeText: "text-rose-800 dark:text-rose-200",
  },
  warn: {
    border: "border-l-amber-400/50 dark:border-l-amber-500/40",
    iconColor: "text-amber-500 dark:text-amber-400",
    badgeBg: "bg-amber-100 dark:bg-amber-900/40",
    badgeText: "text-amber-800 dark:text-amber-200",
  },
  info: {
    border: "border-l-blue-400/50 dark:border-l-blue-500/40",
    iconColor: "text-blue-500 dark:text-blue-400",
    badgeBg: "bg-blue-100 dark:bg-blue-900/40",
    badgeText: "text-blue-800 dark:text-blue-200",
  },
  neutral: {
    border: "border-l-slate-300/60 dark:border-l-slate-600/50",
    iconColor: "text-muted-foreground",
    badgeBg: "bg-slate-100 dark:bg-slate-800",
    badgeText: "text-slate-800 dark:text-slate-200",
  },
};

interface HealthIndicatorCardProps {
  config: HealthIndicatorConfig;
  count: number;
}

function HealthIndicatorCard({ config, count }: HealthIndicatorCardProps) {
  const Icon = config.icon;
  const isHealthy = count === 0;
  const styles = SEVERITY_STYLES[config.severity];

  return (
    <div
      className={cn(
        "flex items-center gap-3 rounded-lg border border-l-2 bg-card px-3 py-2.5 transition-colors",
        isHealthy ? "border-l-border" : styles.border,
      )}
      title={config.description}
    >
      {/* Icon */}
      <Icon
        className={cn(
          "size-3.5 shrink-0",
          isHealthy ? "text-muted-foreground/50" : styles.iconColor,
        )}
      />

      {/* Label */}
      <span
        className={cn(
          "flex-1 truncate text-xs leading-none",
          isHealthy ? "text-muted-foreground" : "text-foreground",
        )}
      >
        {config.label}
      </span>

      {/* Count / status */}
      {isHealthy ? (
        <span className="shrink-0 text-[10px] font-medium text-muted-foreground/60">
          OK
        </span>
      ) : (
        <span
          className={cn(
            "shrink-0 rounded px-1.5 py-0.5 text-[11px] font-semibold tabular-nums",
            styles.badgeBg,
            styles.badgeText,
          )}
        >
          {count.toLocaleString()}
        </span>
      )}
    </div>
  );
}

interface HealthSidebarProps {
  stats: Record<string, number>;
  isLoading?: boolean;
  className?: string;
}

export function HealthSidebar({
  stats,
  isLoading = false,
  className,
}: HealthSidebarProps) {
  return (
    <aside className={cn("flex flex-col gap-2", className)}>
      <div className="flex items-center gap-2 border-t border-border/60 px-0.5 pb-1 pt-3">
        <span className="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground">
          Health Checks
        </span>
      </div>

      {isLoading ? (
        <div className="flex flex-col gap-1.5">
          {Array.from({ length: 7 }).map((_, i) => (
            <Skeleton key={i} className="h-9 w-full rounded-lg" />
          ))}
        </div>
      ) : (
        <div className="flex flex-col gap-1.5">
          {HEALTH_INDICATORS.map((config) => (
            <HealthIndicatorCard
              key={config.key}
              config={config}
              count={stats[config.key] ?? 0}
            />
          ))}
        </div>
      )}
    </aside>
  );
}
