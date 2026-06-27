import { cn } from "@/lib/utils";
import {
  AlertTriangle,
  Clock,
  FileText,
  FolderOpen,
  Siren,
  XCircle,
  type LucideIcon,
} from "lucide-react";
import type { QuickFilter } from "./searchParams";

const STATS_KEYS = {
  OpenSections: "OPEN_SECTIONS",
  DraftSections: "DRAFT_SECTIONS",
  CancelledSections: "CANCELLED_SECTIONS",
  ScheduleConflicts: "SCHEDULE_CONFLICTS",
  DataInconsistencies: "DATA_INCONSISTENCIES",
  MissingRequirements: "MISSING_REQUIREMENTS",
  CapacityConstraints: "CAPACITY_CONSTRAINTS",
  ResourceMisalignments: "RESOURCE_MISALIGNMENTS",
  SchedulePolicyViolations: "SCHEDULE_POLICY_VIOLATIONS",
  DefaultValues: "DEFAULT_VALUES",
} as const;

interface StatConfig {
  key: string;
  label: string;
  icon: LucideIcon;
  filter: QuickFilter | null;
  colorClass: string;
  activeColorClass: string;
}

const STAT_CONFIGS: StatConfig[] = [
  {
    key: STATS_KEYS.DraftSections,
    label: "Draft",
    icon: FileText,
    filter: "draft",
    colorClass:
      "text-secondary-foreground hover:bg-secondary/80",
    activeColorClass:
      "bg-secondary text-secondary-foreground ring-2 ring-secondary/50 ring-offset-1",
  },
  {
    key: STATS_KEYS.OpenSections,
    label: "Open",
    icon: FolderOpen,
    filter: "open",
    colorClass:
      "text-emerald-700 dark:text-emerald-400 hover:bg-emerald-50 dark:hover:bg-emerald-900/20",
    activeColorClass:
      "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-300 ring-2 ring-emerald-300 dark:ring-emerald-700 ring-offset-1",
  },
  {
    key: STATS_KEYS.CancelledSections,
    label: "Cancelled",
    icon: XCircle,
    filter: "cancelled",
    colorClass:
      "text-destructive hover:bg-destructive/5",
    activeColorClass:
      "bg-destructive/15 text-destructive ring-2 ring-destructive/30 ring-offset-1",
  },
  {
    key: STATS_KEYS.DataInconsistencies,
    label: "Unresolved Errors",
    icon: AlertTriangle,
    filter: "errors",
    colorClass:
      "text-amber-700 dark:text-amber-400 hover:bg-amber-50 dark:hover:bg-amber-900/20",
    activeColorClass:
      "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300 ring-2 ring-amber-300 dark:ring-amber-700 ring-offset-1",
  },
  {
    key: STATS_KEYS.ScheduleConflicts,
    label: "Conflicts",
    icon: Siren,
    filter: "conflicts",
    colorClass:
      "text-rose-700 dark:text-rose-400 hover:bg-rose-50 dark:hover:bg-rose-900/20",
    activeColorClass:
      "bg-rose-100 text-rose-800 dark:bg-rose-900/30 dark:text-rose-300 ring-2 ring-rose-300 dark:ring-rose-700 ring-offset-1",
  },
  {
    key: STATS_KEYS.MissingRequirements,
    label: "Unscheduled",
    icon: Clock,
    filter: null,
    colorClass:
      "text-blue-700 dark:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900/20",
    activeColorClass:
      "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300 ring-2 ring-blue-300 dark:ring-blue-700 ring-offset-1",
  },
];

interface StatPillProps {
  config: StatConfig;
  value: number;
  isActive: boolean;
  onClick: () => void;
}

function StatPill({ config, value, isActive, onClick }: StatPillProps) {
  const Icon = config.icon;
  const isClickable = config.filter !== null;

  return (
    <button
      type="button"
      onClick={isClickable ? onClick : undefined}
      disabled={!isClickable}
      className={cn(
        "flex items-center gap-2.5 rounded-lg border bg-card px-4 py-3 text-left transition-all",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1",
        isClickable
          ? "cursor-pointer"
          : "cursor-default",
        isActive
          ? config.activeColorClass
          : [
              "border-border",
              isClickable && config.colorClass,
            ],
      )}
    >
      <div
        className={cn(
          "flex size-8 shrink-0 items-center justify-center rounded-md",
          isActive ? "bg-white/30 dark:bg-black/20" : "bg-muted",
        )}
      >
        <Icon className="size-4" />
      </div>
      <div className="min-w-0">
        <div className="text-xs font-medium leading-none text-current/70 mb-1">
          {config.label}
        </div>
        <div className="text-xl font-bold leading-none tabular-nums">
          {value.toLocaleString()}
        </div>
      </div>
    </button>
  );
}

interface StatsBarProps {
  stats: Record<string, number>;
  activeFilter: QuickFilter;
  onFilterChange: (filter: QuickFilter) => void;
  isLoading?: boolean;
}

export function StatsBar({
  stats,
  activeFilter,
  onFilterChange,
  isLoading = false,
}: StatsBarProps) {
  const handleStatClick = (filter: QuickFilter | null) => {
    if (!filter) return;
    if (activeFilter === filter) {
      onFilterChange("all");
    } else {
      onFilterChange(filter);
    }
  };

  return (
    <div className="grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-6">
      {STAT_CONFIGS.map((config) => (
        <div
          key={config.key}
          className={cn(isLoading && "animate-pulse opacity-60")}
        >
          <StatPill
            config={config}
            value={stats[config.key] ?? 0}
            isActive={config.filter !== null && activeFilter === config.filter}
            onClick={() => handleStatClick(config.filter)}
          />
        </div>
      ))}
    </div>
  );
}


