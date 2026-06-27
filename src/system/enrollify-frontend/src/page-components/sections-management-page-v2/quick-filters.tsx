import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { AlertTriangle, CheckCircle2, FileText, FolderOpen, LayoutList, Siren, XCircle } from "lucide-react";
import type { QuickFilter } from "./searchParams";

interface QuickFilterConfig {
  key: QuickFilter;
  label: string;
  icon: React.ReactNode;
  description: string;
}

const QUICK_FILTERS: QuickFilterConfig[] = [
  {
    key: "all",
    label: "All",
    icon: <LayoutList className="size-3.5" />,
    description: "Show all sections",
  },
  {
    key: "draft",
    label: "Draft",
    icon: <FileText className="size-3.5" />,
    description: "Sections awaiting scheduling",
  },
  {
    key: "open",
    label: "Open",
    icon: <FolderOpen className="size-3.5" />,
    description: "Sections open for enrollment",
  },
  {
    key: "cancelled",
    label: "Cancelled",
    icon: <XCircle className="size-3.5" />,
    description: "Cancelled sections",
  },
  {
    key: "errors",
    label: "Unresolved Errors",
    icon: <AlertTriangle className="size-3.5" />,
    description: "Draft sections with error-severity validation issues",
  },
  {
    key: "conflicts",
    label: "Conflicts",
    icon: <Siren className="size-3.5" />,
    description: "Draft sections with scheduling conflicts",
  },
  {
    key: "needs-attention",
    label: "Needs Attention",
    icon: <CheckCircle2 className="size-3.5" />,
    description: "Draft sections with errors or conflicts",
  },
];

interface QuickFiltersProps {
  activeFilter: QuickFilter;
  onFilterChange: (filter: QuickFilter) => void;
  className?: string;
}

export function QuickFilters({
  activeFilter,
  onFilterChange,
  className,
}: QuickFiltersProps) {
  return (
    <div
      className={cn("flex items-center gap-1.5 flex-wrap", className)}
      role="group"
      aria-label="Quick filters"
    >
      <span className="text-xs font-medium text-muted-foreground shrink-0">
        Filter:
      </span>
      {QUICK_FILTERS.map((filter) => {
        const isActive = activeFilter === filter.key;
        return (
          <Button
            key={filter.key}
            variant={isActive ? "default" : "outline"}
            size="sm"
            onClick={() => onFilterChange(filter.key)}
            className={cn(
              "h-7 gap-1.5 text-xs font-medium transition-all",
              isActive
                ? "shadow-sm"
                : "text-muted-foreground hover:text-foreground",
              filter.key === "errors" &&
                isActive &&
                "bg-amber-600 hover:bg-amber-700 border-amber-700",
              filter.key === "conflicts" &&
                isActive &&
                "bg-rose-600 hover:bg-rose-700 border-rose-700",
              filter.key === "needs-attention" &&
                isActive &&
                "bg-orange-600 hover:bg-orange-700 border-orange-700",
            )}
            title={filter.description}
          >
            {filter.icon}
            {filter.label}
          </Button>
        );
      })}
    </div>
  );
}
