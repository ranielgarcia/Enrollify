import type { AcademicYear } from "@/api/models/academic-year";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { CalendarDays, Edit2, ArrowRight } from "lucide-react";
import { formatDate } from "@/lib/date-utils";
import { cn } from "@/lib/utils";

interface ActiveAcademicYearTabProps {
  activeYear: AcademicYear | null;
  onEdit: () => void;
  createTabLabel: string;
}

function DateRange({
  start,
  end,
  size = "md",
}: {
  start?: string | null;
  end?: string | null;
  size?: "sm" | "md";
}) {
  return (
    <div
      className={cn(
        "flex items-center gap-2 text-muted-foreground",
        size === "sm" ? "text-xs" : "text-sm",
      )}
    >
      <span className="font-medium text-foreground">{formatDate(start)}</span>
      <ArrowRight className={cn(size === "sm" ? "size-3" : "size-3.5")} />
      <span className="font-medium text-foreground">{formatDate(end)}</span>
    </div>
  );
}

export function ActiveAcademicYearTab({
  activeYear,
  onEdit,
  createTabLabel,
}: ActiveAcademicYearTabProps) {
  if (!activeYear) {
    return (
      <div className="flex flex-col items-center justify-center py-24 gap-5 text-center">
        <div className="flex h-20 w-20 items-center justify-center rounded-2xl bg-muted/60 border border-dashed">
          <CalendarDays className="h-9 w-9 text-muted-foreground/60" />
        </div>
        <div className="space-y-1.5">
          <h3 className="text-base font-semibold tracking-tight">
            No Active Academic Year
          </h3>
          <p className="text-sm text-muted-foreground max-w-xs">
            There is no active academic year at the moment. Use the{" "}
            <strong className="text-foreground font-medium">
              {createTabLabel}
            </strong>{" "}
            tab to initiate one.
          </p>
        </div>
      </div>
    );
  }

  const sortedTerms = activeYear.academicTerms
    ?.slice()
    .sort((a, b) => (a.termNumber ?? 0) - (b.termNumber ?? 0));

  const title =
    activeYear.academicYearTitle ??
    `AY ${activeYear.startYear}–${activeYear.endYear}`;

  return (
    <div className="space-y-5 max-w-2xl">
      {/* Hero Card */}
      <div className="relative overflow-hidden rounded-xl border bg-linear-to-br from-primary/5 via-background to-background p-6 shadow-sm">
        {/* Decorative background blob */}
        <div className="pointer-events-none absolute -right-8 -top-8 h-40 w-40 rounded-full bg-primary/8 blur-2xl" />

        <div className="relative flex items-start justify-between gap-4">
          <div className="flex items-center gap-4">
            <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-primary/10 ring-1 ring-primary/20">
              <CalendarDays className="h-5 w-5 text-primary" />
            </div>
            <div className="space-y-1">
              <div className="flex items-center gap-2 flex-wrap">
                <h2 className="text-xl font-bold tracking-tight">{title}</h2>
                <Badge className="text-[11px] px-2 py-0.5 font-semibold bg-emerald-500/15 text-emerald-600 border border-emerald-500/25 hover:bg-emerald-500/15">
                  ● Active
                </Badge>
              </div>
              <DateRange
                start={activeYear.startDate}
                end={activeYear.endDate}
              />
            </div>
          </div>

          <Button
            variant="outline"
            size="sm"
            className="shrink-0 gap-1.5 shadow-sm"
            onClick={onEdit}
          >
            <Edit2 className="size-3.5" />
            Edit
          </Button>
        </div>
      </div>

      {/* Terms */}
      {sortedTerms && sortedTerms.length > 0 && (
        <div className="space-y-3">
          <p className="text-xs font-semibold uppercase tracking-widest text-muted-foreground px-0.5">
            Academic Terms
          </p>

          <div className="grid gap-2">
            {sortedTerms.map((term, idx) => (
              <div
                key={term.id ?? idx}
                className="group flex items-center justify-between rounded-lg border bg-card px-4 py-3 transition-colors hover:bg-accent/40"
              >
                <div className="flex items-center gap-3">
                  {/* Term number chip */}
                  <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-md bg-primary/10 text-xs font-bold text-primary">
                    {term.termNumber}
                  </span>
                  <div className="space-y-0.5">
                    <p className="text-sm font-medium leading-none">
                      {term.termName ?? `Term ${term.termNumber}`}
                    </p>
                    <DateRange
                      start={term.startDate}
                      end={term.endDate}
                      size="sm"
                    />
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
