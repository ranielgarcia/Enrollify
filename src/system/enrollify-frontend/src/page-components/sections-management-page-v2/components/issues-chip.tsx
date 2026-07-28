import { AlertTriangle, CheckCircle2 } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import type { ClassSectionMinimal } from "@/api/models/class-scheduling/class-section";
import { cn } from "@/lib/utils";

import { getValidationSummary } from "../lib/section-filters";

interface IssuesChipProps {
  section: ClassSectionMinimal;
  onClick?: () => void;
  className?: string;
}

/**
 * Compact validation-state chip rendered in a card header. Renders nothing if
 * the section has no offerings yet (avoids a misleading "No issues" pill on a
 * not-yet-initialized section). Clickable when an `onClick` is provided.
 */
export function IssuesChip({ section, onClick, className }: IssuesChipProps) {
  const v = getValidationSummary(section);
  if (!v || !v.hasOfferings) return null;

  if (!v.hasIssues) {
    return (
      <Badge
        variant="outline"
        className={cn(
          "gap-1 border-primary/30 text-primary dark:border-primary/40 dark:text-primary",
          className,
        )}
      >
        <CheckCircle2 className="size-3" />
        No issues
      </Badge>
    );
  }

  const label = `${v.totalValidationIssues} issue${v.totalValidationIssues === 1 ? "" : "s"}`;
  const a11yLabel = `View ${v.totalValidationIssues} validation ${v.totalValidationIssues === 1 ? "issue" : "issues"}`;

  const chipClasses = cn(
    "inline-flex items-center gap-1 rounded-full border border-amber-200 bg-amber-50 px-2 py-0.5 text-xs font-medium text-amber-700",
    "dark:border-amber-800 dark:bg-amber-950/30 dark:text-amber-400",
    className,
  );

  if (!onClick) {
    return (
      <span className={chipClasses}>
        <AlertTriangle className="size-3" />
        {label}
      </span>
    );
  }

  return (
    <button
      type="button"
      onClick={onClick}
      aria-label={a11yLabel}
      className={cn(
        chipClasses,
        "transition-colors hover:bg-amber-100 dark:hover:bg-amber-900/40",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
      )}
    >
      <AlertTriangle className="size-3" />
      {label}
    </button>
  );
}
