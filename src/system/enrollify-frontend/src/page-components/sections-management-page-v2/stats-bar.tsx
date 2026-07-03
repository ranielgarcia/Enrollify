import {
  CalendarX2,
  ClipboardX,
  DatabaseZap,
  FileText,
  FolderOpen,
  Gauge,
  ShieldAlert,
  SlidersHorizontal,
  Unplug,
  X,
  XCircle,
  type LucideIcon,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";

import type { QuickFilter } from "./searchParams";

const STATS_KEYS = {
  DraftSections: "DRAFT_SECTIONS",
  OpenSections: "OPEN_SECTIONS",
  CancelledSections: "CANCELLED_SECTIONS",
  CapacityConstraints: "CAPACITY_CONSTRAINTS",
  DataInconsistencies: "DATA_INCONSISTENCIES",
  DefaultValues: "DEFAULT_VALUES",
  MissingRequirements: "MISSING_REQUIREMENTS",
  ResourceMisalignments: "RESOURCE_MISALIGNMENTS",
  ScheduleConflicts: "SCHEDULE_CONFLICTS",
  SchedulePolicyViolations: "SCHEDULE_POLICY_VIOLATIONS",
} as const;

type Tone = "neutral" | "success" | "danger" | "warn" | "info";

interface PillConfig {
  key: string;
  label: string;
  icon: LucideIcon;
  filter: QuickFilter | null;
  tone: Tone;
  hint?: string;
}

const PIPELINE_PILLS: PillConfig[] = [
  {
    key: STATS_KEYS.DraftSections,
    label: "Draft",
    icon: FileText,
    filter: "draft",
    tone: "neutral",
    hint: "Sections not yet open for enrollment",
  },
  {
    key: STATS_KEYS.OpenSections,
    label: "Open",
    icon: FolderOpen,
    filter: "open",
    tone: "success",
    hint: "Sections open for enrollment",
  },
  {
    key: STATS_KEYS.CancelledSections,
    label: "Cancelled",
    icon: XCircle,
    filter: "cancelled",
    tone: "danger",
    hint: "Cancelled this term",
  },
];

const HEALTH_PILLS: PillConfig[] = [
  {
    key: STATS_KEYS.ScheduleConflicts,
    label: "Conflicts",
    icon: CalendarX2,
    filter: null,
    tone: "danger",
    hint: "Hard schedule conflicts across offerings",
  },
  {
    key: STATS_KEYS.SchedulePolicyViolations,
    label: "Policy",
    icon: ShieldAlert,
    filter: null,
    tone: "warn",
    hint: "Schedule policy rule violations",
  },
  {
    key: STATS_KEYS.CapacityConstraints,
    label: "Capacity",
    icon: Gauge,
    filter: null,
    tone: "warn",
    hint: "Offerings exceeding enrollment capacity",
  },
  {
    key: STATS_KEYS.ResourceMisalignments,
    label: "Misaligned",
    icon: Unplug,
    filter: null,
    tone: "warn",
    hint: "Resource assignments mismatched to requirements",
  },
  {
    key: STATS_KEYS.MissingRequirements,
    label: "Missing",
    icon: ClipboardX,
    filter: null,
    tone: "info",
    hint: "Required fields not yet filled in",
  },
  {
    key: STATS_KEYS.DataInconsistencies,
    label: "Data",
    icon: DatabaseZap,
    filter: null,
    tone: "danger",
    hint: "Data integrity issues blocking processing",
  },
  {
    key: STATS_KEYS.DefaultValues,
    label: "Defaults",
    icon: SlidersHorizontal,
    filter: null,
    tone: "neutral",
    hint: "Offerings still using placeholder values",
  },
];

const TONE_CLASSES: Record<
  Tone,
  {
    icon: string;
    iconActive: string;
    ring: string;
    accentInactive: string;
    accentActive: string;
    activeBg: string;
  }
> = {
  neutral: {
    icon: "text-muted-foreground bg-muted",
    iconActive: "text-foreground bg-secondary",
    ring: "ring-secondary",
    accentInactive: "bg-slate-300 dark:bg-slate-600",
    accentActive: "bg-slate-500 dark:bg-slate-400",
    activeBg: "",
  },
  success: {
    icon: "text-emerald-600 dark:text-emerald-400 bg-emerald-50 dark:bg-emerald-950/40",
    iconActive:
      "text-emerald-700 dark:text-emerald-300 bg-emerald-100 dark:bg-emerald-900/60",
    ring: "ring-emerald-300 dark:ring-emerald-700",
    accentInactive: "bg-emerald-300/70 dark:bg-emerald-700/70",
    accentActive: "bg-emerald-500 dark:bg-emerald-400",
    activeBg: "bg-emerald-50/50 dark:bg-emerald-950/20",
  },
  danger: {
    icon: "text-rose-600 dark:text-rose-400 bg-rose-50 dark:bg-rose-950/40",
    iconActive:
      "text-rose-700 dark:text-rose-300 bg-rose-100 dark:bg-rose-900/60",
    ring: "ring-rose-300 dark:ring-rose-700",
    accentInactive: "bg-rose-300/70 dark:bg-rose-700/70",
    accentActive: "bg-rose-500 dark:bg-rose-400",
    activeBg: "bg-rose-50/50 dark:bg-rose-950/20",
  },
  warn: {
    icon: "text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-950/40",
    iconActive:
      "text-amber-700 dark:text-amber-300 bg-amber-100 dark:bg-amber-900/60",
    ring: "ring-amber-300 dark:ring-amber-700",
    accentInactive: "bg-amber-300/70 dark:bg-amber-700/70",
    accentActive: "bg-amber-500 dark:bg-amber-400",
    activeBg: "bg-amber-50/50 dark:bg-amber-950/20",
  },
  info: {
    icon: "text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-950/40",
    iconActive:
      "text-blue-700 dark:text-blue-300 bg-blue-100 dark:bg-blue-900/60",
    ring: "ring-blue-300 dark:ring-blue-700",
    accentInactive: "bg-blue-300/70 dark:bg-blue-700/70",
    accentActive: "bg-blue-500 dark:bg-blue-400",
    activeBg: "bg-blue-50/50 dark:bg-blue-950/20",
  },
};

interface StatPillProps {
  config: PillConfig;
  value: number;
  isActive: boolean;
  onClick?: () => void;
  compact?: boolean;
}

function StatPill({
  config,
  value,
  isActive,
  onClick,
  compact = false,
}: StatPillProps) {
  const Icon = config.icon;
  const isClickable = config.filter !== null && !!onClick;
  const tone = TONE_CLASSES[config.tone];

  const pillClasses = cn(
    "flex items-center overflow-hidden rounded-lg border bg-card text-left transition-all",
    compact ? "min-w-[108px]" : "min-w-[140px]",
    isActive && "ring-1 ring-offset-0",
    isActive && tone.ring,
    isActive && tone.activeBg,
  );

  const pillInner = (
    <>
      {/* Tone accent stripe */}
      <div
        className={cn(
          "shrink-0 self-stretch",
          compact ? "w-[3px]" : "w-1",
          isActive ? tone.accentActive : tone.accentInactive,
        )}
      />
      {/* Content */}
      <div
        className={cn(
          "flex items-center gap-2.5",
          compact ? "px-2.5 py-2" : "px-3 py-2.5",
        )}
      >
        <div
          className={cn(
            "flex shrink-0 items-center justify-center rounded-md transition-colors",
            compact ? "size-7" : "size-9",
            isActive ? tone.iconActive : tone.icon,
          )}
        >
          <Icon className={cn(compact ? "size-3.5" : "size-4")} />
        </div>
        <div className="min-w-0">
          <div
            className={cn(
              "font-semibold uppercase leading-none text-muted-foreground",
              compact
                ? "text-[9px] tracking-wider"
                : "text-[10px] tracking-widest",
            )}
          >
            {config.label}
          </div>
          <div
            className={cn(
              "mt-0.5 font-bold leading-none tabular-nums",
              compact ? "text-sm" : "text-xl",
            )}
          >
            {value.toLocaleString()}
          </div>
        </div>
      </div>
    </>
  );

  if (isClickable) {
    return (
      <button
        type="button"
        onClick={onClick}
        aria-pressed={isActive ? "true" : "false"}
        title={config.hint}
        className={cn(
          pillClasses,
          "cursor-pointer hover:bg-muted/40",
          "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1",
        )}
      >
        {pillInner}
      </button>
    );
  }

  const pill = (
    <div className={cn(pillClasses, "cursor-default")}>{pillInner}</div>
  );

  if (config.hint) {
    return (
      <Tooltip>
        <TooltipTrigger asChild>{pill}</TooltipTrigger>
        <TooltipContent>{config.hint}</TooltipContent>
      </Tooltip>
    );
  }

  return pill;
}

interface SectionStatsStripProps {
  stats: Record<string, number>;
  activeFilter: QuickFilter;
  onFilterChange: (filter: QuickFilter) => void;
  isLoading?: boolean;
  className?: string;
}

const FILTER_LABELS: Record<Exclude<QuickFilter, "all">, string> = {
  draft: "Draft",
  open: "Open",
  cancelled: "Cancelled",
};

/**
 * Consolidated stats + filter strip for the sections page.
 * - Pipeline cluster (Draft / Open / Cancelled) — clickable status filters with
 *   a left accent stripe that brightens on active state.
 * - Health diagnostics cluster (7 issue categories) — compact display-only counters.
 * - Active-filter chip with one-click clear.
 */
export function SectionStatsStrip({
  stats,
  activeFilter,
  onFilterChange,
  isLoading = false,
  className,
}: SectionStatsStripProps) {
  const handlePipelineClick = (filter: QuickFilter | null) => {
    if (!filter) return;
    onFilterChange(activeFilter === filter ? "all" : filter);
  };

  return (
    <div
      className={cn(
        "rounded-xl border bg-card shadow-sm",
        isLoading && "animate-pulse opacity-60",
        className,
      )}
      role="group"
      aria-label="Section stats and filters"
    >
      <div className="flex flex-wrap items-center gap-2 p-3">
        {/* ── Pipeline status cluster ── */}
        <div className="flex items-center gap-2">
          {PIPELINE_PILLS.map((config) => (
            <StatPill
              key={config.key}
              config={config}
              value={stats[config.key] ?? 0}
              isActive={
                config.filter !== null && activeFilter === config.filter
              }
              onClick={() => handlePipelineClick(config.filter)}
            />
          ))}
        </div>

        <Separator orientation="vertical" className="mx-0.5 h-12 self-center" />

        {/* ── Health diagnostics cluster ── */}
        <div className="flex flex-1 flex-wrap items-center gap-2">
          {HEALTH_PILLS.map((config) => (
            <StatPill
              key={config.key}
              config={config}
              value={stats[config.key] ?? 0}
              isActive={false}
              compact
            />
          ))}
        </div>

        {/* ── Active filter clear chip ── */}
        {activeFilter !== "all" && (
          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                className="h-8 shrink-0 gap-1.5 text-muted-foreground"
                onClick={() => onFilterChange("all")}
                aria-label="Clear filter"
              >
                <span className="truncate text-xs">
                  {FILTER_LABELS[activeFilter]}
                </span>
                <X className="size-3.5" />
              </Button>
            </TooltipTrigger>
            <TooltipContent>Clear active filter</TooltipContent>
          </Tooltip>
        )}
      </div>
    </div>
  );
}
