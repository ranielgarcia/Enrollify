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
      return "bg-primary/10 text-primary border-primary/20";
    case ClassSectionStatusEnum.Locked:
      return "bg-primary/20 text-primary/80 border-primary/30";
    case ClassSectionStatusEnum.Active:
      return "bg-primary/30 text-primary border-primary/40";
    case ClassSectionStatusEnum.Completed:
      return "bg-muted text-muted-foreground border-border";
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
