import {
  AlertOctagon,
  AlertTriangle,
  Clock,
  FileText,
  FolderOpen,
  Siren,
  X,
  XCircle,
  type LucideIcon,
} from "lucide-react";

import { Badge } from "@/components/ui/badge";
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
  DataInconsistencies: "DATA_INCONSISTENCIES",
  ScheduleConflicts: "SCHEDULE_CONFLICTS",
  MissingRequirements: "MISSING_REQUIREMENTS",
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
    key: STATS_KEYS.DataInconsistencies,
    label: "Errors",
    icon: AlertTriangle,
    filter: null,
    tone: "warn",
    hint: "Validation errors that block opening for enrollment",
  },
  {
    key: STATS_KEYS.ScheduleConflicts,
    label: "Conflicts",
    icon: Siren,
    filter: null,
    tone: "danger",
    hint: "Schedule conflicts across offerings",
  },
  {
    key: STATS_KEYS.MissingRequirements,
    label: "Unscheduled",
    icon: Clock,
    filter: null,
    tone: "info",
    hint: "Offerings missing teacher, schedule, or room",
  },
];

const TONE_CLASSES: Record<
  Tone,
  { icon: string; iconActive: string; ring: string }
> = {
  neutral: {
    icon: "text-muted-foreground bg-muted",
    iconActive: "text-foreground bg-secondary",
    ring: "ring-secondary",
  },
  success: {
    icon: "text-emerald-600 dark:text-emerald-400 bg-emerald-50 dark:bg-emerald-950/40",
    iconActive:
      "text-emerald-700 dark:text-emerald-300 bg-emerald-100 dark:bg-emerald-900/60",
    ring: "ring-emerald-300 dark:ring-emerald-700",
  },
  danger: {
    icon: "text-rose-600 dark:text-rose-400 bg-rose-50 dark:bg-rose-950/40",
    iconActive:
      "text-rose-700 dark:text-rose-300 bg-rose-100 dark:bg-rose-900/60",
    ring: "ring-rose-300 dark:ring-rose-700",
  },
  warn: {
    icon: "text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-950/40",
    iconActive:
      "text-amber-700 dark:text-amber-300 bg-amber-100 dark:bg-amber-900/60",
    ring: "ring-amber-300 dark:ring-amber-700",
  },
  info: {
    icon: "text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-950/40",
    iconActive:
      "text-blue-700 dark:text-blue-300 bg-blue-100 dark:bg-blue-900/60",
    ring: "ring-blue-300 dark:ring-blue-700",
  },
};

interface StatPillProps {
  config: PillConfig;
  value: number;
  isActive: boolean;
  onClick?: () => void;
}

function StatPill({ config, value, isActive, onClick }: StatPillProps) {
  const Icon = config.icon;
  const isClickable = config.filter !== null && !!onClick;
  const tone = TONE_CLASSES[config.tone];

  const inner = (
    <>
      <div
        className={cn(
          "flex size-9 shrink-0 items-center justify-center rounded-md transition-colors",
          isActive ? tone.iconActive : tone.icon,
        )}
      >
        <Icon className="size-4" />
      </div>
      <div className="min-w-0">
        <div className="text-[10px] font-semibold uppercase tracking-wide text-muted-foreground leading-none">
          {config.label}
        </div>
        <div className="mt-1 text-xl font-bold leading-none tabular-nums">
          {value.toLocaleString()}
        </div>
      </div>
    </>
  );

  const baseClasses = cn(
    "flex items-center gap-3 rounded-lg border bg-card px-3 py-2.5 text-left transition-all min-w-36",
    "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1",
    isActive && "ring-1 ring-offset-1",
    isActive && tone.ring,
  );

  if (isClickable) {
    return (
      <Button
        type="button"
        variant="ghost"
        onClick={onClick}
        aria-pressed={isActive}
        title={config.hint}
        className={cn(baseClasses, "h-auto cursor-pointer hover:bg-muted/40")}
      >
        {inner}
      </Button>
    );
  }

  if (config.hint) {
    return (
      <Tooltip>
        <TooltipTrigger asChild>
          <div className={cn(baseClasses, "cursor-default")}>{inner}</div>
        </TooltipTrigger>
        <TooltipContent>{config.hint}</TooltipContent>
      </Tooltip>
    );
  }

  return <div className={cn(baseClasses, "cursor-default")}>{inner}</div>;
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
  "needs-attention": "Needs attention",
};

/**
 * Consolidated stats + filter strip for the sections page.
 * - Pipeline cluster (Draft / Open / Cancelled) — clickable status filters.
 * - Health cluster (Errors / Conflicts / Unscheduled) — display-only counters.
 * - "Needs attention" toggle button — single source of truth for filtering on
 *   any offerings-with-issues.
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

  const needsAttentionActive = activeFilter === "needs-attention";
  const needsAttentionCount =
    (stats[STATS_KEYS.DataInconsistencies] ?? 0) +
    (stats[STATS_KEYS.ScheduleConflicts] ?? 0);

  return (
    <div
      className={cn(
        "rounded-lg border bg-card",
        isLoading && "animate-pulse opacity-60",
        className,
      )}
      role="group"
      aria-label="Section stats and filters"
    >
      <div className="flex flex-wrap items-stretch gap-2 p-2">
        {PIPELINE_PILLS.map((config) => (
          <StatPill
            key={config.key}
            config={config}
            value={stats[config.key] ?? 0}
            isActive={config.filter !== null && activeFilter === config.filter}
            onClick={() => handlePipelineClick(config.filter)}
          />
        ))}

        <Separator orientation="vertical" className="mx-1 h-auto" />

        {HEALTH_PILLS.map((config) => (
          <StatPill
            key={config.key}
            config={config}
            value={stats[config.key] ?? 0}
            isActive={false}
          />
        ))}

        <div className="ml-auto flex items-center gap-2 self-center">
          <Button
            type="button"
            variant={needsAttentionActive ? "default" : "outline"}
            size="sm"
            aria-pressed={needsAttentionActive}
            onClick={() =>
              onFilterChange(needsAttentionActive ? "all" : "needs-attention")
            }
            className={cn(
              "h-9 gap-1.5",
              needsAttentionActive &&
                "border-amber-700 bg-amber-600 text-white hover:bg-amber-700",
            )}
          >
            <AlertOctagon className="size-3.5" />
            Needs attention
            {needsAttentionCount > 0 && (
              <Badge
                variant={needsAttentionActive ? "secondary" : "outline"}
                className="ml-1 h-5 px-1.5 font-semibold tabular-nums"
              >
                {needsAttentionCount.toLocaleString()}
              </Badge>
            )}
          </Button>

          {activeFilter !== "all" && (
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  className="h-9 gap-1.5 text-muted-foreground"
                  onClick={() => onFilterChange("all")}
                  aria-label="Clear filter"
                >
                  <span className="truncate">
                    Filter: {FILTER_LABELS[activeFilter]}
                  </span>
                  <X className="size-3.5" />
                </Button>
              </TooltipTrigger>
              <TooltipContent>Clear active filter</TooltipContent>
            </Tooltip>
          )}
        </div>
      </div>
    </div>
  );
}
