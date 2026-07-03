import type { LucideIcon } from "lucide-react";
import type { ReactNode } from "react";

import { cn } from "@/lib/utils";

interface MetaRowProps {
  icon: LucideIcon;
  label: string;
  value: ReactNode;
  valueAs?: "text" | "custom";
  valueClassName?: string;
  action?: ReactNode;
  className?: string;
}

/**
 * Compact label/value row used in the section card meta grid. `valueAs="custom"`
 * lets a caller drop a richer node (e.g. a progress bar) into the value slot
 * while keeping the same icon + label rail.
 */
export function MetaRow({
  icon: Icon,
  label,
  value,
  valueAs = "text",
  valueClassName,
  action,
  className,
}: MetaRowProps) {
  return (
    <div className={cn("flex min-w-0 items-center gap-2", className)}>
      <Icon className="size-3.5 shrink-0 text-muted-foreground" />
      <span className="shrink-0 text-[10px] font-semibold uppercase tracking-wide text-muted-foreground">
        {label}
      </span>
      {valueAs === "custom" ? (
        <div className="flex min-w-0 flex-1 items-center">{value}</div>
      ) : (
        <span
          className={cn(
            "min-w-0 flex-1 truncate text-xs font-medium text-foreground",
            valueClassName,
          )}
        >
          {value}
        </span>
      )}
      {action}
    </div>
  );
}
