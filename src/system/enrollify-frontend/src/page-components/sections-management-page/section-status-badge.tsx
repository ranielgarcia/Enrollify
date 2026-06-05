import { Badge } from "@/components/ui/badge";
import {
  ClassSectionStatusEnum,
  type ClassSectionStatus,
} from "@/api/models/class-section";
import { cn } from "@/lib/utils";

interface SectionStatusBadgeProps {
  status: ClassSectionStatus;
  className?: string;
}

const STATUS_STYLES: Record<number, string> = {
  [ClassSectionStatusEnum.Draft]:
    "bg-secondary text-secondary-foreground hover:bg-secondary/80",
  [ClassSectionStatusEnum.Open]:
    "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400",
  [ClassSectionStatusEnum.Locked]:
    "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-400",
  [ClassSectionStatusEnum.Active]:
    "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400",
  [ClassSectionStatusEnum.Completed]:
    "bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-400",
  [ClassSectionStatusEnum.Cancelled]: "bg-destructive/15 text-destructive",
};

export function SectionStatusBadge({
  status,
  className,
}: SectionStatusBadgeProps) {
  return (
    <Badge
      variant="secondary"
      className={cn(STATUS_STYLES[status.value], "font-medium", className)}
    >
      {status.name}
    </Badge>
  );
}
