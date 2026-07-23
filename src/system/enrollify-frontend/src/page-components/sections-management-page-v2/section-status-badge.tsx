import { Badge } from "@/components/ui/badge";
import { cn } from "@/lib/utils";
import {
  ClassSectionStatusEnum,
  type ClassSectionStatus,
} from "@/api/models/class-scheduling/class-section";

interface SectionStatusBadgeProps {
  status: ClassSectionStatus;
  className?: string;
  size?: "sm" | "default";
}

function getStatusStyles(statusValue: number): string {
  switch (statusValue) {
    case ClassSectionStatusEnum.Draft:
    case ClassSectionStatusEnum.PendingValidation:
    case ClassSectionStatusEnum.Validating:
      return "bg-secondary text-secondary-foreground border-secondary/50";
    case ClassSectionStatusEnum.Open:
      return "bg-emerald-100 text-emerald-800 border-emerald-200 dark:bg-emerald-900/30 dark:text-emerald-300 dark:border-emerald-800";
    case ClassSectionStatusEnum.Locked:
      return "bg-blue-100 text-blue-800 border-blue-200 dark:bg-blue-900/30 dark:text-blue-300 dark:border-blue-800";
    case ClassSectionStatusEnum.Active:
      return "bg-violet-100 text-violet-800 border-violet-200 dark:bg-violet-900/30 dark:text-violet-300 dark:border-violet-800";
    case ClassSectionStatusEnum.Completed:
      return "bg-slate-100 text-slate-700 border-slate-200 dark:bg-slate-800/50 dark:text-slate-300 dark:border-slate-700";
    case ClassSectionStatusEnum.Cancelled:
      return "bg-destructive/15 text-destructive border-destructive/20";
    default:
      return "bg-muted text-muted-foreground border-border";
  }
}

export function SectionStatusBadge({
  status,
  className,
  size = "default",
}: SectionStatusBadgeProps) {
  return (
    <Badge
      variant="outline"
      className={cn(
        "font-medium",
        size === "sm" && "text-xs px-1.5 py-0",
        getStatusStyles(status.value),
        className,
      )}
    >
      {status.name}
    </Badge>
  );
}
