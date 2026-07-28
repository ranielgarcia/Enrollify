import { AlertTriangle, CheckCircle2 } from "lucide-react";

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
      <span
        className={cn(
          "inline-flex items-center gap-1 rounded-full border border-primary/20 bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary",
          "dark:border-primary/30 dark:bg-primary/15 dark:text-primary",
          className,
        )}
      >
        <CheckCircle2 className="size-3" />
        No issues
      </span>
    );
  }

  const label = `${v.totalValidationIssues} issue${v.totalValidationIssues === 1 ? "" : "s"}`;
  const a11yLabel = `View ${v.totalValidationIssues} validation ${v.totalValidationIssues === 1 ? "issue" : "issues"}`;

  const chipClasses = cn(
    "inline-flex items-center gap-1 rounded-full border border-destructive/20 bg-destructive/10 px-2 py-0.5 text-xs font-medium text-destructive",
    "dark:border-destructive/30 dark:bg-destructive/15 dark:text-destructive",
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
        "transition-colors hover:bg-destructive/20 dark:hover:bg-destructive/20",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
      )}
    >
      <AlertTriangle className="size-3" />
      {label}
    </button>
  );
}
