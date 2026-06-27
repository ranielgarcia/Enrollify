import { Button } from "@/components/ui/button";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import {
  ClassSectionStatusEnum,
  type ClassSectionMinimal,
} from "@/api/models/class-scheduling/class-section";
import { ArrowUpRight, CheckCircle2, XCircle } from "lucide-react";
import { cn } from "@/lib/utils";

interface InlineTransitionButtonsProps {
  section: ClassSectionMinimal;
  onOpenSection: (section: ClassSectionMinimal) => void;
  onCancelSection: (section: ClassSectionMinimal) => void;
  onViewDetails: (section: ClassSectionMinimal) => void;
  isOpenPending?: boolean;
  isCancelPending?: boolean;
  size?: "sm" | "default";
  className?: string;
}

export function InlineTransitionButtons({
  section,
  onOpenSection,
  onCancelSection,
  onViewDetails,
  isOpenPending = false,
  isCancelPending = false,
  size = "sm",
  className,
}: InlineTransitionButtonsProps) {
  const isDraft = section.status.value === ClassSectionStatusEnum.Draft;
  const isOpen = section.status.value === ClassSectionStatusEnum.Open;
  const canOpen = isDraft;
  const canCancel = isDraft || isOpen;

  return (
    <div className={cn("flex items-center gap-1.5", className)}>
      {canOpen && (
        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              variant="outline"
              size={size}
              disabled={isOpenPending}
              onClick={() => onOpenSection(section)}
              className="gap-1.5 border-emerald-200 bg-emerald-50 text-emerald-700 hover:bg-emerald-100 hover:text-emerald-800 dark:border-emerald-800 dark:bg-emerald-950/30 dark:text-emerald-400 dark:hover:bg-emerald-900/30"
            >
              <CheckCircle2 className="size-3.5" />
              {size !== "sm" && "Open for Enrollment"}
            </Button>
          </TooltipTrigger>
          <TooltipContent>Open for enrollment</TooltipContent>
        </Tooltip>
      )}

      {canCancel && (
        <Tooltip>
          <TooltipTrigger asChild>
            <Button
              variant="outline"
              size={size}
              disabled={isCancelPending}
              onClick={() => onCancelSection(section)}
              className="gap-1.5 border-destructive/20 bg-destructive/5 text-destructive hover:bg-destructive/10 dark:border-destructive/30"
            >
              <XCircle className="size-3.5" />
              {size !== "sm" && "Cancel"}
            </Button>
          </TooltipTrigger>
          <TooltipContent>Cancel section</TooltipContent>
        </Tooltip>
      )}

      <Tooltip>
        <TooltipTrigger asChild>
          <Button
            variant="ghost"
            size={size}
            onClick={() => onViewDetails(section)}
            className="gap-1.5 text-muted-foreground hover:text-foreground"
          >
            <ArrowUpRight className="size-3.5" />
            {size !== "sm" && "Details"}
          </Button>
        </TooltipTrigger>
        <TooltipContent>View section details</TooltipContent>
      </Tooltip>
    </div>
  );
}
