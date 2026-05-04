import type { AcademicYear } from "@/api/models/academic-year";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { CalendarDays, Edit2, Clock } from "lucide-react";
import { formatDate } from "@/lib/date-utils";

interface ActiveAcademicYearTabProps {
  activeYear: AcademicYear | null;
  onEdit: () => void;
}

function DateField({ label, value }: { label: string; value?: string | null }) {
  return (
    <div className="space-y-1">
      <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
        {label}
      </p>
      <p className="text-sm font-medium">{formatDate(value)}</p>
    </div>
  );
}

export function ActiveAcademicYearTab({
  activeYear,
  onEdit,
}: ActiveAcademicYearTabProps) {
  if (!activeYear) {
    return (
      <div className="flex flex-col items-center justify-center py-20 gap-4 text-center">
        <div className="flex h-16 w-16 items-center justify-center rounded-full bg-muted">
          <CalendarDays className="h-8 w-8 text-muted-foreground" />
        </div>
        <div>
          <h3 className="text-lg font-semibold">No Active Academic Year</h3>
          <p className="text-sm text-muted-foreground mt-1">
            There is no active academic year at the moment. Use the{" "}
            <strong>Create Form</strong> tab to initiate one.
          </p>
        </div>
      </div>
    );
  }

  const sortedTerms = activeYear.academicTerms
    ?.slice()
    .sort((a, b) => (a.termNumber ?? 0) - (b.termNumber ?? 0));

  return (
    <div className="space-y-6 max-w-3xl">
      <Card>
        <CardHeader className="pb-3">
          <div className="flex items-start justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10">
                <CalendarDays className="h-5 w-5 text-primary" />
              </div>
              <div>
                <CardTitle className="text-xl">
                  {activeYear.academicYearTitle ??
                    `AY ${activeYear.startYear}–${activeYear.endYear}`}
                </CardTitle>
                <Badge variant="default" className="mt-1 text-xs">
                  Active
                </Badge>
              </div>
            </div>
            <Button
              variant="outline"
              size="sm"
              className="gap-2"
              onClick={onEdit}
            >
              <Edit2 className="size-4" />
              Edit
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-4">
            <DateField label="Start Date" value={activeYear.startDate} />
            <DateField label="End Date" value={activeYear.endDate} />
          </div>
        </CardContent>
      </Card>

      {sortedTerms && sortedTerms.length > 0 && (
        <div className="space-y-3">
          <h3 className="text-sm font-semibold flex items-center gap-2">
            <Clock className="size-4" />
            Academic Terms
          </h3>
          <div className="grid gap-3">
            {sortedTerms.map((term, idx) => (
              <Card
                key={term.id ?? idx}
                className="border-l-4 border-l-primary/30"
              >
                <CardContent className="pt-1 pb-1">
                  <div className="flex items-center gap-2 mb-3">
                    <Badge variant="secondary" className="text-xs font-mono">
                      Term {term.termNumber}
                    </Badge>
                    {term.termName && (
                      <span className="text-sm font-medium">
                        {term.termName}
                      </span>
                    )}
                  </div>
                  <Separator className="mb-3" />
                  <div className="grid grid-cols-2 gap-4">
                    <DateField label="Start Date" value={term.startDate} />
                    <DateField label="End Date" value={term.endDate} />
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
