import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { Building2, CalendarClock, FilterX, SearchX } from "lucide-react";

type EmptyStateVariant = "no-college" | "no-sections" | "filtered-empty";

interface EmptyStateProps {
  variant: EmptyStateVariant;
  onClearFilters?: () => void;
  onOpenCollegeSelector?: () => void;
  className?: string;
}

const config: Record<
  EmptyStateVariant,
  {
    icon: React.ReactNode;
    title: string;
    description: string;
  }
> = {
  "no-college": {
    icon: <Building2 className="size-10 text-muted-foreground/50" />,
    title: "Select a College",
    description:
      "Choose a college from the selector above to view and manage its class sections.",
  },
  "no-sections": {
    icon: <CalendarClock className="size-10 text-muted-foreground/50" />,
    title: "No Sections Found",
    description:
      "This college doesn't have any class sections for the selected academic term yet.",
  },
  "filtered-empty": {
    icon: <SearchX className="size-10 text-muted-foreground/50" />,
    title: "No Matching Sections",
    description:
      "No sections match the current filter. Try adjusting or clearing the active filters.",
  },
};

export function EmptyState({
  variant,
  onClearFilters,
  onOpenCollegeSelector,
  className,
}: EmptyStateProps) {
  const { icon, title, description } = config[variant];

  return (
    <div
      className={cn(
        "flex flex-col items-center justify-center gap-4 rounded-lg border border-dashed bg-muted/30 px-6 py-16 text-center",
        className,
      )}
    >
      <div className="flex size-16 items-center justify-center rounded-full bg-muted">
        {icon}
      </div>
      <div className="space-y-1">
        <h3 className="text-base font-semibold">{title}</h3>
        <p className="max-w-sm text-sm text-muted-foreground">{description}</p>
      </div>
      {variant === "no-college" && onOpenCollegeSelector && (
        <Button size="sm" onClick={onOpenCollegeSelector} className="gap-2">
          <Building2 className="size-4" />
          Open college selector
        </Button>
      )}
      {variant === "filtered-empty" && onClearFilters && (
        <Button
          variant="outline"
          size="sm"
          onClick={onClearFilters}
          className="gap-2"
        >
          <FilterX className="size-4" />
          Clear Filters
        </Button>
      )}
    </div>
  );
}
