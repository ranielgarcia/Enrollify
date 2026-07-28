import {
  FileText,
  FolderOpen,
  X,
  XCircle,
  type LucideIcon,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { cn } from "@/lib/utils";

import type { QuickFilter } from "./searchParams";

interface PipelineStatConfig {
  key: string;
  label: string;
  icon: LucideIcon;
  filter: QuickFilter;
  hint: string;
}

const PIPELINE_STATS: PipelineStatConfig[] = [
  {
    key: "DRAFT_SECTIONS",
    label: "Draft",
    icon: FileText,
    filter: "draft",
    hint: "Sections not yet open for enrollment",
  },
  {
    key: "OPEN_SECTIONS",
    label: "Open",
    icon: FolderOpen,
    filter: "open",
    hint: "Sections open for enrollment",
  },
  {
    key: "CANCELLED_SECTIONS",
    label: "Cancelled",
    icon: XCircle,
    filter: "cancelled",
    hint: "Cancelled this term",
  },
];

interface PipelineStatCardProps {
  config: PipelineStatConfig;
  value: number;
  isActive: boolean;
  onClick: () => void;
  fullWidth?: boolean;
}

function PipelineStatCard({
  config,
  value,
  isActive,
  onClick,
  fullWidth = false,
}: PipelineStatCardProps) {
  const Icon = config.icon;

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <button
          type="button"
          onClick={onClick}
          aria-pressed={isActive ? "true" : "false"}
          className={cn(
            "flex min-w-[100px] flex-col items-start rounded-lg border bg-card px-4 py-3 text-left transition-all",
            "hover:bg-muted/40 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1",
            fullWidth && "w-full",
            isActive
              ? "border-primary/40 bg-primary/5 ring-1 ring-primary/30"
              : "border-border",
          )}
        >
          <div className="flex w-full items-center justify-between gap-2">
            <Icon
              className={cn(
                "size-3.5 shrink-0",
                isActive ? "text-primary" : "text-muted-foreground/60",
              )}
            />
            <span
              className={cn(
                "text-2xl font-semibold leading-none tabular-nums",
                isActive ? "text-primary" : "text-foreground",
              )}
            >
              {value.toLocaleString()}
            </span>
          </div>
          <span
            className={cn(
              "mt-1.5 text-[10px] font-medium uppercase tracking-wider",
              isActive ? "text-primary/80" : "text-muted-foreground",
            )}
          >
            {config.label}
          </span>
        </button>
      </TooltipTrigger>
      <TooltipContent>{config.hint}</TooltipContent>
    </Tooltip>
  );
}

interface SectionStatsStripProps {
  stats: Record<string, number>;
  activeFilter: QuickFilter;
  onFilterChange: (filter: QuickFilter) => void;
  isLoading?: boolean;
  orientation?: "horizontal" | "vertical";
  className?: string;
}

/**
 * Minimal pipeline status strip for the sections page.
 * Shows Draft / Open / Cancelled section counts as clickable filter cards.
 * Supports a horizontal strip (default) or a vertical stacked layout for use
 * inside the health sidebar.
 */
export function SectionStatsStrip({
  stats,
  activeFilter,
  onFilterChange,
  isLoading = false,
  orientation = "horizontal",
  className,
}: SectionStatsStripProps) {
  const handleClick = (filter: QuickFilter) => {
    onFilterChange(activeFilter === filter ? "all" : filter);
  };

  const isVertical = orientation === "vertical";

  if (isVertical) {
    return (
      <div
        className={cn(
          "flex flex-col gap-2",
          isLoading && "animate-pulse opacity-60",
          className,
        )}
      >
        <div className="flex items-center justify-between gap-2 px-0.5 pb-1">
          <span className="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground">
            Pipeline
          </span>
          {activeFilter !== "all" && (
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  className="-mr-1 h-5 gap-1 px-1.5 text-[10px] text-muted-foreground"
                  onClick={() => onFilterChange("all")}
                  aria-label="Clear filter"
                >
                  Clear
                  <X className="size-2.5" />
                </Button>
              </TooltipTrigger>
              <TooltipContent>Clear active filter</TooltipContent>
            </Tooltip>
          )}
        </div>

        <div
          className="flex flex-col gap-2"
          role="group"
          aria-label="Section status filters"
        >
          {PIPELINE_STATS.map((config) => (
            <PipelineStatCard
              key={config.key}
              config={config}
              value={stats[config.key] ?? 0}
              isActive={activeFilter === config.filter}
              onClick={() => handleClick(config.filter)}
              fullWidth
            />
          ))}
        </div>
      </div>
    );
  }

  return (
    <div
      className={cn(
        "flex items-center gap-2",
        isLoading && "animate-pulse opacity-60",
        className,
      )}
      role="group"
      aria-label="Section status filters"
    >
      {PIPELINE_STATS.map((config) => (
        <PipelineStatCard
          key={config.key}
          config={config}
          value={stats[config.key] ?? 0}
          isActive={activeFilter === config.filter}
          onClick={() => handleClick(config.filter)}
        />
      ))}

      {/* Active-filter clear chip */}
      {activeFilter !== "all" && (
        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              type="button"
              variant="ghost"
              size="sm"
              className="ml-1 h-8 gap-1 text-xs text-muted-foreground"
              onClick={() => onFilterChange("all")}
              aria-label="Clear filter"
            >
              Clear
              <X className="size-3" />
            </Button>
          </TooltipTrigger>
          <TooltipContent>Clear active filter</TooltipContent>
        </Tooltip>
      )}
    </div>
  );
}
