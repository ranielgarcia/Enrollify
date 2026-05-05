import type { AcademicYear } from "@/api/models/academic-year";
import { Badge } from "@/components/ui/badge";
import { CalendarDays, ArrowRight } from "lucide-react";
import { formatDate } from "@/lib/date-utils";
import { cn } from "@/lib/utils";
import { Separator } from "@/components/ui/separator";
import { Fragment } from "react/jsx-runtime";

interface PreviousAcademicYearsTabProps {
  previousYears: AcademicYear[];
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

export function PreviousAcademicYearsTab({
  previousYears,
}: PreviousAcademicYearsTabProps) {
  if (previousYears.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-24 gap-5 text-center">
        <div className="flex h-20 w-20 items-center justify-center rounded-2xl bg-muted/60 border border-dashed">
          <CalendarDays className="h-9 w-9 text-muted-foreground/60" />
        </div>
        <div className="space-y-1.5">
          <h3 className="text-base font-semibold tracking-tight">
            No Previous Academic Years
          </h3>
          <p className="text-sm text-muted-foreground max-w-xs">
            Previous academic years will appear here once they become inactive.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-5 max-w-2xl">
      {previousYears.map((year) => {
        const sortedTerms = (year.academicTerms ?? [])
          .slice()
          .sort((a, b) => (a.termNumber ?? 0) - (b.termNumber ?? 0));

        const title =
          year.academicYearTitle ?? `AY ${year.startYear}–${year.endYear}`;

        return (
          <Fragment key={year.id}>
            <div className="space-y-3">
              {/* Hero Card — inactive style */}
              <div className="relative overflow-hidden rounded-xl border bg-linear-to-br from-muted/30 via-background to-background p-6 shadow-sm">
                <div className="pointer-events-none absolute -right-8 -top-8 h-40 w-40 rounded-full bg-muted/40 blur-2xl" />

                <div className="relative flex items-start justify-between gap-4">
                  <div className="flex items-center gap-4">
                    <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-xl bg-muted/60 ring-1 ring-border">
                      <CalendarDays className="h-5 w-5 text-muted-foreground" />
                    </div>
                    <div className="space-y-1">
                      <div className="flex items-center gap-2 flex-wrap">
                        <h2 className="text-xl font-bold tracking-tight">
                          {title}
                        </h2>
                        <Badge className="text-[11px] px-2 py-0.5 font-semibold bg-muted text-muted-foreground border hover:bg-muted">
                          ● Inactive
                        </Badge>
                      </div>
                      <DateRange start={year.startDate} end={year.endDate} />
                    </div>
                  </div>
                </div>
              </div>

              {/* Terms */}
              {sortedTerms.length > 0 && (
                <div className="space-y-3 pl-4">
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
            <Separator />
          </Fragment>
        );
      })}
    </div>
  );
}
